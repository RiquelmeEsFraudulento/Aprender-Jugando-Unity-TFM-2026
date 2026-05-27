using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class EnemyScript : Damageable
{
    //Declarations
    private Animator animator;
    private SimpleWalk playerCombat;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    public CharacterController characterController;

    [Header("Stats")]
    private float moveSpeed = 1;
    private Vector3 moveDirection;

    [Header("States")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isRetreating;
    [SerializeField] private bool isLockedTarget;
    [SerializeField] private bool isStunned;
    [SerializeField] private bool isWaiting = true;

    [Header("Polish")]
    private Coroutine PrepareAttackCoroutine;
    private Coroutine RetreatCoroutine;
    private Coroutine DamageCoroutine;
    private Coroutine MovementCoroutine;
    private Coroutine DeathCoroutine;
    private Coroutine lockTimerCoroutine;

    // ── Corrutinas de estados especiales ─────────────────────
    private Coroutine sleepCoroutine;
    private Coroutine confusedCoroutine;

    //Events
    public UnityEvent<EnemyScript> OnDamage;
    public UnityEvent<EnemyScript> OnStopMoving;
    public UnityEvent<EnemyScript> OnRetreat;
    public int   danyoAtaque        = 10;
    public float XPEarned = 5f;
    [SerializeField] public EnemyHitbox enemyHitbox;

    private const bool LOG_IA = true;

    void Start()
    {
        enemyManager = GetComponentInParent<EnemyManager>();
        base.InicializarVida();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        playerCombat = FindAnyObjectByType<SimpleWalk>();
        enemyDetection = playerCombat.GetComponentInChildren<EnemyDetection>();

        playerCombat.OnCounterAttack.AddListener((x) => OnPlayerCounter(x));
        playerCombat.OnTrajectory.AddListener((x) => OnPlayerTrajectory(x));

        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    IEnumerator EnemyMovement()
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

    void Update()
    {
        // ── Mirar al jugador solo si NO está confuso ────────────
        if (!EstaConfuso())
        {
            transform.LookAt(new Vector3(playerCombat.transform.position.x, transform.position.y, playerCombat.transform.position.z));
        }

        MoveEnemy(moveDirection);
        base.AvanzarCooldown();
        if (estaEnvenenado) ProcesarVenenoPorTiempo();
    }

    // ══════════════════════════════════════════════════════════
    // SLEEP — Activado desde SimpleWalk / Damageable
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Llamado cuando Damageable.AplicarSleep() tiene éxito.
    /// Detiene toda acción y pone animación IDLE.
    /// </summary>
    public void ActivarSleep()
    {
        if (sleepCoroutine != null)
        {
            StopCoroutine(sleepCoroutine);
            sleepCoroutine = null;
        }
        sleepCoroutine = StartCoroutine(SleepCoroutine());
    }

    IEnumerator SleepCoroutine()
    {
        DebugIA($"[IA] '{name}' → SLEEP (SLEEP)");

        // Detener toda acción actual
        StopEnemyCoroutines();
        StopMoving();

        // Forzar animación SLEEP
        animator.SetTrigger("Sleep");

        // Bucle: permanecer dormido hasta que el estado se limpie
        while (EstaDormido())
        {
            yield return null;
        }

        // ── Despertó (por tiempo o por golpe) ──────────────────
        DebugIA($"[IA] '{name}' → DESPERTÓ");

        // Reanudar comportamiento normal
        isWaiting = true;
        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    // ══════════════════════════════════════════════════════════
    // CONFUSED — Activado desde SimpleWalk / Damageable
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Llamado cuando Damageable.AplicarConfused() tiene éxito.
    /// Gira espaldas al jugador y hace animación de ataque falso.
    /// </summary>
    public void ActivarConfused()
    {
        if (confusedCoroutine != null)
        {
            StopCoroutine(confusedCoroutine);
            confusedCoroutine = null;
        }
        confusedCoroutine = StartCoroutine(ConfusedCoroutine());
    }

    IEnumerator ConfusedCoroutine()
    {
        DebugIA($"[IA] '{name}' → CONFUSED (espaldas al jugador)");

        // Detener toda acción actual
        StopEnemyCoroutines();
        StopMoving();

        // ── Girar 180° (espaldas al jugador) ───────────────────
        Vector3 dirToPlayer = (playerCombat.transform.position - transform.position).normalized;
        Vector3 backDir = -dirToPlayer;
        backDir.y = 0;
        if (backDir != Vector3.zero)
        {
            transform.forward = backDir;
        }

        // ── Animación de pegar de espaldas (ataque falso) ──────
        animator.SetTrigger("AirPunch");

        // Bucle: permanecer confuso hasta que el estado se limpie
        while (EstaConfuso())
        {
            yield return null;
        }

        // ── Se recuperó (por tiempo o por golpe) ───────────────
        DebugIA($"[IA] '{name}' → Se recuperó de CONFUSED");

        // Reanudar comportamiento normal
        isWaiting = true;
        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    // ══════════════════════════════════════════════════════════
    // INTERRUPCIÓN POR GOLPE — override de TakeDamage
    // ══════════════════════════════════════════════════════════

    public override void TakeDamage(int amount, DamageType damageType, GameObject source)
    {
        // --- Daño base, cooldown, veneno/sangrado ---
        base.TakeDamage(amount, damageType, source);

        if (currentHealth <= 0)
            return;

        // --- Interrumpir corrutinas de estados especiales ------
        if (sleepCoroutine != null)
        {
            StopCoroutine(sleepCoroutine);
            sleepCoroutine = null;
            DebugIA($"[IA] '{name}' → SLEEP interrumpido por golpe");
        }
        if (confusedCoroutine != null)
        {
            StopCoroutine(confusedCoroutine);
            confusedCoroutine = null;
            DebugIA($"[IA] '{name}' → CONFUSED interrumpido por golpe");
        }

        // --- Reacción de impacto ---
        StopEnemyCoroutines();
        DamageCoroutine = StartCoroutine(HitCoroutine());

        enemyDetection.SetCurrentTarget(null);
        isLockedTarget = false;
        OnDamage.Invoke(this);

        animator.SetTrigger("Hit");
        transform.DOMove(transform.position - (transform.forward / 2), .3f).SetDelay(.1f);
        StopMoving();

        IEnumerator HitCoroutine()
        {
            isStunned = true;
            yield return new WaitForSeconds(.5f);
            isStunned = false;

            // Tras el aturdimiento, reanudar si sigue vivo
            if (currentHealth > 0 && !EstaDormido() && !EstaConfuso())
            {
                isWaiting = true;
                MovementCoroutine = StartCoroutine(EnemyMovement());
            }
        }
    }

    // ══════════════════════════════════════════════════════════
    // EVENTOS DE JUGADOR (sin cambios, pero protegidos)
    // ══════════════════════════════════════════════════════════

    void OnPlayerCounter(EnemyScript target)
    {
        if (target == this)
        {
            PrepareAttack(false);
        }
    }

    void OnPlayerTrajectory(EnemyScript target)
    {
        if (target == this)
        {
            StopEnemyCoroutines();
            isLockedTarget = true;
            PrepareAttack(false);
            StopMoving();

            if (lockTimerCoroutine != null)
            {
                StopCoroutine(lockTimerCoroutine);
            }

            lockTimerCoroutine = StartCoroutine(LockTimer());
        }
    }

    IEnumerator LockTimer()
    {
        yield return new WaitForSeconds(4f);
        isLockedTarget = false;
    }

    void Death()
    {
        StopEnemyCoroutines();

        this.enabled = false;
        characterController.enabled = false;
        animator.SetTrigger("Death");
        enemyManager.SetEnemyAvailiability(this, false);
    }

    public override void Morir()
    {
        playerCombat.GetComponent<PlayerHealth>().Lootbox();
        StopEnemyCoroutines();

        this.enabled = false;
        characterController.enabled = false;
        animator.SetTrigger("Death");
        enemyManager.SetEnemyAvailiability(this, false);
        enemyManager.RemoveEnemy(this);
        playerCombat.GetComponent<PlayerHealth>().GanarXP(XPEarned);
        DeathCoroutine = StartCoroutine(MuerteCooldown());

        IEnumerator MuerteCooldown()
        {
            yield return new WaitForSeconds(1.4f);
            onDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    public void SetRetreat()
    {
        StopEnemyCoroutines();

        RetreatCoroutine = StartCoroutine(PrepRetreat());

        IEnumerator PrepRetreat()
        {
            yield return new WaitForSeconds(1.4f);
            OnRetreat.Invoke(this);
            isRetreating = true;
            moveDirection = -Vector3.forward;
            isMoving = true;

            float timer = 0f;
            while (isRetreating && timer < 5f &&
                Vector3.Distance(transform.position, playerCombat.transform.position) <= 4f)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            isRetreating = false;
            StopMoving();
            isWaiting = true;
            MovementCoroutine = StartCoroutine(EnemyMovement());
        }
    }

    public void SetAttack()
    {
        if (lockTimerCoroutine != null) StopCoroutine(lockTimerCoroutine);
        isLockedTarget = false;
        isWaiting = false;

        PrepareAttackCoroutine = StartCoroutine(PrepAttack());

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
            {
                PrepareAttack(false);
            }
        }
    }

    void PrepareAttack(bool active)
    {
        isPreparingAttack = active;

        if (active)
        {
            //counterParticle.Play();
        }
        else
        {
            StopMoving();
            //counterParticle.Clear();
            //counterParticle.Stop();
        }
    }

    void MoveEnemy(Vector3 direction)
    {
        // ── No mover si está dormido o confuso ─────────────────
        if (EstaDormido() || EstaConfuso())
            return;

        moveSpeed = 1;

        if (direction == Vector3.forward)
            moveSpeed = 5;
        if (direction == -Vector3.forward)
            moveSpeed = 2;

        animator.SetFloat("InputMagnitude", (characterController.velocity.normalized.magnitude * direction.z) / (5 / moveSpeed), .2f, Time.deltaTime);
        animator.SetBool("Strafe", (direction == Vector3.right || direction == Vector3.left));
        animator.SetFloat("StrafeDirection", direction.normalized.x, .2f, Time.deltaTime);

        if (!isMoving)
            return;

        Vector3 dir = (playerCombat.transform.position - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir;
        Vector3 movedir = Vector3.zero;
        Vector3 finalDirection = Vector3.zero;

        if (direction == Vector3.forward)
            finalDirection = dir;
        if (direction == Vector3.right || direction == Vector3.left)
            finalDirection = (pDir * direction.normalized.x);
        if (direction == -Vector3.forward)
            finalDirection = -transform.forward;

        if (direction == Vector3.right || direction == Vector3.left)
            moveSpeed /= 1.5f;

        movedir += finalDirection * moveSpeed * Time.deltaTime;

        characterController.Move(movedir);

        if (!isPreparingAttack)
            return;

        if (Vector3.Distance(transform.position, playerCombat.transform.position) < 2)
        {
            StopMoving();
            if (!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
                Attack();
            else
                PrepareAttack(false);
        }
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

    // In EnemyScript.cs — replace the Attack() and HitEvent() methods

    private void Attack()
    {
        // ── No atacar si está dormido o confuso ────────────────
        if (EstaDormido() || EstaConfuso())
        {
            DebugIA($"[IA] '{name}' intentó atacar pero está {(EstaDormido() ? "dormido" : "confuso")} → ignorado.");
            PrepareAttack(false);
            return;
        }

        transform.DOMove(transform.position + (transform.forward / 1), .5f);
        animator.SetTrigger("AirPunch");
    }

    public void HitEvent()
    {
        // ── No dañar al jugador si está dormido o confuso ──────
        if (EstaDormido() || EstaConfuso())
        {
            DebugIA($"[IA] '{name}' HitEvent ignorado (estado especial).");
            return;
        }

        // Find the hitbox and trigger its logic via distance check as fallback
        EnemyHitbox hitbox = GetComponentInChildren<EnemyHitbox>();
        if (hitbox != null)
        {
            // Fallback: if hitbox captured nothing, check distance directly
            SimpleWalk playerCombat = FindAnyObjectByType<SimpleWalk>();
            if (playerCombat != null)
            {
                float dist = Vector3.Distance(transform.position, playerCombat.transform.position);
                if (dist < 2.5f && !playerCombat.isCountering && !playerCombat.isAttackingEnemy)
                {
                    GolpearNinja(playerCombat.GetComponent<PlayerHealth>());
                    playerCombat.RecibirDanyo();
                }
            }
        }

        PrepareAttack(false);
    }


    public void EnableHitbox()     {enemyHitbox.OpenHitboxWindow(); }
    public void DisableHitbox()     {enemyHitbox.CloseHitboxWindow(); }


    public void StopMoving()
    {
        isMoving = false;
        moveDirection = Vector3.zero;
        if (characterController.enabled)
            characterController.Move(moveDirection);
    }

    void StopEnemyCoroutines()
    {
        PrepareAttack(false);

        if (isRetreating)
        {
            if (RetreatCoroutine != null)
                StopCoroutine(RetreatCoroutine);
            isRetreating = false;
        }

        if (PrepareAttackCoroutine != null)
            StopCoroutine(PrepareAttackCoroutine);

        if (DamageCoroutine != null)
            StopCoroutine(DamageCoroutine);

        if (MovementCoroutine != null)
            StopCoroutine(MovementCoroutine);

        if (DeathCoroutine != null)
        {
            StopCoroutine(DeathCoroutine);
            DeathCoroutine = null;
        }

        if (sleepCoroutine != null)
        {
            StopCoroutine(sleepCoroutine);
            sleepCoroutine = null;
        }
        if (confusedCoroutine != null)
        {
            StopCoroutine(confusedCoroutine);
            confusedCoroutine = null;
        }
    }
    #region Public Booleans

    public bool IsAttackable()
    {
        return currentHealth > 0;
    }

    public bool IsPreparingAttack()
    {
        return isPreparingAttack;
    }

    public bool IsRetreating()
    {
        return isRetreating;
    }

    public bool IsLockedTarget()
    {
        return isLockedTarget;
    }

    public bool IsStunned()
    {
        return isStunned;
    }

    public void GolpearNinja(PlayerHealth vidaNinja)
    {
        if (vidaNinja == null) return;
        vidaNinja.TakeDamage(danyoAtaque);
        DebugIA($"[IA] '{name}' golpeó al ninja: -{danyoAtaque}");
    }

    void DebugIA(string msg) { if (LOG_IA) Debug.Log(msg); }

    #endregion
}