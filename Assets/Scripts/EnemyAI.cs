// EnemyScript.cs — FIX: Animación de muerte + NullReference + Estados
//
// FIX 1: Morir() — La animación de muerte no se ejecutaba porque
// this.enabled = false desactivaba el Animator ANTES del trigger.
// Además playerCombat era null al destruir desde GameManager.
//
// FIX 2: IsAttackable() — Los enemigos inactivos (dormidos atrapados
// en bucle) se contaban como "vivos" bloqueando la colmena.
//
// FIX 3: HitCoroutine — Corrutina interrumpida dejaba isStunned=true.
//
// FIX 4: isLockedTarget — Nunca se liberaba si enemigo moría.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class EnemyScript : Damageable
{
    // Declarations
    public Animator animator;
    public SimpleWalk playerCombat;
    public EnemyManager enemyManager;
    public EnemyDetection enemyDetection;
    public CharacterController characterController;

    [Header("Stats")]
    private float moveSpeed = 1;
    private Vector3 moveDirection;

    [Header("States")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isRetreating;
    [SerializeField] public bool isLockedTarget;
    [SerializeField] public bool isStunned;
    [SerializeField] public bool isWaiting = true;

    [Header("Polish")]
    private Coroutine PrepareAttackCoroutine;
    private Coroutine RetreatCoroutine;
    private Coroutine DamageCoroutine;
    public Coroutine MovementCoroutine;
    private Coroutine DeathCoroutine;
    private Coroutine lockTimerCoroutine;

    private Coroutine sleepCoroutine;
    private Coroutine confusedCoroutine;

    public UnityEvent<EnemyScript> OnDamage;
    public UnityEvent<EnemyScript> OnStopMoving;
    public UnityEvent<EnemyScript> OnRetreat;
    public int danyoAtaque = 10;
    public float XPEarned = 5f;
    [SerializeField] public EnemyHitbox enemyHitbox;

    private const bool LOG_IA = false;

    // ── Estado interno de vida real ─────────────────────────
    // FIX: Distinto de currentHealth — este es "¿puede hacer algo?"
    private bool _reallyDead = false;

    // ══════════════════════════════════════════════════════════
    // START
    // ══════════════════════════════════════════════════════════

    public virtual void Start()
    {
        enemyManager = GetComponentInParent<EnemyManager>();
        if (enemyManager == null)
            Debug.LogWarning($"[EnemyScript] '{name}' sin EnemyManager en padre.");

        base.InicializarVida();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        playerCombat = FindAnyObjectByType<SimpleWalk>();
        if (playerCombat != null)
        {
            enemyDetection = playerCombat.GetComponentInChildren<EnemyDetection>();
            playerCombat.OnCounterAttack.AddListener((x) => OnPlayerCounter(x));
            playerCombat.OnTrajectory.AddListener((x) => OnPlayerTrajectory(x));
        }

        if (enemyManager != null)
            enemyManager.RegisterAllEnemiesInChildren();

        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    // ══════════════════════════════════════════════════════════
    // FIX: IsAttackable — NO contar enemigos que no pueden actuar
    // ══════════════════════════════════════════════════════════

    public bool IsAttackable()
    {
        if (_reallyDead) return false;
        if (currentHealth <= 0) return false;
        // FIX: Si está inactivo/no disponible, no cuenta
        if (!isActiveAndEnabled) return false;
        if (EstaDormido() && sleepCoroutine != null) return false;  // Dormido de verdad
        return true;
    }

    // ══════════════════════════════════════════════════════════
    // MAIN UPDATE
    // ══════════════════════════════════════════════════════════

    void Update()
    {
        if (EstaConfuso())
        {
            // No mirar al jugador si está confuso
        }
        else
        {
            if (playerCombat != null)
                transform.LookAt(new Vector3(
                    playerCombat.transform.position.x,
                    transform.position.y,
                    playerCombat.transform.position.z));
        }

        MoveEnemy(moveDirection);
        base.AvanzarCooldown();
        if (estaEnvenenado) ProcesarVenenoPorTiempo();
    }

    // ══════════════════════════════════════════════════════════
    // SLEEP
    // ══════════════════════════════════════════════════════════

    public void ActivarSleep()
    {
        if (sleepCoroutine != null) StopCoroutine(sleepCoroutine);
        sleepCoroutine = StartCoroutine(SleepCoroutine());
    }

    IEnumerator SleepCoroutine()
    {
        StopEnemyCoroutines();
        StopMoving();
        if (animator != null) animator.SetTrigger("Sleep");

        while (EstaDormido()) yield return null;

        // Despertó
        if (animator != null) animator.SetTrigger("Idle");
        sleepCoroutine = null;
        isWaiting = true;
        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    // ══════════════════════════════════════════════════════════
    // CONFUSED
    // ══════════════════════════════════════════════════════════

    public void ActivarConfused()
    {
        if (confusedCoroutine != null) StopCoroutine(confusedCoroutine);
        confusedCoroutine = StartCoroutine(ConfusedCoroutine());
    }

    IEnumerator ConfusedCoroutine()
    {
        StopEnemyCoroutines();
        StopMoving();

        Vector3 dirToPlayer = (playerCombat.transform.position - transform.position).normalized;
        Vector3 backDir = -dirToPlayer;
        backDir.y = 0;
        if (backDir != Vector3.zero) transform.forward = backDir;

        if (animator != null) animator.SetTrigger("AirPunch");

        while (EstaConfuso()) yield return null;

        if (animator != null) animator.SetTrigger("Idle");
        confusedCoroutine = null;
        isWaiting = true;
        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    // ══════════════════════════════════════════════════════════
    // FIX: TAKEDAMAGE — Liberar lock y stun properly
    // ══════════════════════════════════════════════════════════

    public override void TakeDamage(int amount, DamageType damageType, GameObject source)
    {
        base.TakeDamage(amount, damageType, source);

        if (currentHealth <= 0) return;

        // Parar estados especiales si estaban activos
        if (sleepCoroutine != null) { StopCoroutine(sleepCoroutine); sleepCoroutine = null; }
        if (confusedCoroutine != null) { StopCoroutine(confusedCoroutine); confusedCoroutine = null; }

        // FIX: Liberar lock y stun AL RECIBIR DAÑO
        isStunned = false;
        isLockedTarget = false;

        // Parar hit coroutine anterior si estaba activa
        if (DamageCoroutine != null) { StopCoroutine(DamageCoroutine); DamageCoroutine = null; }

        StopEnemyCoroutines();

        if (enemyDetection != null) enemyDetection.SetCurrentTarget(null);

        OnDamage?.Invoke(this);

        if (animator != null) animator.SetTrigger("Hit");

        transform.DOMove(transform.position - (transform.forward / 2), .3f).SetDelay(.1f);
        StopMoving();

        DamageCoroutine = StartCoroutine(HitCoroutine());
    }

    // ══════════════════════════════════════════════════════════
    // FIX: HIT COROUTINE — SIEMPRE desbloquear isStunned
    // ══════════════════════════════════════════════════════════

    IEnumerator HitCoroutine()
    {
        isStunned = true;

        float stunTime = 0.5f;
        float elapsed = 0f;

        while (elapsed < stunTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // CRITICAL FIX: isStunned SIEMPRE vuelve a false
        isStunned = false;

        // Corrutina de movimiento puede reiniciarse
        if (currentHealth > 0 && !EstaDormido() && !EstaConfuso() && _reallyDead == false)
        {
            isWaiting = true;
            MovementCoroutine = StartCoroutine(EnemyMovement());
        }
    }

    // ══════════════════════════════════════════════════════════
    // LOCK-ON CALLERS
    // ══════════════════════════════════════════════════════════

    void OnPlayerCounter(EnemyScript target)
    {
        if (target == this) PrepareAttack(false);
    }

    void OnPlayerTrajectory(EnemyScript target)
    {
        if (target != this) return;

        StopEnemyCoroutines();
        isLockedTarget = true;
        PrepareAttack(false);
        StopMoving();

        if (lockTimerCoroutine != null) StopCoroutine(lockTimerCoroutine);
        lockTimerCoroutine = StartCoroutine(LockTimer());
    }

    IEnumerator LockTimer()
    {
        yield return new WaitForSeconds(4f);
        // FIX: Solo liberar si seguimos existiendo
        if (this != null && gameObject != null)
            isLockedTarget = false;
    }

    // ══════════════════════════════════════════════════════════
    // FIX: MORIR — Animación de muerte + null safety + limpieza
    // ══════════════════════════════════════════════════════════

    public override void Morir()
    {
        if (_reallyDead) return;
        _reallyDead = true;

        Debug.Log($"[Enemy] ☠ '{name}' muriendo...");

        // ── 1. Parar TODAS las corrutinas inmediatamente ──
        StopAllCoroutines();

        // ── 2. Resetear TODOS los estados ──
        isStunned = false;
        isLockedTarget = false;
        isRetreating = false;
        isPreparingAttack = false;
        isMoving = false;
        isWaiting = false;

        // ── 3. Animación de muerte — ANTES de desactivar ──
        if (animator != null)
        {
            animator.enabled = true;  // Asegurar que está activo
            animator.SetTrigger("Death");
        }

        // ── 4. Desactivar componentes de gameplay ──
        this.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        // Desactivar hitbox si existe
        if (enemyHitbox != null)
        {
            enemyHitbox.CloseHitboxWindow();
            enemyHitbox.enabled = false;
        }

        // ── 5. Notificar al EnemyManager ──
        if (enemyManager != null)
        {
            enemyManager.SetEnemyAvailability(this, false);
            enemyManager.RemoveEnemy(this);
        }

        // ── 6. Dar XP y loot al jugador ──
        var player = FindAnyObjectByType<SimpleWalk>();
        if (player != null)
        {
            var ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.GanarXP(XPEarned);
                ph.Lootbox();
            }
        }

        // ── 7. Destruir tras animación ──
        DeathCoroutine = StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Esperar a que se reproduzca la animación de muerte
        yield return new WaitForSeconds(2f);

        // Desvincular eventos si quedaran
        UnsubscribeEvents();

        if (gameObject != null)
            Destroy(gameObject);
    }

    // ══════════════════════════════════════════════════════════
    // FIX: Death() — Para compatibilidad con llamadas legacy
    // ══════════════════════════════════════════════════════════

    void Death()
    {
        Morir();
    }

    // ══════════════════════════════════════════════════════════
    // FIX: ONDESTROY — Limpieza final garantizada
    // ══════════════════════════════════════════════════════════

    void OnDestroy()
    {
        _reallyDead = true;
        isStunned = false;
        isLockedTarget = false;

        UnsubscribeEvents();
    }

    void UnsubscribeEvents()
    {
        var player = FindAnyObjectByType<SimpleWalk>();
        if (player != null)
        {
            player.OnCounterAttack.RemoveListener(OnPlayerCounter);
            player.OnTrajectory.RemoveListener(OnPlayerTrajectory);
        }
    }

    // ══════════════════════════════════════════════════════════
    // RESET — Para EnemyManager.ForceUnstuck()
    // ══════════════════════════════════════════════════════════

    public void ResetAllStates()
    {
        isStunned = false;
        isLockedTarget = false;
        isRetreating = false;
        isPreparingAttack = false;
        isMoving = false;
        isWaiting = true;
        _reallyDead = false;

        StopAllCoroutines();

        StopMoving();

        if (currentHealth > 0 && !EstaDormido() && !EstaConfuso())
            MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    // ══════════════════════════════════════════════════════════
    // ENEMY AI MOVEMENT
    // ══════════════════════════════════════════════════════════

    public IEnumerator EnemyMovement()
    {
        if (this == null || !isActiveAndEnabled) yield break;

        yield return new WaitUntil(() => isWaiting == true);

        if (EstaDormido() || EstaConfuso())
        {
            yield return new WaitForSeconds(1f);
            if (this != null && isActiveAndEnabled)
                MovementCoroutine = StartCoroutine(EnemyMovement());
            yield break;
        }

        int randomChance = Random.Range(0, 2);

        if (randomChance == 1)
        {
            int randomDir = Random.Range(0, 2);
            moveDirection = randomDir == 1 ? Vector3.right : Vector3.left;
            isMoving = true;
        }
        else
        {
            StopMoving();
        }

        yield return new WaitForSeconds(1);

        if (this != null && isActiveAndEnabled)
            MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    // ══════════════════════════════════════════════════════════
    // AI — ATTACK / RETREAT / SET RETREAT
    // ══════════════════════════════════════════════════════════

    public void SetAttack()
    {
        if (lockTimerCoroutine != null) StopCoroutine(lockTimerCoroutine);
        isLockedTarget = false;
        isWaiting = false;

        PrepareAttackCoroutine = StartCoroutine(PrepAttack());
    }

    IEnumerator PrepAttack()
    {
        PrepareAttack(true);
        yield return new WaitForSeconds(0.2f);
        moveDirection = Vector3.forward;
        isMoving = true;

        float timer = 0f;
        while (isPreparingAttack && timer < 3f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (isPreparingAttack)
            PrepareAttack(false);
    }

    public void SetRetreat()
    {
        StopEnemyCoroutines();
        RetreatCoroutine = StartCoroutine(PrepRetreat());
    }

    IEnumerator PrepRetreat()
    {
        yield return new WaitForSeconds(1.4f);
        OnRetreat.Invoke(this);
        isRetreating = true;
        moveDirection = -Vector3.forward;
        isMoving = true;

        float timer = 0f;
        float distToPlayer = playerCombat != null
            ? Vector3.Distance(transform.position, playerCombat.transform.position)
            : 99f;

        while (isRetreating && timer < 5f && distToPlayer <= 4f)
        {
            timer += Time.deltaTime;
            if (playerCombat != null)
                distToPlayer = Vector3.Distance(transform.position, playerCombat.transform.position);
            yield return null;
        }

        isRetreating = false;
        StopMoving();
        isWaiting = true;
        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT HELPERS
    // ══════════════════════════════════════════════════════════

    void PrepareAttack(bool active)
    {
        isPreparingAttack = active;
        if (!active) StopMoving();
    }

    private void Attack()
    {
        if (EstaDormido() || EstaConfuso())
        {
            PrepareAttack(false);
            return;
        }

        transform.DOMove(transform.position + (transform.forward / 1), .5f);
        if (animator != null) animator.SetTrigger("AirPunch");
    }

    public void HitEvent()
    {
        if (EstaDormido() || EstaConfuso()) return;

        EnemyHitbox hitbox = GetComponentInChildren<EnemyHitbox>();
        if (hitbox != null)
        {
            SimpleWalk sw = FindAnyObjectByType<SimpleWalk>();
            if (sw != null)
            {
                float dist = Vector3.Distance(transform.position, sw.transform.position);
                if (dist < 2.5f && !sw.isCountering && !sw.isAttackingEnemy)
                {
                    var ph = sw.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.TakeDamage(danyoAtaque);
                        sw.RecibirDanyo();
                    }
                }
            }
        }

        PrepareAttack(false);
    }

    public void GolpearNinja(PlayerHealth vidaNinja)
    {
        if (vidaNinja == null) return;
        vidaNinja.TakeDamage(danyoAtaque);
    }

    // ══════════════════════════════════════════════════════════
    // MOVEMENT
    // ══════════════════════════════════════════════════════════

    void MoveEnemy(Vector3 direction)
    {
        if (EstaDormido() || EstaConfuso()) return;

        moveSpeed = 1;

        if (direction == Vector3.forward) moveSpeed = 5;
        if (direction == -Vector3.forward) moveSpeed = 2;

        if (animator != null)
        {
            animator.SetFloat("InputMagnitude",
                (characterController.velocity.normalized.magnitude * direction.z) / (5 / moveSpeed),
                .2f, Time.deltaTime);
            animator.SetBool("Strafe", (direction == Vector3.right || direction == Vector3.left));
            animator.SetFloat("StrafeDirection", direction.normalized.x, .2f, Time.deltaTime);
        }

        if (!isMoving) return;

        if (playerCombat == null) { StopMoving(); return; }

        Vector3 dir = (playerCombat.transform.position - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir;
        Vector3 movedir = Vector3.zero;
        Vector3 finalDirection = Vector3.zero;

        if (direction == Vector3.forward) finalDirection = dir;
        if (direction == Vector3.right || direction == Vector3.left) finalDirection = (pDir * direction.normalized.x);
        if (direction == -Vector3.forward) finalDirection = -transform.forward;

        if (direction == Vector3.right || direction == Vector3.left) moveSpeed /= 1.5f;

        movedir += finalDirection * moveSpeed * Time.deltaTime;
        characterController.Move(movedir);

        if (!isPreparingAttack) return;

        if (Vector3.Distance(transform.position, playerCombat.transform.position) < 2)
        {
            StopMoving();
            if (!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
                Attack();
            else
                PrepareAttack(false);
        }
    }

    public void StopMoving()
    {
        isMoving = false;
        moveDirection = Vector3.zero;
        if (characterController != null && characterController.enabled)
            characterController.Move(moveDirection);
    }

    public void ReleaseLock()
    {
        isLockedTarget = false;
        if (lockTimerCoroutine != null)
        {
            StopCoroutine(lockTimerCoroutine);
            lockTimerCoroutine = null;
        }
    }

    // ══════════════════════════════════════════════════════════
    // STOP ALL — Con reset de estados
    // ══════════════════════════════════════════════════════════

    void StopEnemyCoroutines()
    {
        PrepareAttack(false);

        if (isRetreating)
        {
            if (RetreatCoroutine != null) StopCoroutine(RetreatCoroutine);
            isRetreating = false;
        }

        if (PrepareAttackCoroutine != null) { StopCoroutine(PrepareAttackCoroutine); PrepareAttackCoroutine = null; }
        if (DamageCoroutine != null) { StopCoroutine(DamageCoroutine); DamageCoroutine = null; }
        if (MovementCoroutine != null) { StopCoroutine(MovementCoroutine); MovementCoroutine = null; }
        if (DeathCoroutine != null) { StopCoroutine(DeathCoroutine); DeathCoroutine = null; }
        if (lockTimerCoroutine != null) { StopCoroutine(lockTimerCoroutine); lockTimerCoroutine = null; }
        if (sleepCoroutine != null) { StopCoroutine(sleepCoroutine); sleepCoroutine = null; }
        if (confusedCoroutine != null) { StopCoroutine(confusedCoroutine); confusedCoroutine = null; }

        // FIX: Resetear estados al parar corrutinas
        isStunned = false;
        isPreparingAttack = false;
    }

    // ══════════════════════════════════════════════════════════
    // PUBLIC BOOLEANS
    // ══════════════════════════════════════════════════════════

    public bool IsPreparingAttack() => isPreparingAttack;
    public bool IsRetreating() => isRetreating;
    public bool IsLockedTarget() => isLockedTarget;
    public bool IsStunned() => isStunned;


    public void EnableHitbox()     {enemyHitbox.OpenHitboxWindow(); }
    public void DisableHitbox()     {enemyHitbox.CloseHitboxWindow(); }
}