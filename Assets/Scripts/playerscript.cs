using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Cinemachine;

public class SimpleWalk : MonoBehaviour
{
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private MovementInput movementInput;
    private Animator animator;
    private CinemachineImpulseSource impulseSource;
    private CharacterController characterController;   // Nuevo: para movimiento en lock-on

    [Header("Target")]
    private EnemyScript lockedTarget;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown;

    [Header("States")]
    public bool isAttackingEnemy = false;
    public bool isCountering = false;

    [Header("Public References")]
    [SerializeField] private Transform punchPosition;
    //[SerializeField] private ParticleSystemScript punchParticle;
    [SerializeField] private GameObject lastHitCamera;
    [SerializeField] private Transform lastHitFocusObject;

    //Coroutines
    private Coroutine counterCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine damageCoroutine;

    [Space]
    //Events
    public UnityEvent<EnemyScript> OnTrajectory;
    public UnityEvent<EnemyScript> OnHit;
    public UnityEvent<EnemyScript> OnCounterAttack;

    int animationCount = 0;
    string[] attacks;

    // ─── Nuevo: Lock-On System ─────────────────────────────────
    [Header("Lock-On System")]
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float rotationThreshold = 2f;
    [SerializeField] private float rotationDuration = 0.15f;

    private Transform lockedEnemy;          // Enemigo fijado (solo Transform)
    private bool isLockedOn = false;
    
    public Collider RapierCollider;
    public Collider LightSaberCollider;
    public Collider RightKickCollider;
    public Collider LeftKickCollider;
    public RapierHitbox Rapier;
    public WeaponHitbox LightSaber;
    public KickHitbox RightKick;
    public KickHitbox LeftKick;



    void Start()
    {
        enemyManager = FindAnyObjectByType<EnemyManager>();
        animator = GetComponent<Animator>();
        enemyDetection = GetComponentInChildren<EnemyDetection>();
        movementInput = GetComponent<MovementInput>();
        impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
        characterController = GetComponent<CharacterController>();  // Nuevo
    }

