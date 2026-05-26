using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using DG.Tweening;
//using Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class SimpleWalk : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════
    // MOVIMIENTO
    // ══════════════════════════════════════════════════════════
    public float speed = 2f;
    public Animator animator;

    private CharacterController controller;
    private Vector3 moveDirection;

    // ══════════════════════════════════════════════════════════
    // LOCK-ON SYSTEM
    // ══════════════════════════════════════════════════════════
    [Header("Lock-On System")]

    [Tooltip("Radius in which enemies can be detected for lock-on.")]
    public float detectionRadius = 8f;

    [Tooltip("Minimum angle difference (degrees) before re-triggering DOTween rotation.")]
    public float rotationThreshold = 2f;

    [Tooltip("DOTween rotation duration in seconds.")]
    public float rotationDuration = 0.15f;

    public float kickRange       = 0.3f;   // Patadas: TrCrescent, TrChut
    public float lightSaberRange = 4f;     // LightSaber: TrSwSwing, TrSw360
    public float rapierRange     = 1f;   // Rapier: TrRaSwing, TrRa360

    /// <summary>The enemy currently locked onto. Null when unlocked.</summary>
    private Transform lockedEnemy;

    /// <summary>Whether the lock-on system is currently active.</summary>
    private bool isLockedOn = false;

    // ══════════════════════════════════════════════════════════
    // PELOTA (reservada)
    // ══════════════════════════════════════════════════════════
    public GameObject ballPrefab;
    private GameObject currentBall;
    public Transform ballSpawnPoint;

    // ══════════════════════════════════════════════════════════
    // HITBOXES
    // ══════════════════════════════════════════════════════════
    public Collider RapierCollider;
    public Collider LightSaberCollider;
    public Collider RightKickCollider;
    public Collider LeftKickCollider;
    public RapierHitbox Rapier;
    public WeaponHitbox LightSaber;
    public KickHitbox RightKick;
    public KickHitbox LeftKick;

    // ══════════════════════════════════════════════════════════
    // COMBAT — Settings (equivalente a CombatScript)
    // ══════════════════════════════════════════════════════════
    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1.3f;

    [Header("Combat State")]
    public Vector2 moveAxis;
    public bool isAttackingEnemy = false;
    public bool isCountering     = false;

    [Header("Combat References")]
    [SerializeField] private Transform punchPosition;
    //[SerializeField] private ParticleSystemScript punchParticle;
    [SerializeField] private GameObject lastHitCamera;
    [SerializeField] private Transform lastHitFocusObject;
    [SerializeField] private EnemyDetection enemyDetection;

    // ══════════════════════════════════════════════════════════
    // COMBAT — Eventos (EnemyScript se suscribe en su Start)
    // ══════════════════════════════════════════════════════════
    [Header("Combat Events")]
    public UnityEvent<EnemyScript> OnHit;
    public UnityEvent<EnemyScript> OnCounterAttack;
    public UnityEvent<EnemyScript> OnTrajectory;


    
    [Header("Estado especial seleccionado")]
    [Tooltip("7 = Sleep, 8 = Confused, 9 = None. Los hitboxes aplican este estado al llegar a 8 golpes.")]
    public Damageable.EstadoEspecial estadoSeleccionado = Damageable.EstadoEspecial.None;

    public System.Action DamageEvent;

    // ══════════════════════════════════════════════════════════
    // COMBAT — Internos
    // ══════════════════════════════════════════════════════════
    private EnemyManager   enemyManager;
    private EnemyScript    lockedTarget;
    // private CinemachineImpulseSource impulseSource;

    private Coroutine counterCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine damageCoroutine;

    private int      animationCount = 0;
    private string[] attacks;
    private string[] attackslong;
    private string[] attacksnear;

    private bool manualUnlock = false;


    

    // ══════════════════════════════════════════════════════════
    // START
    // ══════════════════════════════════════════════════════════
    void Start()
    {
        controller    = GetComponent<CharacterController>();
        //impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
        enemyManager  = FindAnyObjectByType<EnemyManager>();

        if (animator == null)
            animator = GetComponent<Animator>();

        // enemyDetection puede asignarse desde Inspector o buscarse aquí
        if (enemyDetection == null)
            enemyDetection = GetComponentInChildren<EnemyDetection>();

    }

    // ══════════════════════════════════════════════════════════
    // UPDATE
    // ══════════════════════════════════════════════════════════
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");

        moveAxis      = new Vector2(horizontal, vertical);

        // ── Lock-On Toggle (Tab key) ──────────────────────────────────────────
        // Auto-lock si no hay objetivo fijado y hay enemigos vivos
        if (!isLockedOn && !manualUnlock && enemyManager.AliveEnemyCount() > 0)
        {
            TryLockOn();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isLockedOn)
            {
                UnlockTarget();
            }
            else
            {
                manualUnlock = false;
                TryLockOn();
            }
        }

        // ── Scroll para cambiar objetivo (cuando estamos fijados) ─────
        if (isLockedOn && Mathf.Abs(Input.mouseScrollDelta.y) > 0.1f)
        {
            int direction = Input.mouseScrollDelta.y > 0 ? 1 : -1; // arriba=1 (derecha), abajo=-1 (izquierda)
            CycleLockTarget(direction);
        }

        if (isLockedOn)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                CycleLockTarget(1);  // E = siguiente objetivo (derecha)
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                CycleLockTarget(-1); // Q = objetivo anterior (izquierda)
            }
        }

        // ── Auto-Unlock: enemy died or walked out of range ────────────────────
        if (isLockedOn)
        {
            if (lockedEnemy == null ||
                Vector3.Distance(transform.position, lockedEnemy.position) > detectionRadius)
            {
                UnlockTarget();
            }
        }

        // ── Movement & Rotation ───────────────────────────────────────────────
        if (isLockedOn && lockedEnemy != null && !isAttackingEnemy)
        {
            // --- Lock-On Movement: relative to the locked enemy ---------------
            Vector3 toEnemy = (lockedEnemy.position - transform.position).normalized;
            Vector3 right   = Vector3.Cross(Vector3.up, toEnemy);
            moveDirection   = (toEnemy * vertical + right * horizontal).normalized;

            // --- Smooth rotation toward enemy via DOTween (Y-axis only) -------
            Vector3 lookDir = new Vector3(toEnemy.x, 0f, toEnemy.z);
            if (lookDir != Vector3.zero)
            {
                float angleDiff = Vector3.Angle(transform.forward, lookDir);
                if (angleDiff > rotationThreshold)
                {
                    // Kill any running rotation tween before starting a new one
                    transform.DOKill();
                    transform
                        .DOLookAt(lockedEnemy.position, rotationDuration,
                                  AxisConstraint.Y, Vector3.up)
                        .SetEase(Ease.OutSine);
                }
            }

            // --- Visual debug line from player to locked enemy ----------------
            Debug.DrawLine(transform.position, lockedEnemy.position, Color.cyan);

            // --- Move (lock-on mode) ------------------------------------------
            if (moveDirection.magnitude >= 0.1f)
                controller.Move(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            // --- Free Movement (original behaviour) ---------------------------
            moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

            if (moveDirection.magnitude >= 0.1f && !isAttackingEnemy)
            {
                // Rotate character toward movement direction (instant, original)
                transform.forward = moveDirection;

                // Move
                controller.Move(moveDirection * speed * Time.deltaTime);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.Sleep;
            Debug.Log("[Estado] Modo seleccionado: SLEEP (se aplicará al enemigo tras 8 golpes)");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.Confused;
            Debug.Log("[Estado] Modo seleccionado: CONFUSED (se aplicará al enemigo tras 8 golpes)");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.None;
            Debug.Log("[Estado] Modo seleccionado: NONE (sin efecto de estado)");
        }

        animator.SetBool("isWalking", moveDirection.magnitude > 0f && !isAttackingEnemy);

        

        /*
        // ── Tus ataques originales ────────────────────────────
        if (Input.GetKeyDown(KeyCode.B))      StartCoroutine(DanceSpin());
        if (Input.GetKeyDown(KeyCode.J))      StartCoroutine(JumpCountdown());
        if (Input.GetKeyDown(KeyCode.C))      StartCoroutine(Crescent());
        if (Input.GetKeyDown(KeyCode.X))      StartCoroutine(Chut());
        if (Input.GetKeyDown(KeyCode.Mouse0)) AttackCheck();   // ← ahora lanza AttackCheck
        if (Input.GetKeyDown(KeyCode.Mouse1)) StartCoroutine(SwSwing());
        if (Input.GetKeyDown(KeyCode.R))      StartCoroutine(Ra360());
        if (Input.GetKeyDown(KeyCode.F))      StartCoroutine(Sw360());
        if (Input.GetKeyDown(KeyCode.Z))      StartCoroutine(RaSwing());
        if (Input.GetKeyDown(KeyCode.Space))  CounterCheck();  // ← ahora lanza CounterCheck
        */

        if (Input.GetKeyDown(KeyCode.Mouse0)) AttackCheck();   // ← ahora lanza AttackCheck
        if (Input.GetKeyDown(KeyCode.Mouse1)) AttackCheckWithSpecified("TrSwSwing");
        if (Input.GetKeyDown(KeyCode.R))      AttackCheckWithSpecified("TrRa360");
        if (Input.GetKeyDown(KeyCode.F))      AttackCheckWithSpecified("TrSw360");
        if (Input.GetKeyDown(KeyCode.Z))      AttackCheckWithSpecified("TrRaSwing");
        if (Input.GetKeyDown(KeyCode.C))      AttackCheckWithSpecified("TrCrescent");
        if (Input.GetKeyDown(KeyCode.X))      AttackCheckWithSpecified("TrChut");
        if (Input.GetKeyDown(KeyCode.Space))  CounterCheck();  // ← ahora lanza CounterCheck

    }


    /// <summary>
/// Cambia el enemigo fijado al más cercano a la derecha (direction=1) o izquierda (-1).
/// </summary>
    void CycleLockTarget(int direction)
    {
        if (lockedEnemy == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        Transform current = lockedEnemy;
        float currentAngle = AngleToTarget(current);

        Transform best = null;
        float bestDiff = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy") || hit.transform == current) continue;

            float angle = AngleToTarget(hit.transform);
            float diff = Mathf.DeltaAngle(currentAngle, angle);

            // direction=1 → diff positiva pequeña (derecha), direction=-1 → diff negativa pequeña (izquierda)
            if (direction > 0 && diff > 0 && diff < bestDiff)
            {
                bestDiff = diff;
                best = hit.transform;
            }
            else if (direction < 0 && diff < 0 && -diff < bestDiff)
            {
                bestDiff = -diff;
                best = hit.transform;
            }
        }

        if (best != null)
        {
            lockedEnemy = best;
            // Forzar rotación inmediata hacia el nuevo objetivo
            transform.DOKill();
            transform.DOLookAt(lockedEnemy.position, rotationDuration, AxisConstraint.Y, Vector3.up)
                    .SetEase(Ease.OutSine);
        }
    }

    float AngleToTarget(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — AttackCheck (de CombatScript, adaptado)
    // ══════════════════════════════════════════════════════════
    // In AttackCheck()
    void AttackCheck()
    {
        if (isAttackingEnemy) return;

        if (enemyDetection.CurrentTarget() == null)
        {
            if (enemyManager.AliveEnemyCount() == 0)
            {
                Attack(null, 0);
                return;
            }
            else
            {
                lockedTarget = enemyManager.RandomEnemy();
            }
        }

        if (enemyDetection.InputMagnitude() > .2f)
            lockedTarget = enemyDetection.CurrentTarget();

        if (lockedTarget == null)
            lockedTarget = enemyManager.RandomEnemy();

        float distance = lockedTarget != null ? TargetDistance(lockedTarget) : 0f;
        Attack(lockedTarget, distance);
    }

    void AttackCheckWithSpecified(string attackTrigger)
    {
        if (isAttackingEnemy) return;

        // ---- Target selection (identical to AttackCheck) ----
        if (enemyDetection.CurrentTarget() == null)
        {
            if (enemyManager.AliveEnemyCount() == 0)
            {
                // No enemies: still play the animation without a target
                AttackType(attackTrigger, 1.3f, null, 0);
                return;
            }
            else
            {
                lockedTarget = enemyManager.RandomEnemy();
            }
        }

        if (enemyDetection.InputMagnitude() > .2f)
            lockedTarget = enemyDetection.CurrentTarget();

        if (lockedTarget == null)
            lockedTarget = enemyManager.RandomEnemy();

        // ---- Force the specified attack, ignoring distance ----
        // 1.3f = animation length, 0.65f = movement duration (lunge toward enemy)
        AttackType(attackTrigger, 1.3f, lockedTarget, 0.65f);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — Attack (selección de ataque según distancia y tipo)
// ───────────────────────────────────────────────────────────────────────────

    public void Attack(EnemyScript target, float distance)
    {
        attackslong  = new string[] { "TrRaSwing", "TrSwSwing", "TrRa360", "TrSw360" };
        attacksnear  = new string[] { "TrCrescent", "TrChut" };

        string[] kickAttacks       = new string[] { "TrCrescent", "TrChut" };
        string[] lightSaberAttacks = new string[] { "TrSwSwing", "TrSw360" };
        string[] rapierAttacks     = new string[] { "TrRaSwing", "TrRa360" };

        if (target == null)
        {
            AttackType("TrRaSwing", .2f, null, 0);
            return;
        }

        if (distance < 15)
        {
            string[] attackPool;

            if (distance <= kickRange)
            {
                // Rango patadas
                attackPool = kickAttacks;
            }
            else if (distance <= lightSaberRange)
            {
                // Rango LightSaber — ejecuta aunque no llegue al objetivo exacto
                attackPool = lightSaberAttacks;

                if (distance > rapierRange)
                {
                    Debug.Log("Ataque fuera de rango");
                }
            }
            else if (distance <= rapierRange)
            {
                // Rango Rapier
                attackPool = rapierAttacks;
            }
            else
            {
                // Distancia entre lightSaberRange y 15 → fallback Rapier
                attackPool = rapierAttacks;
            }

            animationCount = (animationCount + 1) % attackPool.Length;
            string attackString = IsLastHit()
                ? attackPool[UnityEngine.Random.Range(0, attackPool.Length)]
                : attackPool[animationCount];

            AttackType(attackString, attackCooldown, target, .65f);
              //if (impulseSource != null)
            //impulseSource.m_ImpulseDefinition.m_AmplitudeGain = Mathf.Max(3, 1 * distance);
        }
        else
        {
            lockedTarget = null;
            AttackType("TrRaSwing", .2f, null, 0);
        }
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — AttackType
    // ══════════════════════════════════════════════════════════
    void AttackType(string attackTrigger, float cooldown, EnemyScript target, float movementDuration)
    {
        animator.SetTrigger(attackTrigger);

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine(IsLastHit() ? 1.5f : cooldown));

        if (IsLastHit())
            StartCoroutine(FinalBlowCoroutine());

        if (target == null) return;

        target.StopMoving();
        MoveTowardsTarget(target, movementDuration);

        IEnumerator AttackCoroutine(float duration)
        {
            isAttackingEnemy = true;
            yield return new WaitForSeconds(duration);
            isAttackingEnemy = false;

            if (lockedTarget != null)
                lockedTarget.ReleaseLock();

            yield return new WaitForSeconds(.2f);
            // Recupera velocidad gradualmente tras el ataque
            speed = 0f;
            DOVirtual.Float(0, 2f, .6f, v => speed = v);
        }

        IEnumerator FinalBlowCoroutine()
        {
            Time.timeScale = .5f;
            if (lastHitCamera != null)    lastHitCamera.SetActive(true);
            if (lastHitFocusObject != null) lastHitFocusObject.position = lockedTarget.transform.position;
            yield return new WaitForSecondsRealtime(2);
            if (lastHitCamera != null)    lastHitCamera.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — MoveTowardsTarget (DOTween hacia el enemigo)
    // ══════════════════════════════════════════════════════════
    void MoveTowardsTarget(EnemyScript target, float duration)
    {
        OnTrajectory?.Invoke(target);
        transform.DOLookAt(target.transform.position, .2f);
        transform.DOMove(TargetOffset(target.transform), duration);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — CounterCheck (Space)
    // ══════════════════════════════════════════════════════════
    // In CounterCheck()
    void CounterCheck()
    {
        if (isCountering || isAttackingEnemy || !enemyManager.AnEnemyIsPreparingAttack())
            return;

        lockedTarget = ClosestCounterEnemy();
        if (lockedTarget == null) return;   // ← important

            // ───── NUEVO: activar lock-on hacia el enemigo que contraatacamos ─────
        lockedEnemy = lockedTarget.transform;
        isLockedOn = true;
        transform.DOKill();
        transform.DOLookAt(lockedEnemy.position, rotationDuration, AxisConstraint.Y, Vector3.up)
                .SetEase(Ease.OutSine);

        OnCounterAttack?.Invoke(lockedTarget);

        if (lockedTarget == null || TargetDistance(lockedTarget) > 2)
        {
            float dist = lockedTarget != null ? TargetDistance(lockedTarget) : 0f;
            Attack(lockedTarget, dist);
            return;
        }

        float duration = .6f;
        animator.SetTrigger("Dodge");
        transform.DOLookAt(lockedTarget.transform.position, .6f);
        transform.DOMove(transform.position + lockedTarget.transform.forward, duration);

        if (counterCoroutine != null) StopCoroutine(counterCoroutine);
        counterCoroutine = StartCoroutine(CounterCoroutine(duration));

        IEnumerator CounterCoroutine(float dur)
        {
            isCountering = true;
            yield return new WaitForSeconds(dur);
            float dist = lockedTarget != null ? TargetDistance(lockedTarget) : 0f;
            Attack(lockedTarget, dist);
            isCountering = false;

                // ✅ LIBERAR TRAS EL CONTRAATAQUE
            if (lockedTarget != null)
                lockedTarget.ReleaseLock();
            
        }
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — HitEvent (llamado por Animation Event al golpear)
    // ══════════════════════════════════════════════════════════
    public void HitEvent()
    {
        if (lockedTarget == null || enemyManager.AliveEnemyCount() == 0) return;

        OnHit?.Invoke(lockedTarget);

        //if (punchParticle != null && punchPosition != null)
        //    punchParticle.PlayParticleAtPosition(punchPosition.position);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — RecibirDanyo (EnemyScript llama DamageEvent())
    // Equivale a CombatScript.DamageEvent()
    // ══════════════════════════════════════════════════════════
    public void RecibirDanyo()
    {
        animator.SetTrigger("NinjaHit");
        DamageEvent?.Invoke();

        if (damageCoroutine != null) StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(DamageCoroutine());



        IEnumerator DamageCoroutine()
        {
            speed = 0f;
            yield return new WaitForSeconds(.5f);
            speed = 2f;
            DOVirtual.Float(0, 2f, .6f, v => speed = v);
        }
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — Helpers
    // ══════════════════════════════════════════════════════════
    float TargetDistance(EnemyScript target)
    {
        if (target == null) 
            return float.MaxValue;  // or some large distance, or handle differently
        return Vector3.Distance(transform.position, target.transform.position);
    }
    public Vector3 TargetOffset(Transform target)
        => Vector3.MoveTowards(target.position, transform.position, .95f);

    // ClosestCounterEnemy() as shown above
    // ClosestCounterEnemy() as shown above
    EnemyScript ClosestCounterEnemy()
    {
        float minDistance = 100f;
        int finalIndex = -1;

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript enemy = enemyManager.allEnemies[i].enemyScript;
            if (enemy != null && enemy.IsPreparingAttack())
            {
                float d = Vector3.Distance(transform.position, enemy.transform.position);
                if (d < minDistance) 
                { 
                    minDistance = d; 
                    finalIndex = i; 
                }
            }
        }

        return finalIndex >= 0 ? enemyManager.allEnemies[finalIndex].enemyScript : null;
    }
    bool IsLastHit()
    {
        if (lockedTarget == null) return false;
        return enemyManager.AliveEnemyCount() == 1 && lockedTarget.currentHealth <= 1;
    }

    // ══════════════════════════════════════════════════════════
    // LOCK-ON — Métodos
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Finds the most-forward enemy inside detectionRadius and locks onto it.
    /// Uses Physics.OverlapSphere + Vector3.Dot (Mix and Jam style).
    /// </summary>
    void TryLockOn()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        Transform bestCandidate = null;
        float bestDot = -Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            // Only consider objects tagged "Enemy"
            if (!hit.CompareTag("Enemy"))
                continue;

            // Skip if the enemy is directly behind us (dot would be negative)
            Vector3 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToEnemy);

            if (dot > bestDot)
            {
                bestDot       = dot;
                bestCandidate = hit.transform;
            }
        }

        if (bestCandidate != null)
        {
            lockedEnemy = bestCandidate;
            isLockedOn  = true;

            // Kick off the very first rotation tween immediately
            transform.DOKill();
            transform
                .DOLookAt(lockedEnemy.position, rotationDuration,
                          AxisConstraint.Y, Vector3.up)
                .SetEase(Ease.OutSine);
        }
    }

    /// <summary>
    /// Releases the current lock-on target and kills any running rotation tween.
    /// </summary>
    void UnlockTarget()
    {
        isLockedOn  = false;
        lockedEnemy = null;
        transform.DOKill();
        manualUnlock = true;   // evita que se auto‑enganche
    }

    // ══════════════════════════════════════════════════════════
    // TUS CORUTINAS ORIGINALES (sin cambios de lógica)
    // ══════════════════════════════════════════════════════════
    IEnumerator DanceSpin()
    {
        for (int i = 0; i < 360; i += 30)
        {
            transform.Rotate(0, 30, 0);
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator JumpCountdown()
    {
        for (int i = 3; i > 0; i--)
        {
            Debug.Log(i);
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("¡Jump!");
        controller.Move(Vector3.up * 2f);
    }


    // ══════════════════════════════════════════════════════════
    // ESTADOS SLEEP / CONFUSED — Debug keys 7 / 8 / 9
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Aplica un estado especial al enemigo actualmente locked-on.
    /// Teclas: 7 = Sleep, 8 = Confused.
    /// </summary>

    public Damageable.EstadoEspecial GetEstadoSeleccionado()
    {
        return estadoSeleccionado;
    }

    IEnumerator Ra360()    { isAttackingEnemy = true; animator.SetTrigger("TrRa360");   yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Sw360()    { isAttackingEnemy = true; animator.SetTrigger("TrSw360");   yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator SwSwing()  { isAttackingEnemy = true; animator.SetTrigger("TrSwSwing"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator RaSwing()  { isAttackingEnemy = true; animator.SetTrigger("TrRaSwing"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Crescent()  { isAttackingEnemy = true; animator.SetTrigger("TrCrescent"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Chut()     { isAttackingEnemy = true; animator.SetTrigger("TrChut");    yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    // ══════════════════════════════════════════════════════════
    // HITBOXES — llamadas desde Animation Events
    // ══════════════════════════════════════════════════════════
    public void EnableRapierHitbox()     { RapierCollider.enabled = true;      Rapier.Reactivar(); }
    public void EnableLightSaberHitbox() { LightSaberCollider.enabled = true;  LightSaber.Reactivar(); }
    public void EnableLeftKickHitbox()   { LeftKickCollider.enabled = true;    LeftKick.Reactivar(); }
    public void EnableRightKickHitbox()  { RightKickCollider.enabled = true;   RightKick.Reactivar(); }
    public void DisableRapierHitbox()     { RapierCollider.enabled = false; }
    public void DisableLightSaberHitbox() { LightSaberCollider.enabled = false; }
    public void DisableLeftKickHitbox()   { LeftKickCollider.enabled = false; }
    public void DisableRightKickHitbox()  { RightKickCollider.enabled = false; }
}