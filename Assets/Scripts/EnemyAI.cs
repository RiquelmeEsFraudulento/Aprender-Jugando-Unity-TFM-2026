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

    [Header("═══ FORMATION SYSTEM ═══")]
    public float formationReachTime = 4f;
    public float positionTolerance = 0.8f;
    public float repositionSpeed = 3.5f;
    public float strafeSpeed = 1.2f;
    public float formationRecalcInterval = 1.5f;
    public float minPlayerDistance = 1.6f;

    [SerializeField] private int formationRing = 0;
    [SerializeField] private int formationSlot = -1;
    [SerializeField] private int formationSlotsInRing = 1;
    [SerializeField] private float formationRadius = 3f;
    [SerializeField] private Vector3 formationTargetPos;
    [SerializeField] private float formationRecalcTimer = 0f;
    [SerializeField] private float strafeAngleOffset = 0f;
    [SerializeField] private int strafeDir = 1;
    [SerializeField] private float lastDirFlipTime = -10f;
    [SerializeField] private float timeSinceSpawn = 0f;
    [SerializeField] private bool hasReachedRing = false;

    [SerializeField] private bool isInFormation = false;

    
    [SerializeField] private Vector3 lastMoveDir = Vector3.zero;
    [SerializeField] private float oscillationBlockUntil = -1f;

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

        // ── Formation init ──
        timeSinceSpawn = 0f;
        hasReachedRing = false;
        formationSlot = -1;
        formationRecalcTimer = 0f;
        strafeDir = Random.value > 0.5f ? 1 : -1;
        strafeAngleOffset = Random.Range(0f, 360f);
        lastDirFlipTime = -10f;
        oscillationBlockUntil = -1f;
        lastMoveDir = Vector3.zero;

        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    // ══════════════════════════════════════════════════════════
    // IS ATTACKABLE — NO contar enemigos que no pueden actuar
    // ══════════════════════════════════════════════════════════

    public bool IsAttackable()
    {
        if (_reallyDead) return false;
        if (currentHealth <= 0) return false;
        if (!isActiveAndEnabled) return false;
        if (EstaDormido() && sleepCoroutine != null) return false;
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
    // TAKEDAMAGE
    // ══════════════════════════════════════════════════════════

    public override void TakeDamage(int amount, DamageType damageType, GameObject source)
    {
        base.TakeDamage(amount, damageType, source);

        if (currentHealth <= 0) return;

        if (sleepCoroutine != null) { StopCoroutine(sleepCoroutine); sleepCoroutine = null; }
        if (confusedCoroutine != null) { StopCoroutine(confusedCoroutine); confusedCoroutine = null; }

        isStunned = false;
        isLockedTarget = false;

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
    // HIT COROUTINE
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

        isStunned = false;

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
        if (this != null && gameObject != null)
            isLockedTarget = false;
    }

    // ══════════════════════════════════════════════════════════
    // MORIR — Animación de muerte + null safety + limpieza
    // ══════════════════════════════════════════════════════════

    public override void Morir()
    {
        if (_reallyDead) return;
        _reallyDead = true;

        Debug.Log($"[Enemy] ☠ '{name}' muriendo...");

        StopAllCoroutines();

        isStunned = false;
        isLockedTarget = false;
        isRetreating = false;
        isPreparingAttack = false;
        isMoving = false;
        isWaiting = false;

        // Formation state
        isInFormation = false;
        hasReachedRing = false;
        formationSlot = -1;
        formationRing = 0;
        formationSlotsInRing = 1;
        formationRadius = 3f;
        formationTargetPos = Vector3.zero;
        formationRecalcTimer = 999f;
        strafeDir = Random.value > 0.5f ? 1 : -1;
        strafeAngleOffset = Random.Range(0f, 360f);
        lastDirFlipTime = -10f;
        oscillationBlockUntil = -1f;
        lastMoveDir = Vector3.zero;

        if (animator != null)
        {
            animator.enabled = true;
            animator.SetTrigger("Death");
        }

        this.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        if (enemyHitbox != null)
        {
            enemyHitbox.CloseHitboxWindow();
            enemyHitbox.enabled = false;
        }

        if (enemyManager != null)
        {
            enemyManager.SetEnemyAvailability(this, false);
            enemyManager.RemoveEnemy(this);
        }

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

        DeathCoroutine = StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(2f);
        UnsubscribeEvents();
        if (gameObject != null)
            Destroy(gameObject);
    }

    void Death()
    {
        Morir();
    }

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
    // RESET ALL STATES — Para EnemyManager.ForceUnstuck()
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

        // Formation reset
        isInFormation = false;
        hasReachedRing = false;
        formationSlot = -1;
        formationRing = 0;
        formationSlotsInRing = 1;
        formationRadius = 3f;
        formationTargetPos = Vector3.zero;
        formationRecalcTimer = 999f;
        strafeDir = Random.value > 0.5f ? 1 : -1;
        strafeAngleOffset = Random.Range(0f, 360f);
        lastDirFlipTime = -10f;
        timeSinceSpawn = 0f;
        oscillationBlockUntil = -1f;
        lastMoveDir = Vector3.zero;
        moveSpeed = 1f;

        StopAllCoroutines();
        StopMoving();

        if (currentHealth > 0 && !EstaDormido() && !EstaConfuso())
            MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    
    // ══════════════════════════════════════════════════════════
    // ENEMY MOVEMENT COROUTINE — Define moveDirection cada 0.4s
    // ══════════════════════════════════════════════════════════

    public IEnumerator EnemyMovement()
    {
        if (this == null || !isActiveAndEnabled) yield break;
        yield return new WaitUntil(() => isWaiting == true);

        while (this != null && isActiveAndEnabled && currentHealth > 0 && !_reallyDead)
        {
            if (EstaDormido() || EstaConfuso()) { yield return new WaitForSeconds(0.3f); continue; }
            if (_reallyDead) yield break;
            if (isPreparingAttack || isRetreating || isStunned)
            {
                isInFormation = false;
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            if (playerCombat == null || enemyManager == null)
            {
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            timeSinceSpawn += 0.4f;
            formationRecalcTimer += 0.4f;

            if (formationRecalcTimer >= formationRecalcInterval || formationSlot < 0)
            {
                formationRecalcTimer = 0f;
                RecalculateSlot();
            }

            if (formationSlot >= 0)
            {
                formationTargetPos = EnemyFormation.GetSlotWorldPosition(
                    playerCombat.transform.position,
                    formationSlot, formationSlotsInRing,
                    formationRadius, strafeAngleOffset);

                float distToPlayer = Vector3.Distance(transform.position, playerCombat.transform.position);
                float distToSlot = Vector3.Distance(
                    new Vector3(transform.position.x, 0, transform.position.z),
                    new Vector3(formationTargetPos.x, 0, formationTargetPos.z));

                if (timeSinceSpawn < formationReachTime && !hasReachedRing)
                {
                    if (distToSlot > positionTolerance * 2.5f || distToPlayer > formationRadius * 1.8f)
                    {
                        Vector3 dir = (formationTargetPos - transform.position);
                        dir.y = 0;
                        if (dir.sqrMagnitude > 0.001f)
                            moveDirection = dir.normalized;
                        isMoving = true;
                    }
                    else
                    {
                        hasReachedRing = true;
                        isInFormation = true;
                        StopMoving();
                    }
                }
                else
                {
                    hasReachedRing = true;

                    if (distToPlayer < minPlayerDistance)
                    {
                        Vector3 away = (transform.position - playerCombat.transform.position);
                        away.y = 0;
                        if (away.sqrMagnitude > 0.001f)
                            moveDirection = away.normalized;
                        isMoving = true;
                        isInFormation = false;
                    }
                    else if (distToSlot <= positionTolerance)
                    {
                        isInFormation = true;
                        float noise = Mathf.PerlinNoise(Time.time * 0.4f + GetInstanceID() * 1.7f, 0f);
                        if (noise > 0.65f)
                        {
                            Vector3 toP = (playerCombat.transform.position - transform.position);
                            toP.y = 0; toP.Normalize();
                            Vector3 tangent = Vector3.Cross(Vector3.up, toP) * strafeDir;
                            moveDirection = tangent.normalized;
                            isMoving = true;
                        }
                        else
                        {
                            StopMoving();
                        }
                    }
                    else
                    {
                        isInFormation = false;
                        Vector3 dir = (formationTargetPos - transform.position);
                        dir.y = 0;
                        if (dir.sqrMagnitude > 0.001f)
                            moveDirection = dir.normalized;
                        isMoving = true;
                    }
                }
            }
            else
            {
                if (!isMoving)
                {
                    int r = Random.Range(0, 2);
                    moveDirection = r == 1 ? Vector3.right : Vector3.left;
                    isMoving = true;
                }
            }

            yield return new WaitForSeconds(0.4f);
        }
    }

    // ══════════════════════════════════════════════════════════
    // MOVEENEMY — Ejecuta el movimiento (llamado en Update)
    // ══════════════════════════════════════════════════════════

    void MoveEnemy(Vector3 direction)
    {
        if (EstaDormido() || EstaConfuso()) return;
        if (characterController == null || !characterController.enabled) return;
        if (playerCombat == null) { StopMoving(); return; }

        if (!isPreparingAttack && !isRetreating && formationSlot >= 0)
        {
            formationTargetPos = EnemyFormation.GetSlotWorldPosition(
                playerCombat.transform.position,
                formationSlot, formationSlotsInRing,
                formationRadius, strafeAngleOffset);
        }

        float distToPlayer = Vector3.Distance(transform.position, playerCombat.transform.position);
        float ctrlRadius = characterController.radius + 0.15f;

        if (isPreparingAttack && direction == Vector3.forward && distToPlayer < 2f)
        {
            StopMoving();
            if (!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
                Attack();
            else
                PrepareAttack(false);
            return;
        }

        if (!isMoving) return;

        Vector3 finalDir = direction;
        if (!isPreparingAttack && distToPlayer < minPlayerDistance)
        {
            finalDir = (transform.position - playerCombat.transform.position);
            finalDir.y = 0;
            if (finalDir.sqrMagnitude < 0.001f) finalDir = -transform.forward;
            finalDir.Normalize();
        }

        moveSpeed = 1f;
        if (finalDir == Vector3.forward) moveSpeed = 5f;
        if (finalDir == -Vector3.forward) moveSpeed = 2f;

        if (!isInFormation && !isPreparingAttack && formationSlot >= 0)
            moveSpeed = repositionSpeed;
        if (isInFormation && !isPreparingAttack && !isRetreating)
            moveSpeed = strafeSpeed;

        finalDir = AvoidOtherEnemies(finalDir, ctrlRadius);

        if (Time.time < oscillationBlockUntil) { StopMoving(); return; }
        if (lastMoveDir != Vector3.zero && finalDir != Vector3.zero)
        {
            float dot = Vector3.Dot(lastMoveDir.normalized, finalDir.normalized);
            if (dot < -0.7f && Time.time - lastDirFlipTime < 0.8f)
                oscillationBlockUntil = Time.time + 0.6f;
        }
        if (finalDir != Vector3.zero) lastMoveDir = finalDir;

        Vector3 toPlayer = (playerCombat.transform.position - transform.position).normalized;
        toPlayer.y = 0;
        Vector3 right = Quaternion.AngleAxis(90, Vector3.up) * toPlayer;

        Vector3 worldMove = Vector3.zero;
        if (finalDir == Vector3.forward) worldMove = toPlayer;
        else if (finalDir == Vector3.right) worldMove = right;
        else if (finalDir == Vector3.left) worldMove = -right;
        else if (finalDir == -Vector3.forward || finalDir == Vector3.back) worldMove = -toPlayer;
        else worldMove = finalDir;

        if (finalDir == Vector3.right || finalDir == Vector3.left)
            moveSpeed /= 1.5f;

        Vector3 displacement = worldMove * moveSpeed * Time.deltaTime;
        characterController.Move(displacement);

        if (animator != null)
        {
            bool strafing = isInFormation && !isPreparingAttack && !isRetreating;
            if (strafing)
            {
                animator.SetBool("Strafe", true);
                animator.SetFloat("StrafeDirection", strafeDir, 0.15f, Time.deltaTime);
                animator.SetFloat("InputMagnitude", strafeSpeed / 5f, 0.15f, Time.deltaTime);
            }
            else
            {
                float norm = moveSpeed / 5f;
                animator.SetFloat("InputMagnitude",
                    (characterController.velocity.normalized.magnitude * worldMove.z) / Mathf.Max(0.1f, 5f / moveSpeed),
                    0.15f, Time.deltaTime);
                animator.SetBool("Strafe", (finalDir == Vector3.right || finalDir == Vector3.left));
                animator.SetFloat("StrafeDirection", finalDir.normalized.x, 0.15f, Time.deltaTime);
            }
        }
    }

    // ══════════════════════════════════════════════════════════
    // AVOID OTHER ENEMIES — Con cooldown en el cambio de dirección
    // ══════════════════════════════════════════════════════════

    Vector3 AvoidOtherEnemies(Vector3 desiredDir, float ctrlRadius)
    {
        if (enemyManager == null) return desiredDir;

        Vector3 avoidance = Vector3.zero;
        int count = 0;
        float checkDist = ctrlRadius * 2.5f + 0.4f;

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            var other = enemyManager.allEnemies[i].enemyScript;
            if (other == null || other == this) continue;
            if (!other.IsAttackable()) continue;

            Vector3 diff = transform.position - other.transform.position;
            diff.y = 0;
            float dist = diff.magnitude;

            if (dist < checkDist && dist > 0.05f)
            {
                float urgency = 1f - (dist / checkDist);
                avoidance += diff.normalized * urgency;
                count++;

                if (dist < ctrlRadius + 0.3f && Time.time - lastDirFlipTime > 0.6f)
                {
                    strafeDir *= -1;
                    lastDirFlipTime = Time.time;
                }
            }
        }

        if (count > 0)
        {
            avoidance /= count;
            avoidance.y = 0;
            Vector3 blended = (desiredDir + avoidance * 2f).normalized;
            return blended;
        }

        return desiredDir;
    }

    // ══════════════════════════════════════════════════════════
    // RECALCULAR SLOT — Llamado periódicamente por EnemyMovement
    // ══════════════════════════════════════════════════════════

    void RecalculateSlot()
    {
        if (playerCombat == null || enemyManager == null) return;

        int totalAlive = EnemyFormation.CountAlive(enemyManager);
        int globalSlot = EnemyFormation.GetGlobalSlot(this, enemyManager);

        if (totalAlive <= 0 || globalSlot < 0)
        {
            formationSlot = -1;
            return;
        }

        EnemyFormation.ComputeSlot(globalSlot, totalAlive,
            out formationRing, out formationSlot, out formationSlotsInRing, out formationRadius);

        float ringRot = formationRing * (180f / Mathf.Max(1, formationSlotsInRing));
        float individualRot = globalSlot * 47f;
        strafeAngleOffset = ringRot + individualRot;
    }

    // ══════════════════════════════════════════════════════════
    // RESET SURROUND STATE — Llamado cuando otro enemigo muere
    // ══════════════════════════════════════════════════════════

    public void ResetSurroundState()
    {
        hasReachedRing = false;
        formationRecalcTimer = 999f;
        formationSlot = -1;
    }

    // ══════════════════════════════════════════════════════════
    // AI — ATAQUE / RETIRADA
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
    // MOVEMENT HELPERS
    // ══════════════════════════════════════════════════════════

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

    public void EnableHitbox()     { enemyHitbox.OpenHitboxWindow(); }
    public void DisableHitbox()   { enemyHitbox.CloseHitboxWindow(); }
}

/// <summary>
/// Static formation calculator. All enemies share this brain.
/// Determines ring, slot, and world position for each enemy
/// based on alive count so they distribute in concentric rings.
/// </summary>
public static class EnemyFormation
{
    // ══════════════════════════════════════════════════════════
    // TUNING — Adjust these for spacing feel
    // ══════════════════════════════════════════════════════════
    public const float BASE_RING_RADIUS = 3.0f;     // Inner ring distance from player
    public const float RING_WIDTH = 2.5f;           // Distance between ring 0 and ring 1
    public const float ENEMY_SPACING_MIN = 1.6f;    // Minimum arc-length between two enemies on same ring
    public const float PLAYER_RADIUS = 0.5f;        // Player collision proxy radius

    // ══════════════════════════════════════════════════════════
    // RING + SLOT ASSIGNMENT
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Given a global index (0..totalAlive-1), compute which ring
    /// and which slot within that ring this enemy occupies.
    /// </summary>
    public static void ComputeSlot(
        int globalIndex,
        int totalAlive,
        out int ringIndex,
        out int slotInRing,
        out int slotsInRing,
        out float ringRadius)
    {
        ringIndex = 0;
        slotInRing = 0;
        slotsInRing = 1;
        ringRadius = BASE_RING_RADIUS;

        if (totalAlive <= 0) return;

        int consumed = 0;
        int ring = 0;

        while (consumed < totalAlive)
        {
            float r = BASE_RING_RADIUS + ring * RING_WIDTH;
            float circumference = 2f * Mathf.PI * (r + PLAYER_RADIUS);
            int capacity = Mathf.Max(1, Mathf.FloorToInt(circumference / ENEMY_SPACING_MIN));
            int remaining = totalAlive - consumed;
            int here = Mathf.Min(capacity, remaining);

            if (globalIndex < consumed + here)
            {
                ringIndex = ring;
                slotInRing = globalIndex - consumed;
                slotsInRing = here;
                ringRadius = r;
                return;
            }

            consumed += here;
            ring++;
        }

        // Fallback — should never reach here
        ringIndex = ring;
        slotInRing = 0;
        slotsInRing = 1;
        ringRadius = BASE_RING_RADIUS + ring * RING_WIDTH;
    }

    // ══════════════════════════════════════════════════════════
    // POSITION FROM SLOT
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// World position for a given slot.
    /// angleOffsetDegrees rotates the whole ring so rings don't line up.
    /// </summary>
    public static Vector3 GetSlotWorldPosition(
        Vector3 playerPos,
        int slotInRing,
        int slotsInRing,
        float ringRadius,
        float angleOffsetDegrees)
    {
        float angle;
        if (slotsInRing <= 1)
        {
            angle = angleOffsetDegrees;
        }
        else
        {
            float angleStep = 360f / slotsInRing;
            angle = angleOffsetDegrees + angleStep * slotInRing;
        }

        float rad = angle * Mathf.Deg2Rad;
        return playerPos + new Vector3(
            Mathf.Sin(rad) * ringRadius,
            0f,
            Mathf.Cos(rad) * ringRadius
        );
    }

    // ══════════════════════════════════════════════════════════
    // QUERIES
    // ══════════════════════════════════════════════════════════

    public static int GetGlobalSlot(EnemyScript query, EnemyManager mgr)
    {
        int slot = 0;
        for (int i = 0; i < mgr.allEnemies.Count; i++)
        {
            var e = mgr.allEnemies[i].enemyScript;
            if (e == null || !e.IsAttackable()) continue;
            if (e == query) return slot;
            slot++;
        }
        return -1;
    }

    public static int CountAlive(EnemyManager mgr)
    {
        int n = 0;
        for (int i = 0; i < mgr.allEnemies.Count; i++)
        {
            var e = mgr.allEnemies[i].enemyScript;
            if (e != null && e.IsAttackable()) n++;
        }
        return n;
    }
}

    