    void Update()
    {
        // ── Lock-On: rotación y movimiento relativos al enemigo fijado ──
        if (!isLockedOn || lockedEnemy == null)
            return;

        // Auto‑desfijar si el enemigo muere o se aleja demasiado
        if (lockedEnemy == null ||
            Vector3.Distance(transform.position, lockedEnemy.position) > detectionRadius)
        {
            UnlockTarget();
            return;
        }

        // Movimiento relativo al enemigo fijado
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");

        Vector3 toEnemy = (lockedEnemy.position - transform.position).normalized;
        Vector3 right   = Vector3.Cross(Vector3.up, toEnemy);
        Vector3 moveDir = (toEnemy * vertical + right * horizontal).normalized;

        if (moveDir.magnitude >= 0.1f)
        {
            characterController.Move(moveDir * movementInput.movementSpeed * Time.deltaTime);
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        // Rotación suave hacia el enemigo (solo en Y)
        Vector3 lookDir = new Vector3(toEnemy.x, 0f, toEnemy.z);
        if (lookDir != Vector3.zero)
        {
            float angleDiff = Vector3.Angle(transform.forward, lookDir);
            if (angleDiff > rotationThreshold)
            {
                transform.DOKill();   // Mata cualquier tween anterior
                transform
                    .DOLookAt(lockedEnemy.position, rotationDuration,
                              AxisConstraint.Y, Vector3.up)
                    .SetEase(Ease.OutSine);
            }
        }
        /*

          // ── Action keys (unchanged) ───────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.B))
            StartCoroutine(DanceSpin());

        if (Input.GetKeyDown(KeyCode.J))
            StartCoroutine(JumpCountdown());

        if (Input.GetKeyDown(KeyCode.M))
            StartCoroutine(Martelo());

        if (Input.GetKeyDown(KeyCode.K))
            StartCoroutine(Chut());

        if (Input.GetKeyDown(KeyCode.Mouse0))
            StartCoroutine(RaSwing());

        if (Input.GetKeyDown(KeyCode.Mouse1))
            StartCoroutine(SwSwing());

        if (Input.GetKeyDown(KeyCode.Q))
            StartCoroutine(Ra360());

        if (Input.GetKeyDown(KeyCode.E))
            StartCoroutine(Sw360());
        */
        Debug.DrawLine(transform.position, lockedEnemy.position, Color.cyan);
    }

    /// <summary>Alterna el Lock‑On (llamado por evento de input, p.ej. tecla Tab).</summary>
    //public void OnLockOn()
    //{
        //if (isLockedOn)
            //UnlockTarget();
        //else
            //TryLockOn();
    //}

    void TryLockOn()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        Transform bestCandidate = null;
        float bestDot = -Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy"))
                continue;

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

            // Desactiva MovementInput para que no interfiera con el movimiento relativo
            movementInput.enabled = false;

            // Primera rotación instantánea
            transform.DOKill();
            transform
                .DOLookAt(lockedEnemy.position, rotationDuration,
                          AxisConstraint.Y, Vector3.up)
                .SetEase(Ease.OutSine);
        }
    }

    void UnlockTarget()
    {
        isLockedOn = false;
        lockedEnemy = null;
        transform.DOKill();
        movementInput.enabled = true;
    }
    // ─── Fin del Lock-On System ───────────────────────────────

    //This function gets called whenever the player inputs the punch action
    void AttackCheck()
    {
        if (isAttackingEnemy)
            return;

        // ── Si hay Lock‑On, úsalo directamente ─────────────────
        if (isLockedOn && lockedEnemy != null)
        {
            EnemyScript lockedEnemyScript = lockedEnemy.GetComponent<EnemyScript>();
            if (lockedEnemyScript != null)
            {
                lockedTarget = lockedEnemyScript;
                Attack(lockedTarget, TargetDistance(lockedTarget));
                return;
            }
        }
        // ────────────────────────────────────────────────────────

        //Check to see if the detection behavior has an enemy set
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

        //If the player is moving the movement input, use the "directional" detection to determine the enemy
        if (enemyDetection.InputMagnitude() > .2f)
            lockedTarget = enemyDetection.CurrentTarget();

        //Extra check to see if the locked target was set
        if(lockedTarget == null)
            lockedTarget = enemyManager.RandomEnemy();

        //AttackTarget
        Attack(lockedTarget, TargetDistance(lockedTarget));
    }

    public void Attack(EnemyScript target, float distance)
    {
        //Types of attack animation
        attacks = new string[] { "TrSw360", "TrRa360", "TrRaSwing", "TrMartelo", "TrChut"};

        //Attack nothing in case target is null
        if (target == null)
        {
            AttackType("TrRaSwing", .2f, null, 0);
            return;
        }

        if (3 < distance && distance < 15)
        {
            animationCount = (int)Mathf.Repeat((float)animationCount + 1, (float)attacks.Length);
            string attackString = isLastHit() ? attacks[Random.Range(0, attacks.Length)] : attacks[animationCount];
            AttackType(attackString, attackCooldown, target, .65f);
        }
        else if (distance > 3)
        {
            AttackType("TrAirKick", attackCooldown, target, .65f);
        } else
        {
            lockedTarget = null;
            AttackType("TrRaSwing", .2f, null, 0);
        }

        //Change impulse
        if (impulseSource != null)
            impulseSource.GetComponent<CinemachineImpulseSource>().ImpulseDefinition.AmplitudeGain = Mathf.Max(3, 1 * distance);
    }

    void AttackType(string attackTrigger, float cooldown, EnemyScript target, float movementDuration)
    {
        animator.SetTrigger(attackTrigger);

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine(isLastHit() ? 1.5f : cooldown));

        //Check if last enemy
        if (isLastHit())
            StartCoroutine(FinalBlowCoroutine());

        if (target == null)
            return;

        target.StopMoving();
        MoveTorwardsTarget(target, movementDuration);

        IEnumerator AttackCoroutine(float duration)
        {
            movementInput.acceleration = 0;
            isAttackingEnemy = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            isAttackingEnemy = false;
            yield return new WaitForSeconds(.2f);
            movementInput.enabled = true;
            LerpCharacterAcceleration();
        }

        IEnumerator FinalBlowCoroutine()
        {
            Time.timeScale = .5f;
            lastHitCamera.SetActive(true);
            lastHitFocusObject.position = lockedTarget.transform.position;
            yield return new WaitForSecondsRealtime(2);
            lastHitCamera.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void MoveTorwardsTarget(EnemyScript target, float duration)
    {
        OnTrajectory.Invoke(target);
        transform.DOLookAt(target.transform.position, .2f);
        transform.DOMove(TargetOffset(target.transform), duration);
    }

    void CounterCheck()
    {
        //Initial check
        if (isCountering || isAttackingEnemy || !enemyManager.AnEnemyIsPreparingAttack())
            return;

        lockedTarget = ClosestCounterEnemy();
        OnCounterAttack.Invoke(lockedTarget);

        if (TargetDistance(lockedTarget) > 2)
        {
            Attack(lockedTarget, TargetDistance(lockedTarget));
            return;
        }

        float duration = .2f;
        animator.SetTrigger("Dodge");
        transform.DOLookAt(lockedTarget.transform.position, .2f);
        transform.DOMove(transform.position + lockedTarget.transform.forward, duration);

        if (counterCoroutine != null)
            StopCoroutine(counterCoroutine);
        counterCoroutine = StartCoroutine(CounterCoroutine(duration));

        IEnumerator CounterCoroutine(float duration)
        {
            isCountering = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            Attack(lockedTarget, TargetDistance(lockedTarget));
            isCountering = false;
        }
    }

    float TargetDistance(EnemyScript target)
    {
        if (target == null) 
            return float.MaxValue;  // or some large distance, or handle differently
        return Vector3.Distance(transform.position, target.transform.position);
    }

    public Vector3 TargetOffset(Transform target)
    {
        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, .95f);
    }

    public void HitEvent()
    {
        if (lockedTarget == null || enemyManager.AliveEnemyCount() == 0)
            return;

        OnHit.Invoke(lockedTarget);

        //Polish
        //punchParticle.PlayParticleAtPosition(punchPosition.position);
    }

    public void DamageEvent()
    {
        animator.SetTrigger("NinjaHit");

        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(DamageCoroutine());

        IEnumerator DamageCoroutine()
        {
            movementInput.enabled = false;
            yield return new WaitForSeconds(.5f);
            movementInput.enabled = true;
            LerpCharacterAcceleration();
        }
    }

    EnemyScript ClosestCounterEnemy()
    {
        float minDistance = 100;
        int finalIndex = 0;

        for (int i = 0; i < enemyManager.allEnemies.Length; i++)
        {
            EnemyScript enemy = enemyManager.allEnemies[i].enemyScript;

            if (enemy.IsPreparingAttack())
            {
                if (Vector3.Distance(transform.position, enemy.transform.position) < minDistance)
                {
                    minDistance = Vector3.Distance(transform.position, enemy.transform.position);
                    finalIndex = i;
                }
            }
        }

        return enemyManager.allEnemies[finalIndex].enemyScript;
    }

    void LerpCharacterAcceleration()
    {
        movementInput.acceleration = 0;
        DOVirtual.Float(0, 1, .6f, ((acceleration)=> movementInput.acceleration = acceleration));
    }

    bool isLastHit()
    {
        if (lockedTarget == null)
            return false;

        return enemyManager.AliveEnemyCount() == 1 && lockedTarget.health <= 1;
    }

    #region Input

    private void OnCounter()
    {
        CounterCheck();
    }

    private void OnAttack()
    {
        AttackCheck();
    }

    // Nuevo: se llama desde el sistema de input (configurar Tab)
    private void OnLockOn()
    {
        if (isLockedOn)
            UnlockTarget();
        else
            TryLockOn();
    }


     // ─────────────────────────────────────────────
    //  Original Coroutines & Methods (unchanged)
    // ─────────────────────────────────────────────

    IEnumerator DanceSpin()
    {
        for (int i = 0; i < 360; i += 30)
        {
            transform.Rotate(0, 30, 0);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void EnableRapierHitbox()
    {
        RapierCollider.enabled = true;
        Rapier.Reactivar();
    }

    public void EnableLightSaberHitbox()
    {
        LightSaberCollider.enabled = true;
        LightSaber.Reactivar();
    }

    public void EnableLeftKickHitbox()
    {
        LeftKickCollider.enabled = true;
        LeftKick.Reactivar();
    }

    public void EnableRightKickHitbox()
    {
        RightKickCollider.enabled = true;
        RightKick.Reactivar();
    }

    public void DisableRapierHitbox()
    {
        RapierCollider.enabled = false;
    }

    public void DisableLightSaberHitbox()
    {
        LightSaberCollider.enabled = false;
    }

    public void DisableLeftKickHitbox()
    {
        LeftKickCollider.enabled = false;
    }

    public void DisableRightKickHitbox()
    {
        RightKickCollider.enabled = false;
    }

    IEnumerator JumpCountdown()
    {
        for (int i = 3; i > 0; i--)
        {
            Debug.Log(i);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("¡Jump!");
        characterController.Move(Vector3.up * 2f);
    }

    IEnumerator RaSwing()
    {
        animator.SetTrigger("TrRaSwing");
        yield return new WaitForSeconds(1.3f);
    }

    IEnumerator SwSwing()
    {
        animator.SetTrigger("TrSwSwing");
        yield return new WaitForSeconds(1.3f);
    }

    IEnumerator Ra360()
    {
        animator.SetTrigger("TrRa360");
        yield return new WaitForSeconds(1.3f);
    }

    IEnumerator Sw360()
    {
        animator.SetTrigger("TrSw360");
        yield return new WaitForSeconds(1.3f);
    }

    IEnumerator Martelo()
    {
        animator.SetTrigger("TrMartelo");
        yield return new WaitForSeconds(1.3f);
    }

    IEnumerator Chut()
    {
        animator.SetTrigger("TrChut");
        yield return new WaitForSeconds(1.3f);
    }

    #endregion
}