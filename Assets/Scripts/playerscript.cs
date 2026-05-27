// ===== FILE: ./playerscript.cs =====

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(CharacterController))]
public class SimpleWalk : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════
    // MOVIMIENTO
    // ══════════════════════════════════════════════════════════
    public float speed = 2f;
    public Animator animator;

    [Header("Walk Animation Regulation")]
    [Tooltip("Multiplier for the walking animation speed. Set to 0.25 if you previously needed x4 speed.")]
    public float walkAnimationSpeed = 0.25f;

    private CharacterController controller;
    private Vector3 moveDirection;

    // ══════════════════════════════════════════════════════════
    // LOCK-ON SYSTEM
    // ══════════════════════════════════════════════════════════
    [Header("Lock-On System")]
    public float detectionRadius = 8f;
    public float rotationDuration = 0.15f;

    [Header("Lock-On UI")]
    public GameObject lockOnPrefab;
    public float lockOnHeightOffset = 2f;
    public float lockOnSmoothTime = 0.1f;

    [Tooltip("Minimum angle difference (degrees) before re-triggering DOTween rotation.")]
    public float rotationThreshold = 2f;

    private GameObject lockOnInstance;
    private Transform lockOnTransform;
    private Camera lockOnCamera;
    private Vector3 lockOnVelocity;

    public float kickRange = 0.3f;
    public float lightSaberRange = 4f;
    public float rapierRange = 1f;

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
    // COMBAT — Settings
    // ══════════════════════════════════════════════════════════
    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1.3f;

    [Header("Combat State")]
    public Vector2 moveAxis;
    public bool isAttackingEnemy = false;
    public bool isCountering = false;

    [Header("Combat References")]
    [SerializeField] private Transform punchPosition;
    [SerializeField] private GameObject lastHitCamera;
    [SerializeField] private Transform lastHitFocusObject;
    [SerializeField] private EnemyDetection enemyDetection;

    [Header("Combat Events")]
    public UnityEvent<EnemyScript> OnHit;
    public UnityEvent<EnemyScript> OnCounterAttack;
    public UnityEvent<EnemyScript> OnTrajectory;

    [Header("Estado especial seleccionado")]
    public Damageable.EstadoEspecial estadoSeleccionado = Damageable.EstadoEspecial.None;

    public System.Action DamageEvent;

    // ══════════════════════════════════════════════════════════
    // COMBAT — Internals
    // ══════════════════════════════════════════════════════════
    private EnemyManager enemyManager;
    private EnemyScript lockedTarget;

    private Coroutine counterCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine damageCoroutine;

    private int animationCount = 0;
    private string[] attacks;
    private string[] attackslong;
    private string[] attacksnear;

    private bool manualUnlock = false;

    // ══════════════════════════════════════════════════════════
    // START
    // ══════════════════════════════════════════════════════════
    void Start()
    {
        controller = GetComponent<CharacterController>();
        enemyManager = FindAnyObjectByType<EnemyManager>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (enemyDetection == null)
            enemyDetection = GetComponentInChildren<EnemyDetection>();
    }

    // ══════════════════════════════════════════════════════════
    // UPDATE
    // ══════════════════════════════════════════════════════════
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveAxis = new Vector2(horizontal, vertical);

        // ── Auto-lock si no hay lock y hay enemigos vivos ──────
        if (!isLockedOn && !manualUnlock && enemyManager.AliveEnemyCount() > 0)
        {
            // Solo auto-lockar si aún no tenemos target válido en este frame
            if (lockedTarget == null || !lockedTarget.IsAttackable())
            {
                TryLockOn();
            }
            else
            {
                // Recuperar lock-on hacia el target que ya teníamos
                Transform t = lockedTarget.transform;
                if (t != null)
                {
                    lockedEnemy = t;
                    isLockedOn = true;
                    transform.DOKill();
                    transform.DOLookAt(lockedEnemy.position, rotationDuration, AxisConstraint.Y, Vector3.up)
                        .SetEase(Ease.OutSine);
                    CreateLockOnIndicator(lockedEnemy);
                }
            }
        }

        // ── Lock-On Toggle (Tab) ──────────────────────────────
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isLockedOn)
                UnlockTarget();
            else
            {
                manualUnlock = false;
                TryLockOn();
            }
        }

        // ── Scroll / Q / E para cambiar objetivo ─────────────
        if (isLockedOn && Mathf.Abs(Input.mouseScrollDelta.y) > 0.1f)
        {
            CycleLockTarget(Input.mouseScrollDelta.y > 0 ? 1 : -1);
        }
        if (isLockedOn)
        {
            if (Input.GetKeyDown(KeyCode.E)) CycleLockTarget(1);
            else if (Input.GetKeyDown(KeyCode.Q)) CycleLockTarget(-1);
        }

        // ── Auto-Unlock si el enemigo murió o se alejó ────────
        if (isLockedOn)
        {
            if (lockedEnemy == null ||
                Vector3.Distance(transform.position, lockedEnemy.position) > detectionRadius)
            {
                UnlockTarget();
            }
        }

        // ── Movimiento ────────────────────────────────────────
        if (isLockedOn && lockedEnemy != null && !isAttackingEnemy)
        {
            Vector3 toEnemy = (lockedEnemy.position - transform.position).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, toEnemy);
            moveDirection = (toEnemy * vertical + right * horizontal).normalized;

            Vector3 lookDir = new Vector3(toEnemy.x, 0f, toEnemy.z);
            if (lookDir != Vector3.zero)
            {
                float angleDiff = Vector3.Angle(transform.forward, lookDir);
                if (angleDiff > rotationThreshold)
                {
                    transform.DOKill();
                    transform
                        .DOLookAt(lockedEnemy.position, rotationDuration, AxisConstraint.Y, Vector3.up)
                        .SetEase(Ease.OutSine);
                }
            }

            Debug.DrawLine(transform.position, lockedEnemy.position, Color.cyan);

            if (moveDirection.magnitude >= 0.1f)
                controller.Move(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

            if (moveDirection.magnitude >= 0.1f && !isAttackingEnemy)
            {
                transform.forward = moveDirection;
                controller.Move(moveDirection * speed * Time.deltaTime);
            }
        }

        // ── Animator ──────────────────────────────────────────
        Vector3 localMove = transform.InverseTransformDirection(moveDirection);
        animator.SetFloat("VelocityX", localMove.x, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityY", localMove.z, 0.1f, Time.deltaTime);
        animator.SetFloat("WalkAnimSpeed", walkAnimationSpeed);
        animator.SetBool("isWalking", moveDirection.magnitude > 0f && !isAttackingEnemy);

        // ── Selección de estado especial ──────────────────────
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.Sleep;
            Debug.Log("[Estado] Modo seleccionado: SLEEP");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.Confused;
            Debug.Log("[Estado] Modo seleccionado: CONFUSED");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.None;
            Debug.Log("[Estado] Modo seleccionado: NONE");
        }

        // ── Ataques ───────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.Mouse0)) AttackCheck();
        if (Input.GetKeyDown(KeyCode.Mouse1)) AttackCheckWithSpecified("TrSwSwing");
        if (Input.GetKeyDown(KeyCode.R)) AttackCheckWithSpecified("TrRa360");
        if (Input.GetKeyDown(KeyCode.F)) AttackCheckWithSpecified("TrSw360");
        if (Input.GetKeyDown(KeyCode.Z)) AttackCheckWithSpecified("TrRaSwing");
        if (Input.GetKeyDown(KeyCode.C)) AttackCheckWithSpecified("TrCrescent");
        if (Input.GetKeyDown(KeyCode.X)) AttackCheckWithSpecified("TrChut");
        if (Input.GetKeyDown(KeyCode.Space)) CounterCheck();
    }

    void LateUpdate()
    {
        UpdateLockOnIndicator();
    }

    // ══════════════════════════════════════════════════════════
    // LOCK-ON — Cycle target
    // ══════════════════════════════════════════════════════════
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

            // ── Sincronizar lockedTarget con el nuevo enemigo ──
            EnemyScript es = best.GetComponent<EnemyScript>();
            if (es != null && es.IsAttackable())
                lockedTarget = es;

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
    // COMBAT — AttackCheck
    // ══════════════════════════════════════════════════════════
    /// <summary>
    /// Selecciona el objetivo de ataque con esta prioridad:
    ///   1. Si hay lock-on → usarlo SIEMPRE.
    ///   2. Si no hay lock-on pero hay input detection → usar CurrentTarget().
    ///   3. Fallback → aleatorio.
    /// 
    /// Así el ataque SIEMPRE se orienta al enemigo seleccionado.
    /// </summary>
    void AttackCheck()
    {
        if (isAttackingEnemy) return;

        // ── 1) PRIORIDAD: Lock-on activo ─────────────────────
        if (isLockedOn && lockedEnemy != null)
        {
            EnemyScript es = lockedEnemy.GetComponent<EnemyScript>();
            if (es != null && es.IsAttackable())
            {
                lockedTarget = es;
                float distance = TargetDistance(lockedTarget);
                Attack(lockedTarget, distance);
                return;
            }
            else
            {
                // El enemigo del lock-on murió → desbloquear
                UnlockTarget();
            }
        }

        // ── 2) Detección por input direction ──────────────────
        if (enemyDetection.InputMagnitude() > 0.2f)
        {
            EnemyScript detected = enemyDetection.CurrentTarget();
            if (detected != null && detected.IsAttackable())
            {
                lockedTarget = detected;
                float distance = TargetDistance(lockedTarget);
                Attack(lockedTarget, distance);
                return;
            }
        }

        // ── 3) Fallback: aleatorio ──────────────────────────
        if (enemyManager.AliveEnemyCount() > 0)
        {
            lockedTarget = enemyManager.RandomEnemy();
            if (lockedTarget != null)
            {
                float distance = TargetDistance(lockedTarget);
                Attack(lockedTarget, distance);
                return;
            }
        }

        // ── 4) Sin enemigos → atacar al aire ────────────────
        Attack(null, 0);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — AttackCheckWithSpecified
    // ══════════════════════════════════════════════════════════
    void AttackCheckWithSpecified(string attackTrigger)
    {
        if (isAttackingEnemy) return;

        // MISMA LÓGICA DE SELECCIÓN que AttackCheck()
        if (isLockedOn && lockedEnemy != null)
        {
            EnemyScript es = lockedEnemy.GetComponent<EnemyScript>();
            if (es != null && es.IsAttackable())
            {
                lockedTarget = es;
                AttackType(attackTrigger, 1.3f, lockedTarget, 0.65f);
                return;
            }
            else
            {
                UnlockTarget();
            }
        }

        if (enemyDetection.InputMagnitude() > 0.2f)
        {
            EnemyScript detected = enemyDetection.CurrentTarget();
            if (detected != null && detected.IsAttackable())
            {
                lockedTarget = detected;
                AttackType(attackTrigger, 1.3f, lockedTarget, 0.65f);
                return;
            }
        }

        if (enemyManager.AliveEnemyCount() > 0)
        {
            lockedTarget = enemyManager.RandomEnemy();
            if (lockedTarget != null)
            {
                AttackType(attackTrigger, 1.3f, lockedTarget, 0.65f);
                return;
            }
        }

        // Sin enemigos
        AttackType(attackTrigger, 1.3f, null, 0);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — Attack (selección de tipo según distancia)
    // ══════════════════════════════════════════════════════════
    public void Attack(EnemyScript target, float distance)
    {
        string[] kickAttacks = new string[] { "TrCrescent", "TrChut" };
        string[] lightSaberAttacks = new string[] { "TrSwSwing", "TrSw360" };
        string[] rapierAttacks = new string[] { "TrRaSwing", "TrRa360" };

        if (target == null)
        {
            AttackType("TrRaSwing", 0.2f, null, 0);
            return;
        }

        if (distance < 15f)
        {
            string[] attackPool;

            if (distance <= kickRange)
                attackPool = kickAttacks;
            else if (distance <= lightSaberRange)
                attackPool = lightSaberAttacks;
            else if (distance <= rapierRange)
                attackPool = rapierAttacks;
            else
                attackPool = rapierAttacks;

            animationCount = (animationCount + 1) % attackPool.Length;
            string attackString = IsLastHit()
                ? attackPool[UnityEngine.Random.Range(0, attackPool.Length)]
                : attackPool[animationCount];

            AttackType(attackString, attackCooldown, target, 0.65f);
        }
        else
        {
            // Fuera de rango → mantener target pero atacar al aire
            AttackType("TrRaSwing", 0.2f, null, 0);
        }
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — AttackType (ejecuta el ataque)
    // ══════════════════════════════════════════════════════════
    void AttackType(string attackTrigger, float cooldown, EnemyScript target, float movementDuration)
    {
        animator.SetTrigger(attackTrigger);

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine(
            IsLastHit() ? 1.5f : cooldown, target, movementDuration));

        if (IsLastHit())
            StartCoroutine(FinalBlowCoroutine());
    }

    IEnumerator AttackCoroutine(float duration, EnemyScript target, float movementDuration)
    {
        isAttackingEnemy = true;

        if (target != null)
        {
            target.StopMoving();
            MoveTowardsTarget(target, movementDuration);
        }

        yield return new WaitForSeconds(duration);
        isAttackingEnemy = false;

        if (lockedTarget != null)
            lockedTarget.ReleaseLock();

        // Recupera velocidad gradualmente
        speed = 0f;
        yield return new WaitForSeconds(0.2f);
        DOVirtual.Float(0, 2f, 0.6f, v => speed = v);
    }

    IEnumerator FinalBlowCoroutine()
    {
        Time.timeScale = 0.5f;
        if (lastHitCamera != null) lastHitCamera.SetActive(true);
        if (lastHitFocusObject != null)
            lastHitFocusObject.position = lockedTarget != null ? lockedTarget.transform.position : transform.position;
        yield return new WaitForSecondsRealtime(2f);
        if (lastHitCamera != null) lastHitCamera.SetActive(false);
        Time.timeScale = 1f;
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — MoveTowardsTarget
    // ══════════════════════════════════════════════════════════
    void MoveTowardsTarget(EnemyScript target, float duration)
    {
        if (target == null) return;

        OnTrajectory?.Invoke(target);
        transform.DOLookAt(target.transform.position, 0.2f, AxisConstraint.Y, Vector3.up);
        transform.DOMove(TargetOffset(target.transform), duration);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — CounterCheck
    // ══════════════════════════════════════════════════════════
    /// <summary>
    /// Contraataca al enemigo MÁS CERCANO que esté preparando un ataque.
    /// Se orienta hacia él y ejecuta el contraataque.
    /// </summary>
    void CounterCheck()
    {
        if (isCountering || isAttackingEnemy) return;

        // ── Buscar enemigo más cercano preparando ataque ─────
        EnemyScript closest = ClosestCounterEnemy();
        if (closest == null) return;

        lockedTarget = closest;

        // ── Activar lock-on hacia el enemigo del contraataque ─
        Transform targetTransform = closest.transform;
        if (targetTransform != null)
        {
            lockedEnemy = targetTransform;
            isLockedOn = true;

            transform.DOKill();
            transform.DOLookAt(lockedEnemy.position, rotationDuration, AxisConstraint.Y, Vector3.up)
                .SetEase(Ease.OutSine);

            // Asegurar que el indicador visual exista
            if (lockOnInstance == null)
                CreateLockOnIndicator(lockedEnemy);
        }

        OnCounterAttack?.Invoke(lockedTarget);

        // ── Si está demasiado lejos, atacar avanzando ───────
        float dist = TargetDistance(lockedTarget);
        if (dist > 2f)
        {
            Attack(lockedTarget, dist);
            return;
        }

        // ── Cerca: executar dodge + contraataque ─────────────
        float duration = 0.6f;
        animator.SetTrigger("Dodge");

        // Orientar visualmente al enemigo
        if (targetTransform != null)
        {
            Vector3 dirToEnemy = (targetTransform.position - transform.position).normalized;
            dirToEnemy.y = 0;
            if (dirToEnemy != Vector3.zero)
                transform.forward = dirToEnemy;
        }

        // Mover hacia atrás del enemigo
        transform.DOMove(transform.position + (targetTransform != null ? targetTransform.forward : transform.forward), duration);

        if (counterCoroutine != null) StopCoroutine(counterCoroutine);
        counterCoroutine = StartCoroutine(CounterCoroutine(closest, duration));
    }

    IEnumerator CounterCoroutine(EnemyScript originalTarget, float dur)
    {
        isCountering = true;
        yield return new WaitForSeconds(dur);

        // ── Verificar que el target sigue vivo ──────────────
        EnemyScript currentTarget = originalTarget;
        if (currentTarget == null || !currentTarget.IsAttackable())
        {
            // El enemigo murió durante el dodge → buscar otro
            currentTarget = ClosestCounterEnemy();
            if (currentTarget == null)
            {
                // No hay nadie para contraatacar
                isCountering = false;
                yield break;
            }
            lockedTarget = currentTarget;
        }

        float dist = TargetDistance(currentTarget);
        Attack(currentTarget, dist);
        isCountering = false;

        if (lockedTarget != null)
            lockedTarget.ReleaseLock();
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — HitEvent (Animation Event)
    // ══════════════════════════════════════════════════════════
    public void HitEvent()
    {
        if (lockedTarget == null || enemyManager.AliveEnemyCount() == 0) return;

        OnHit?.Invoke(lockedTarget);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — RecibirDanyo
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
            yield return new WaitForSeconds(0.5f);
            speed = 2f;
            DOVirtual.Float(0, 2f, 0.6f, v => speed = v);
        }
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — Helpers
    // ══════════════════════════════════════════════════════════
    float TargetDistance(EnemyScript target)
    {
        if (target == null) return float.MaxValue;
        return Vector3.Distance(transform.position, target.transform.position);
    }

    public Vector3 TargetOffset(Transform target)
        => Vector3.MoveTowards(target.position, transform.position, 0.95f);

    /// <summary>
    /// Encuentra el enemigo MÁS CERCANO que esté preparando un ataque.
    /// </summary>
    EnemyScript ClosestCounterEnemy()
    {
        float minDistance = Mathf.Infinity;
        EnemyScript best = null;

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript enemy = enemyManager.allEnemies[i].enemyScript;

            // ── Validaciones robustas ──────────────────────────
            if (enemy == null) continue;
            if (!enemy.isActiveAndEnabled) continue;
            if (!enemy.IsAttackable()) continue;
            if (!enemy.IsPreparingAttack()) continue;

            float d = Vector3.Distance(transform.position, enemy.transform.position);
            if (d < minDistance)
            {
                minDistance = d;
                best = enemy;
            }
        }

        return best;
    }

    bool IsLastHit()
    {
        if (lockedTarget == null) return false;
        return enemyManager.AliveEnemyCount() == 1 && lockedTarget.currentHealth <= 1;
    }

    // ══════════════════════════════════════════════════════════
    // LOCK-ON — TryLockOn
    // ══════════════════════════════════════════════════════════
    void TryLockOn()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        Transform bestCandidate = null;
        float bestDot = -Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector3 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToEnemy);

            if (dot > bestDot)
            {
                bestDot = dot;
                bestCandidate = hit.transform;
            }
        }

        if (bestCandidate != null)
        {
            lockedEnemy = bestCandidate;
            isLockedOn = true;

            // Sincronizar lockedTarget
            EnemyScript es = bestCandidate.GetComponent<EnemyScript>();
            if (es != null && es.IsAttackable())
                lockedTarget = es;

            transform.DOKill();
            transform.DOLookAt(lockedEnemy.position, rotationDuration, AxisConstraint.Y, Vector3.up)
                .SetEase(Ease.OutSine);

            CreateLockOnIndicator(lockedEnemy);
        }
    }

    void CreateLockOnIndicator(Transform target)
    {
        if (lockOnPrefab == null)
        {
            Debug.LogWarning("[Lock-On] lockOnPrefab is not assigned in the Inspector.");
            return;
        }

        if (lockOnInstance != null)
            Destroy(lockOnInstance);

        Vector3 spawnPos = target.position + Vector3.up * lockOnHeightOffset;
        lockOnInstance = Instantiate(lockOnPrefab, spawnPos, Quaternion.identity);
        lockOnTransform = lockOnInstance.transform;

        if (lockOnCamera == null)
            lockOnCamera = Camera.main;
    }

    void UpdateLockOnIndicator()
    {
        if (!isLockedOn || lockOnInstance == null || lockedEnemy == null) return;

        Vector3 targetPos = lockedEnemy.position + Vector3.up * lockOnHeightOffset;
        lockOnTransform.position = Vector3.SmoothDamp(
            lockOnTransform.position, targetPos, ref lockOnVelocity, lockOnSmoothTime);

        if (lockOnCamera != null)
            lockOnTransform.forward = lockOnCamera.transform.forward;
    }

    void UnlockTarget()
    {
        isLockedOn = false;
        lockedEnemy = null;
        transform.DOKill();
        manualUnlock = true;

        if (lockOnInstance != null)
        {
            Destroy(lockOnInstance);
            lockOnInstance = null;
            lockOnTransform = null;
        }
    }

    // ══════════════════════════════════════════════════════════
    // API pública
    // ══════════════════════════════════════════════════════════
    public Damageable.EstadoEspecial GetEstadoSeleccionado()
    {
        return estadoSeleccionado;
    }

    // ══════════════════════════════════════════════════════════
    // Hitbox enable/disable (Animation Events)
    // ══════════════════════════════════════════════════════════
    public void EnableRapierHitbox() { RapierCollider.enabled = true; Rapier.Reactivar(); }
    public void EnableLightSaberHitbox() { LightSaberCollider.enabled = true; LightSaber.Reactivar(); }
    public void EnableLeftKickHitbox() { LeftKickCollider.enabled = true; LeftKick.Reactivar(); }
    public void EnableRightKickHitbox() { RightKickCollider.enabled = true; RightKick.Reactivar(); }
    public void DisableRapierHitbox() { RapierCollider.enabled = false; }
    public void DisableLightSaberHitbox() { LightSaberCollider.enabled = false; }
    public void DisableLeftKickHitbox() { LeftKickCollider.enabled = false; }
    public void DisableRightKickHitbox() { RightKickCollider.enabled = false; }

    // ══════════════════════════════════════════════════════════
    // Corrutinas originales (sin cambios)
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

    IEnumerator Ra360() { isAttackingEnemy = true; animator.SetTrigger("TrRa360"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Sw360() { isAttackingEnemy = true; animator.SetTrigger("TrSw360"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator SwSwing() { isAttackingEnemy = true; animator.SetTrigger("TrSwSwing"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator RaSwing() { isAttackingEnemy = true; animator.SetTrigger("TrRaSwing"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Crescent() { isAttackingEnemy = true; animator.SetTrigger("TrCrescent"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Chut() { isAttackingEnemy = true; animator.SetTrigger("TrChut"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
}