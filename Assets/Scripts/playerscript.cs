// ===== FILE: ./SimpleWalk.cs =====

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
    public float walkAnimationSpeed = 0.25f;

    private CharacterController controller;
    private Vector3 moveDirection;

    // ══════════════════════════════════════════════════════════
    // LOCK-ON SYSTEM
    // ══════════════════════════════════════════════════════════
    [Header("Lock-On System")]
    public float detectionRadius = 10f;
    public float rotationDuration = 0.15f;

    [Header("Lock-On UI")]
    public GameObject lockOnPrefab;
    public float lockOnHeightOffset = 2f;
    public float lockOnSmoothTime = 0.1f;
    public float rotationThreshold = 2f;

    private GameObject lockOnInstance;
    private Transform lockOnTransform;
    private Camera lockOnCamera;
    private Vector3 lockOnVelocity;

    public float kickRange = 1.5f;
    public float lightSaberRange = 5f;
    public float rapierRange = 3f;

    private Transform lockedEnemy;
    private bool isLockedOn = false;
    private bool manualUnlock = false;

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
    // INTERNALS
    // ══════════════════════════════════════════════════════════
    private EnemyManager enemyManager;
    private EnemyScript lockedTarget;

    private Coroutine counterCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine damageCoroutine;
    private Coroutine autoSwitchToAttackingCoroutine;
    private Coroutine autoLockCoroutine;

    private PlayerHUDController _hud;

    private string[] attacks;

    private const bool LOG_LOCK = true;
    private const bool LOG_COMBAT = true;
    private const bool LOG_COUNTER = true;

    // ══════════════════════════════════════════════════════════
    // COUNTER KEYS — Tecla directa, sin mantener
    // ══════════════════════════════════════════════════════════
    // V       → Counter KICK aleatorio
    // B       → Counter LIGHTSABER aleatorio
    // N       → Counter RAPIER aleatorio
    // Space   → Counter normal (predeterminado, random entre todos)

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

        _hud = FindAnyObjectByType<PlayerHUDController>();

        attacks = new string[] {
            "TrCrescent", "TrChut",
            "TrRaSwing", "TrRa360",
            "TrSwSwing", "TrSw360"
        };
    }

    // ══════════════════════════════════════════════════════════
    // UPDATE
    // ══════════════════════════════════════════════════════════
    void Update()
    {
        if (enemyManager == null || enemyManager.gameObject == null || !enemyManager.isActiveAndEnabled)
            enemyManager = FindAnyObjectByType<EnemyManager>();

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveAxis = new Vector2(horizontal, vertical);

        // ── Auto-lock al entrar o al morir target ────────────
        GestionarAutoLock();

        // ── Auto-switch al enemigo que te va a atacar ────────
        GestionarAutoSwitchAAttacker();

        // ── Lock-On Toggle (Tab) ──────────────────────────────
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isLockedOn)
                UnlockTarget();
            else
            {
                manualUnlock = false;
                ForceLockOnBest();
            }
        }

        // ── Cambiar objetivo (Q / E) ─────────────────────────
        if (isLockedOn)
        {
            if (Input.GetKeyDown(KeyCode.E)) CycleLockTarget(1);
            else if (Input.GetKeyDown(KeyCode.Q)) CycleLockTarget(-1);
        }

        // ── Auto-Unlock si el enemigo murió o se alejó ────────
        ValidarLockActual();

        // ── Movimiento ────────────────────────────────────────
        ProcesarMovimiento(horizontal, vertical);

        // ── Animator ──────────────────────────────────────────
        ProcesarAnimator();

        // ── Selección de estado especial (7/8/9) ──────────────
        ProcesarEstadoEspecial();

        // ══════════════════════════════════════════════════════
        // ATAQUES — Solo si NO está contraatacando
        // ══════════════════════════════════════════════════════
        if (!isCountering)
        {
            if (Input.GetKeyDown(KeyCode.CapsLock)) AttackCheck();
            if (Input.GetKeyDown(KeyCode.LeftShift)) AttackCheckKick();
            if (Input.GetKeyDown(KeyCode.Mouse1)) AttackCheckRapier();
            if (Input.GetKeyDown(KeyCode.Mouse0)) AttackCheckSW();
            if (Input.GetKeyDown(KeyCode.V)) AttackCheckWithSpecified("TrCrescent");
            if (Input.GetKeyDown(KeyCode.B)) AttackCheckWithSpecified("TrChut");
            if (Input.GetKeyDown(KeyCode.N)) AttackCheckWithSpecified("TrRaSwing");
            if (Input.GetKeyDown(KeyCode.F)) AttackCheckWithSpecified("TrRa360");
            if (Input.GetKeyDown(KeyCode.R)) AttackCheckWithSpecified("TrSwSwing");
            if (Input.GetKeyDown(KeyCode.T)) AttackCheckWithSpecified("TrSw360");
        }

        // ══════════════════════════════════════════════════════
        // COUNTERS — Tecla directa, sin mantener
        // ══════════════════════════════════════════════════════
        if (Input.GetKeyDown(KeyCode.Z)) EjecutarCounter("KICK");
        if (Input.GetKeyDown(KeyCode.X)) EjecutarCounter("LIGHTSABER");
        if (Input.GetKeyDown(KeyCode.C)) EjecutarCounter("RAPIER");
        if (Input.GetKeyDown(KeyCode.Space)) EjecutarCounter("DEFAULT");
    }

    void LateUpdate()
    {
        UpdateLockOnIndicator();
    }

    // ══════════════════════════════════════════════════════════
    // AUTO-LOCK
    // ══════════════════════════════════════════════════════════
    /// <summary>
    /// Si no hay lock-on activo (porque murió el enemigo o
    /// porque está en escena nueva), busca automáticamente
    /// al enemigo más cercano en rango.
    /// </summary>
    void GestionarAutoLock()
    {
        // Si el usuario pulsó Tab para desbloquear, no auto-lockear
        if (manualUnlock) return;

        // Si ya está lockeado y el target sigue vivo → OK
        if (isLockedOn && EstaEnemyVivoYActivo(lockedEnemy))
            return;

        // Si el lock está activo pero el target murió → desbloquear
        if (isLockedOn && lockedEnemy != null && !EstaEnemyVivoYActivo(lockedEnemy))
        {
            DebugLock($"[AutoLock] Target '{lockedEnemy.name}' murió. Buscando siguiente...");
            UnlockTargetSilent();
        }

        // Si no hay manager o no hay enemigos, nada que hacer
        if (enemyManager == null || enemyManager.aliveEnemyCount == 0)
            return;

        // Intentar auto-lockear al más cercano
        ForceLockOnBest();
    }

    /// <summary>
    /// Cambia el lock al enemigo que está preparando un ataque
    /// contra el jugador, si es diferente al actual.
    /// Solo si el jugador no está atacando ni counterando.
    /// </summary>
    void GestionarAutoSwitchAAttacker()
    {
        if (enemyManager == null) return;
        if (isAttackingEnemy || isCountering) return;

        // Buscar cualquier enemigo que esté preparando ataque
        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript e = enemyManager.allEnemies[i].enemyScript;

            if (e == null) continue;
            if (!e.isActiveAndEnabled) continue;
            if (!e.IsAttackable()) continue;
            if (!e.IsPreparingAttack()) continue;

            // Si es diferente al lock actual, cambiar
            if (lockedEnemy != e.transform)
            {
                DebugLock($"[AutoSwitch] Cambiando lock a atacante: '{e.name}'");
                LockOnSpecific(e.transform);
            }

            return; // Solo el primero que encuentra
        }
    }

    /// <summary>
    /// Valida si el lock actual sigue siendo válido.
    /// Si murió o se alejó demasiado, desbloquear.
    /// </summary>
    void ValidarLockActual()
    {
        if (!isLockedOn || lockedEnemy == null) return;

        // ── Verificar si sigue vivo ──────────────────────────
        if (!EstaEnemyVivoYActivo(lockedEnemy))
        {
            DebugLock($"[Validar] '{lockedEnemy.name}' ya no está vivo/activo. Auto-switch...");
            UnlockTargetSilent();
            manualUnlock = false; // Permitir auto-lock inmediato
            ForceLockOnBest();
            return;
        }

        // ── Verificar distancia ──────────────────────────────
        float dist = Vector3.Distance(transform.position, lockedEnemy.position);
        if (dist > detectionRadius)
        {
            DebugLock($"[Validar] '{lockedEnemy.name}' fuera de rango ({dist:F1}m > {detectionRadius}m). Desbloqueando...");
            UnlockTargetSilent();
            manualUnlock = false;
        }
    }

    // ══════════════════════════════════════════════════════════
    // MOVIMIENTO
    // ══════════════════════════════════════════════════════════
    void ProcesarMovimiento(float horizontal, float vertical)
    {
        if (isLockedOn && lockedEnemy != null && !isAttackingEnemy)
        {
            Vector3 toEnemy = (lockedEnemy.position - transform.position).normalized;
            toEnemy.y = 0;

            Vector3 right = Vector3.Cross(Vector3.up, toEnemy);
            right.y = 0;
            right.Normalize();

            moveDirection = (toEnemy * vertical + right * horizontal).normalized;

            // Rotar suavemente hacia el enemigo
            if (toEnemy != Vector3.zero)
            {
                float angleDiff = Vector3.Angle(transform.forward, toEnemy);
                if (angleDiff > rotationThreshold)
                {
                    transform.DOKill();
                    transform.DOLookAt(
                        lockedEnemy.position + Vector3.up * 1.5f,
                        rotationDuration,
                        AxisConstraint.Y,
                        Vector3.up
                    ).SetEase(Ease.OutSine);
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
                // Rotar hacia dirección de movimiento
                if (moveDirection != Vector3.zero)
                {
                    transform.DOKill();
                    transform.DORotateQuaternion(
                        Quaternion.LookRotation(moveDirection),
                        rotationDuration
                    );
                }
                controller.Move(moveDirection * speed * Time.deltaTime);
            }
        }
    }

    // ══════════════════════════════════════════════════════════
    // ANIMATOR
    // ══════════════════════════════════════════════════════════
    void ProcesarAnimator()
    {
        Vector3 localMove = transform.InverseTransformDirection(moveDirection);
        animator.SetFloat("VelocityX", localMove.x, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityY", localMove.z, 0.1f, Time.deltaTime);
        animator.SetFloat("WalkAnimSpeed", walkAnimationSpeed);
        animator.SetBool("isWalking", moveDirection.magnitude > 0.1f && !isAttackingEnemy);
    }

    // ══════════════════════════════════════════════════════════
    // ESTADO ESPECIAL
    // ══════════════════════════════════════════════════════════
    void ProcesarEstadoEspecial()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.Sleep;
            Debug.Log("[Estado] Seleccionado: SLEEP");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.Confused;
            Debug.Log("[Estado] Seleccionado: CONFUSED");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            estadoSeleccionado = Damageable.EstadoEspecial.None;
            Debug.Log("[Estado] Seleccionado: NONE");
        }
    }

    // ══════════════════════════════════════════════════════════
    // LOCK-ON API
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Busca al enemigo más cercano que esté vivo y lo lockea.
    /// No requiere que ya exista un lock activo.
    /// </summary>
    void ForceLockOnBest()
    {
        if (enemyManager == null || enemyManager.aliveEnemyCount == 0) return;

        EnemyScript closest = null;
        float closestDist = Mathf.Infinity;

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript e = enemyManager.allEnemies[i].enemyScript;
            if (e == null || !e.isActiveAndEnabled || !e.IsAttackable()) continue;

            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < closestDist && d <= detectionRadius)
            {
                closestDist = d;
                closest = e;
            }
        }

        if (closest != null)
        {
            DebugLock($"[ForceLock] Lockeando a '{closest.name}' (dist={closestDist:F1}m)");
            LockOnSpecific(closest.transform);
            manualUnlock = false;
        }
    }

    /// <summary>
    /// Lockea a un enemigo específico (por su Transform).
    /// </summary>
    void LockOnSpecific(Transform target)
    {
        if (target == null) return;

        lockedEnemy = target;
        isLockedOn = true;

        EnemyScript es = target.GetComponent<EnemyScript>();
        if (es != null && es.IsAttackable())
            lockedTarget = es;
        else
            lockedTarget = null;

        // Rotar hacia él
        transform.DOKill();
        transform.DOLookAt(
            target.position + Vector3.up * 1.5f,
            rotationDuration,
            AxisConstraint.Y,
            Vector3.up
        ).SetEase(Ease.OutSine);

        // Crear/actualizar indicador visual
        CreateLockOnIndicator(target);

        DebugLock($"[Lock] Activado sobre '{target.name}'");
    }

    /// <summary>
    /// Desbloquea el lock actual. Silencioso = no loguea.
    /// </summary>
    void UnlockTargetSilent()
    {
        isLockedOn = false;
        lockedEnemy = null;
        lockedTarget = null;
        transform.DOKill();

        if (lockOnInstance != null)
        {
            Destroy(lockOnInstance);
            lockOnInstance = null;
            lockOnTransform = null;
        }
    }

    /// <summary>
    /// Desbloqueo con botón Tab (manual).
    /// Marca manualUnlock = true para que no auto-lockee.
    /// </summary>
    void UnlockTarget()
    {
        DebugLock("[Lock] Desbloqueo manual (Tab).");
        UnlockTargetSilent();
        manualUnlock = true;
    }

    /// <summary>
    /// Cambia el lock al siguiente/enemigo en orden angular.
    /// </summary>
    void CycleLockTarget(int direction)
    {
        if (lockedEnemy == null || enemyManager == null) return;

        EnemyScript best = null;
        float bestDiff = Mathf.Infinity;
        float currentAngle = AngleToTarget(lockedEnemy);

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript e = enemyManager.allEnemies[i].enemyScript;
            if (e == null || !e.isActiveAndEnabled || !e.IsAttackable()) continue;
            if (e.transform == lockedEnemy) continue;

            float angle = AngleToTarget(e.transform);
            float diff = Mathf.DeltaAngle(currentAngle, angle);

            if (direction > 0 && diff > 0 && diff < bestDiff)
            {
                bestDiff = diff;
                best = e;
            }
            else if (direction < 0 && diff < 0 && -diff < bestDiff)
            {
                bestDiff = -diff;
                best = e;
            }
        }

        if (best != null)
        {
            DebugLock($"[Cycle] Cambiando a '{best.name}' (dir={direction})");
            LockOnSpecific(best.transform);
        }
    }

    /// <summary>
    /// Devuelve true si el Transform corresponde a un enemigo
    /// que existe, está activo en la escena, y tiene vida > 0.
    /// </summary>
    bool EstaEnemyVivoYActivo(Transform t)
    {
        if (t == null) return false;

        EnemyScript es = t.GetComponent<EnemyScript>();
        if (es == null) return false;
        if (!es.isActiveAndEnabled) return false;
        if (!es.IsAttackable()) return false;

        return true;
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — RESOLVE TARGET
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Decide cuál es el mejor enemigo para atacar ahora.
    /// Prioridad:
    ///   1. Lock-on actual (si está vivo)
    ///   2. Enemigo detectado por input direction
    ///   3. Enemigo más cercano (fallback)
    /// </summary>
    void ResolveTarget()
    {
        // ── 1) Lock-on activo sigue vivo ─────────────────────
        if (isLockedOn && EstaEnemyVivoYActivo(lockedEnemy))
        {
            lockedTarget = lockedEnemy.GetComponent<EnemyScript>();
            if (lockedTarget != null && lockedTarget.IsAttackable())
                return;

            // El referente del transform ya no es válido
            UnlockTargetSilent();
        }

        // ── 2) Detección por dirección de input ──────────────
        if (enemyDetection != null && enemyDetection.InputMagnitude() > 0.2f)
        {
            EnemyScript detected = enemyDetection.CurrentTarget();
            if (detected != null && detected.IsAttackable())
            {
                lockedTarget = detected;
                return;
            }
        }

        // ── 3) Fallback: más cercano ─────────────────────────
        if (enemyManager != null && enemyManager.aliveEnemyCount > 0)
        {
            lockedTarget = GetClosestEnemy();
            return;
        }

        // ── Sin enemigos ─────────────────────────────────────
        lockedTarget = null;
    }

    /// <summary>
    /// Devuelve el enemigo más cercano al jugador que esté vivo.
    /// </summary>
    EnemyScript GetClosestEnemy()
    {
        if (enemyManager == null) return null;

        EnemyScript closest = null;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript e = enemyManager.allEnemies[i].enemyScript;
            if (e == null || !e.isActiveAndEnabled || !e.IsAttackable()) continue;

            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = e;
            }
        }

        return closest;
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — ATTACK CHECKS
    // ══════════════════════════════════════════════════════════

       void AttackCheck()
    {
        if (isAttackingEnemy) return;

        ResolveTarget();
        if (lockedTarget == null)
        {
            AttackType("TrRaSwing", 0.2f, null, 0);
            return;
        }

        float distance = TargetDistance(lockedTarget);
        Attack(lockedTarget, distance);
    }

    void AttackCheckKick()
    {
        if (isAttackingEnemy) return;

        ResolveTarget();
        if (lockedTarget == null)
        {
            AttackType("TrCrescent", 0.2f, null, 0);
            return;
        }

        float distance = TargetDistance(lockedTarget);
        string[] kickAttacks = { "TrCrescent", "TrChut" };

        if (distance <= kickRange)
        {
            string atk = kickAttacks[UnityEngine.Random.Range(0, kickAttacks.Length)];
            AttackType(atk, attackCooldown, lockedTarget, 0.65f);
        }
        else
        {
            // Fuera de rango de kick → atacar de todas formas con lo que haya
            Attack(lockedTarget, distance);
        }
    }

    void AttackCheckRapier()
    {
        if (isAttackingEnemy) return;

        ResolveTarget();
        if (lockedTarget == null)
        {
            AttackType("TrRaSwing", 0.2f, null, 0);
            return;
        }

        float distance = TargetDistance(lockedTarget);
        string[] rapierAttacks = { "TrRaSwing", "TrRa360" };

        if (distance <= rapierRange)
        {
            string atk = rapierAttacks[UnityEngine.Random.Range(0, rapierAttacks.Length)];
            AttackType(atk, attackCooldown, lockedTarget, 0.65f);
        }
        else
        {
            Attack(lockedTarget, distance);
        }
    }

    void AttackCheckSW()
    {
        if (isAttackingEnemy) return;

        ResolveTarget();
        if (lockedTarget == null)
        {
            AttackType("TrSwSwing", 0.2f, null, 0);
            return;
        }

        float distance = TargetDistance(lockedTarget);
        string[] saberAttacks = { "TrSwSwing", "TrSw360" };

        if (distance <= lightSaberRange)
        {
            string atk = saberAttacks[UnityEngine.Random.Range(0, saberAttacks.Length)];
            AttackType(atk, attackCooldown, lockedTarget, 0.65f);
        }
        else
        {
            Attack(lockedTarget, distance);
        }
    }

    void AttackCheckWithSpecified(string attackTrigger)
    {
        if (isAttackingEnemy) return;

        // Misma lógica de selección que el resto
        if (isLockedOn && EstaEnemyVivoYActivo(lockedEnemy))
        {
            lockedTarget = lockedEnemy.GetComponent<EnemyScript>();
            if (lockedTarget != null && lockedTarget.IsAttackable())
            {
                AttackType(attackTrigger, 1.3f, lockedTarget, 0.65f);
                return;
            }
        }

        if (enemyDetection != null && enemyDetection.InputMagnitude() > 0.2f)
        {
            EnemyScript detected = enemyDetection.CurrentTarget();
            if (detected != null && detected.IsAttackable())
            {
                lockedTarget = detected;
                AttackType(attackTrigger, 1.3f, lockedTarget, 0.65f);
                return;
            }
        }

        if (enemyManager != null && enemyManager.aliveEnemyCount > 0)
        {
            lockedTarget = GetClosestEnemy();
            if (lockedTarget != null)
            {
                AttackType(attackTrigger, 1.3f, lockedTarget, 0.65f);
                return;
            }
        }

        // Sin enemigos: ejecutar animación sin daño
        AttackType(attackTrigger, 1.3f, null, 0);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — ATTACK (context-sensitive random by range)
    // ══════════════════════════════════════════════════════════

    public void Attack(EnemyScript target, float distance)
    {
        if (target == null)
        {
            AttackType("TrRaSwing", 0.2f, null, 0);
            return;
        }

        string[] kickAttacks       = { "TrCrescent", "TrChut" };
        string[] rapierAttacks     = { "TrRaSwing", "TrRa360" };
        string[] lightSaberAttacks = { "TrSwSwing", "TrSw360" };

        string[] pool;

        if (distance <= kickRange)
        {
            // En rango de kick → todos los tipos disponibles
            pool = new string[]
            {
                kickAttacks[0], kickAttacks[1],
                rapierAttacks[0], rapierAttacks[1],
                lightSaberAttacks[0], lightSaberAttacks[1]
            };
        }
        else if (distance <= rapierRange)
        {
            // En rango de rapier → rapier + lightsaber
            pool = new string[]
            {
                rapierAttacks[0], rapierAttacks[1],
                lightSaberAttacks[0], lightSaberAttacks[1]
            };
        }
        else if (distance <= lightSaberRange)
        {
            // Solo lightsaber
            pool = new string[]
            {
                lightSaberAttacks[0], lightSaberAttacks[1]
            };
        }
        else
        {
            // Fuera de rango → animación vacía
            AttackType("TrRaSwing", 0.2f, null, 0);
            return;
        }

        int idx = UnityEngine.Random.Range(0, pool.Length);
        AttackType(pool[idx], attackCooldown, target, 0.65f);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — ATTACK TYPE (ejecuta la animación + corrutina)
    // ══════════════════════════════════════════════════════════

    void AttackType(string attackTrigger, float cooldown, EnemyScript target, float movementDuration)
    {
        animator.SetTrigger(attackTrigger);

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine(
            IsLastHit() ? 1.5f : cooldown,
            target,
            movementDuration
        ));

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

        // Recuperar velocidad gradualmente
        speed = 0f;
        yield return new WaitForSeconds(0.2f);
        DOVirtual.Float(0f, 2f, 0.6f, v => speed = v);
    }

    IEnumerator FinalBlowCoroutine()
    {
        Time.timeScale = 0.5f;
        if (lastHitCamera != null) lastHitCamera.SetActive(true);
        if (lastHitFocusObject != null)
            lastHitFocusObject.position = lockedTarget != null
                ? lockedTarget.transform.position
                : transform.position;

        yield return new WaitForSecondsRealtime(2f);

        if (lastHitCamera != null) lastHitCamera.SetActive(false);
        Time.timeScale = 1f;
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — COUNTER ATTACK SYSTEM
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Ejecuta un contraataque. El tipo de ataque se decide
    /// según el parámetro counterType:
    ///   "KICK"       → random entre TrCrescent / TrChut
    ///   "LIGHTSABER" → random entre TrSwSwing / TrSw360
    ///   "RAPIER"     → random entre TrRaSwing / TrRa360
    ///   "DEFAULT"    → random entre TODOS los ataques
    ///
    /// Si el enemigo no está en rango para el arma elegida,
    /// busca un ataque válido por distancia como fallback.
    /// </summary>
    void EjecutarCounter(string counterType)
    {
        if (isCountering || isAttackingEnemy) return;

        // ── Buscar enemigo más cercano preparando ataque ─────
        EnemyScript closest = ClosestCounterEnemy();
        if (closest == null)
        {
            DebugCounter("[Counter] No hay enemigos preparando ataque. Cancelado.");
            return;
        }

        lockedTarget = closest;

        // ── Activar lock-on hacia el enemigo del contraataque ─
        Transform targetTransform = closest.transform;
        if (targetTransform != null)
        {
            lockedEnemy = targetTransform;
            isLockedOn = true;

            transform.DOKill();
            transform.DOLookAt(
                lockedEnemy.position + Vector3.up * 1.5f,
                rotationDuration,
                AxisConstraint.Y,
                Vector3.up
            ).SetEase(Ease.OutSine);

            if (lockOnInstance == null)
                CreateLockOnIndicator(lockedEnemy);
        }

        OnCounterAttack?.Invoke(lockedTarget);

        // ── Si está demasiado lejos, atacar avanzando ───────
        float dist = TargetDistance(lockedTarget);
        if (dist > 2f)
        {
            DebugCounter($"[Counter] Enemigo lejos ({dist:F1}m). Avanzando...");
            Attack(lockedTarget, dist);
            return;
        }

        // ── Cerca: ejecutar dodge + contraataque ─────────────
        float dodgeDuration = 0.6f;
        animator.SetTrigger("Dodge");

        // Orientar visualmente al enemigo
        if (targetTransform != null)
        {
            Vector3 dirToEnemy = (targetTransform.position - transform.position).normalized;
            dirToEnemy.y = 0;
            if (dirToEnemy != Vector3.zero)
                transform.forward = dirToEnemy;
        }

        // Mover hacia atrás del enemigo (dodge)
        Vector3 dodgeDir = targetTransform != null ? targetTransform.forward : transform.forward;
        transform.DOMove(transform.position + dodgeDir, dodgeDuration);

        if (counterCoroutine != null) StopCoroutine(counterCoroutine);
        counterCoroutine = StartCoroutine(CounterCoroutine(closest, dodgeDuration, counterType));
    }

    IEnumerator CounterCoroutine(EnemyScript originalTarget, float dodgeDur, string counterType)
    {
        isCountering = true;

        // ── Esperar a que termine la animación de dodge ──────
        yield return new WaitForSeconds(dodgeDur);

        // ── Verificar que el target sigue vivo ──────────────
        EnemyScript currentTarget = originalTarget;
        if (currentTarget == null || !currentTarget.IsAttackable())
        {
            DebugCounter("[Counter] Target original murió durante dodge. Buscando otro...");
            currentTarget = ClosestCounterEnemy();

            if (currentTarget == null)
            {
                // No hay nadie para contraatacar → buscar cualquier enemigo
                currentTarget = GetClosestEnemy();
                if (currentTarget == null)
                {
                    DebugCounter("[Counter] No hay ningún enemigo. Cancelado.");
                    isCountering = false;
                    yield break;
                }
            }

            lockedTarget = currentTarget;
        }

        float dist = TargetDistance(currentTarget);

        // ══════════════════════════════════════════════════════
        // SELECCIÓN DE ATAQUE SEGÚN TIPO DE COUNTER
        // ══════════════════════════════════════════════════════
        string attackTrigger = SeleccionarCounterAttack(counterType, dist);

        if (attackTrigger != null)
        {
            DebugCounter($"[Counter] Tipo={counterType} → Ataque: {attackTrigger} (dist={dist:F1}m)");
            AttackType(attackTrigger, attackCooldown, currentTarget, 0.65f);
        }
        else
        {
            DebugCounter($"[Counter] Fuera de rango para todo. Sin ataque.");
        }

        isCountering = false;

        if (lockedTarget != null)
            lockedTarget.ReleaseLock();
    }

    /// <summary>
    /// Elige un ataque aleatorio del pool correspondiente al tipo de counter.
    /// Si el enemigo no está en rango para ese pool, busca fallback por distancia.
    /// </summary>
    string SeleccionarCounterAttack(string counterType, float distance)
    {
        string[] kickPool       = { "TrCrescent", "TrChut" };
        string[] rapierPool     = { "TrRaSwing", "TrRa360" };
        string[] lightsaberPool = { "TrSwSwing", "TrSw360" };
        string[] allPool        = { "TrCrescent", "TrChut", "TrRaSwing", "TrRa360", "TrSwSwing", "TrSw360" };

        string[] pool = null;
        string poolName = "";

        switch (counterType)
        {
            case "KICK":
                pool = kickPool;
                poolName = "KICK";
                break;

            case "RAPIER":
                pool = rapierPool;
                poolName = "RAPIER";
                break;

            case "LIGHTSABER":
                pool = lightsaberPool;
                poolName = "LIGHTSABER";
                break;

            case "DEFAULT":
            default:
                pool = allPool;
                poolName = "ALL";
                break;
        }

        // ── Verificar si hay algún ataque del pool en rango ──
        bool anyInRange = false;
        foreach (string atk in pool)
        {
            if (AtaqueEnRango(atk, distance))
            {
                anyInRange = true;
                break;
            }
        }

        // ── Si ninguno está en rango, buscar fallback por distancia ──
        if (!anyInRange)
        {
            DebugCounter($"[Counter] Pool '{poolName}' fuera de rango. Buscando fallback...");
            pool = GetPoolPorDistancia(distance);

            if (pool == null || pool.Length == 0)
                return null; // Completamente fuera de rango
        }

        // ── Elegir aleatorio del pool ────────────────────────
        int idx = UnityEngine.Random.Range(0, pool.Length);
        return pool[idx];
    }

    /// <summary>
    /// Devuelve si un ataque específico puede alcanzar al enemigo a esa distancia.
    /// </summary>
    bool AtaqueEnRango(string attackTrigger, float distance)
    {
        if (attackTrigger == "TrCrescent" || attackTrigger == "TrChut")
            return distance <= kickRange;

        if (attackTrigger == "TrRaSwing" || attackTrigger == "TrRa360")
            return distance <= rapierRange;

        if (attackTrigger == "TrSwSwing" || attackTrigger == "TrSw360")
            return distance <= lightSaberRange;

        return false;
    }

    /// <summary>
    /// Devuelve el pool de ataques válido según la distancia.
    /// </summary>
    string[] GetPoolPorDistancia(float distance)
    {
        if (distance <= kickRange)
            return new string[] { "TrCrescent", "TrChut", "TrRaSwing", "TrRa360", "TrSwSwing", "TrSw360" };

        if (distance <= rapierRange)
            return new string[] { "TrRaSwing", "TrRa360", "TrSwSwing", "TrSw360" };

        if (distance <= lightSaberRange)
            return new string[] { "TrSwSwing", "TrSw360" };

        return null;
    }

    /// <summary>
    /// Encuentra el enemigo MÁS CERCANO que esté preparando un ataque.
    /// </summary>
    EnemyScript ClosestCounterEnemy()
    {
        if (enemyManager == null) return null;

        float minDistance = Mathf.Infinity;
        EnemyScript best = null;

        for (int i = 0; i < enemyManager.allEnemies.Count; i++)
        {
            EnemyScript enemy = enemyManager.allEnemies[i].enemyScript;

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

    // ══════════════════════════════════════════════════════════
    // COMBAT — HIT EVENT (Animation Event)
    // ══════════════════════════════════════════════════════════

    public void HitEvent()
    {
        if (lockedTarget == null || enemyManager == null || enemyManager.aliveEnemyCount == 0)
            return;

        OnHit?.Invoke(lockedTarget);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — RECIBIR DAÑO
    // ══════════════════════════════════════════════════════════

    public void RecibirDanyo()
    {
        animator.SetTrigger("NinjaHit");
        DamageEvent?.Invoke();

        if (damageCoroutine != null) StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(DamageCoroutine());
    }

    IEnumerator DamageCoroutine()
    {
        speed = 0f;
        yield return new WaitForSeconds(0.5f);
        speed = 2f;
        DOVirtual.Float(0f, 2f, 0.6f, v => speed = v);
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — HELPERS
    // ══════════════════════════════════════════════════════════

    float TargetDistance(EnemyScript target)
    {
        if (target == null) return float.MaxValue;
        return Vector3.Distance(transform.position, target.transform.position);
    }

    public Vector3 TargetOffset(Transform target)
    {
        return Vector3.MoveTowards(target.position, transform.position, 0.95f);
    }

    void MoveTowardsTarget(EnemyScript target, float duration)
    {
        if (target == null) return;

        OnTrajectory?.Invoke(target);
        transform.DOLookAt(target.transform.position + Vector3.up * 1.5f, 0.2f, AxisConstraint.Y, Vector3.up);
        transform.DOMove(TargetOffset(target.transform), duration);
    }

    bool IsLastHit()
    {
        if (lockedTarget == null) return false;
        return enemyManager != null
            && enemyManager.aliveEnemyCount == 1
            && lockedTarget.currentHealth <= 1;
    }

    float AngleToTarget(Transform target)
    {
        if (target == null) return 0f;
        Vector3 dir = (target.position - transform.position).normalized;
        return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
    }

    // ══════════════════════════════════════════════════════════
    // LOCK-ON INDICATOR UI
    // ══════════════════════════════════════════════════════════

    void CreateLockOnIndicator(Transform target)
    {
        if (lockOnPrefab == null)
        {
            Debug.LogWarning("[Lock-On] lockOnPrefab no asignado en Inspector.");
            return;
        }

        if (lockOnInstance != null)
            Destroy(lockOnInstance);

        Vector3 spawnPos = target.position + Vector3.up * lockOnHeightOffset;
        lockOnInstance = Instantiate(lockOnPrefab, spawnPos, Quaternion.identity);
        lockOnTransform = lockOnInstance.transform;

        if (lockOnCamera == null)
            lockOnCamera = Camera.main;

        RangeIndicator ri = lockOnInstance.GetComponentInChildren<RangeIndicator>();
        if (ri != null)
            ri.Initialize(target, transform);
    }

    void UpdateLockOnIndicator()
    {
        if (!isLockedOn || lockOnInstance == null || lockedEnemy == null) return;

        Vector3 targetPos = lockedEnemy.position + Vector3.up * lockOnHeightOffset;
        lockOnTransform.position = Vector3.SmoothDamp(
            lockOnTransform.position,
            targetPos,
            ref lockOnVelocity,
            lockOnSmoothTime
        );

        if (lockOnCamera != null)
            lockOnTransform.forward = lockOnCamera.transform.forward;
    }

    // ══════════════════════════════════════════════════════════
    // API PÚBLICA
    // ══════════════════════════════════════════════════════════

    public Damageable.EstadoEspecial GetEstadoSeleccionado()
    {
        return estadoSeleccionado;
    }

    // ══════════════════════════════════════════════════════════
    // HITBOX ENABLE/DISABLE (Animation Events)
    // ══════════════════════════════════════════════════════════

    public void EnableRapierHitbox()
    {
        if (RapierCollider != null) RapierCollider.enabled = true;
        if (Rapier != null) Rapier.Reactivar();
    }

    public void EnableLightSaberHitbox()
    {
        if (LightSaberCollider != null) LightSaberCollider.enabled = true;
        if (LightSaber != null) LightSaber.Reactivar();
    }

    public void EnableLeftKickHitbox()
    {
        if (LeftKickCollider != null) LeftKickCollider.enabled = true;
        if (LeftKick != null) LeftKick.Reactivar();
    }

    public void EnableRightKickHitbox()
    {
        if (RightKickCollider != null) RightKickCollider.enabled = true;
        if (RightKick != null) RightKick.Reactivar();
    }

    public void DisableRapierHitbox()
    {
        if (RapierCollider != null) RapierCollider.enabled = false;
    }

    public void DisableLightSaberHitbox()
    {
        if (LightSaberCollider != null) LightSaberCollider.enabled = false;
    }

    public void DisableLeftKickHitbox()
    {
        if (LeftKickCollider != null) LeftKickCollider.enabled = false;
    }

    public void DisableRightKickHitbox()
    {
        if (RightKickCollider != null) RightKickCollider.enabled = false;
    }

    // ══════════════════════════════════════════════════════════
    // CORRUTINAS ORIGINALES (compatibilidad)
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

    IEnumerator Ra360()  { isAttackingEnemy = true; animator.SetTrigger("TrRa360");  yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Sw360()  { isAttackingEnemy = true; animator.SetTrigger("TrSw360");  yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator SwSwing(){ isAttackingEnemy = true; animator.SetTrigger("TrSwSwing"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator RaSwing(){ isAttackingEnemy = true; animator.SetTrigger("TrRaSwing"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Crescent(){ isAttackingEnemy = true; animator.SetTrigger("TrCrescent"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Chut()   { isAttackingEnemy = true; animator.SetTrigger("TrChut");    yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }

    // ══════════════════════════════════════════════════════════
    // DEBUG HELPERS
    // ══════════════════════════════════════════════════════════

    void DebugLock(string msg)    { if (LOG_LOCK)    Debug.Log(msg); }
    void DebugCombat(string msg)  { if (LOG_COMBAT)  Debug.Log(msg); }
    void DebugCounter(string msg) { if (LOG_COUNTER) Debug.Log(msg); }
}