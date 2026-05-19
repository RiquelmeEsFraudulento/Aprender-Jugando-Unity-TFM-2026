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
    public WeaponHitbox RightKick;
    public WeaponHitbox LeftKick;

    // ══════════════════════════════════════════════════════════
    // COMBAT — Settings (equivalente a CombatScript)
    // ══════════════════════════════════════════════════════════
    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 0.6f;

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

    // ══════════════════════════════════════════════════════════
    // START
    // ══════════════════════════════════════════════════════════
    void Start()
    {
        controller    = GetComponent<CharacterController>();
        //impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
        enemyManager  = FindObjectOfType<EnemyManager>();

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
        moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // No permite mover mientras ataca (igual que AttackCoroutine desactivaba MovementInput)
        if (moveDirection.magnitude >= 0.1f && !isAttackingEnemy)
        {
            transform.forward = moveDirection;
            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        animator.SetBool("isWalking", moveDirection.magnitude > 0f && !isAttackingEnemy);

        // ── Tus ataques originales ────────────────────────────
        if (Input.GetKeyDown(KeyCode.B))      StartCoroutine(DanceSpin());
        if (Input.GetKeyDown(KeyCode.J))      StartCoroutine(JumpCountdown());
        if (Input.GetKeyDown(KeyCode.M))      StartCoroutine(Martelo());
        if (Input.GetKeyDown(KeyCode.K))      StartCoroutine(Chut());
        if (Input.GetKeyDown(KeyCode.Mouse0)) AttackCheck();   // ← ahora lanza AttackCheck
        if (Input.GetKeyDown(KeyCode.Mouse1)) StartCoroutine(SwSwing());
        if (Input.GetKeyDown(KeyCode.Q))      StartCoroutine(Ra360());
        if (Input.GetKeyDown(KeyCode.E))      StartCoroutine(Sw360());
        if (Input.GetKeyDown(KeyCode.Space))  CounterCheck();  // ← ahora lanza CounterCheck
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — AttackCheck (de CombatScript, adaptado)
    // ══════════════════════════════════════════════════════════
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

        Attack(lockedTarget, TargetDistance(lockedTarget));
    }

    // ══════════════════════════════════════════════════════════
    // COMBAT — Attack
    // ══════════════════════════════════════════════════════════
    public void Attack(EnemyScript target, float distance)
    {
        attacks = new string[] { "TrRaSwing", "TrSwSwing", "TrMartelo", "TrChut" };

        if (target == null)
        {
            AttackType("TrRaSwing", .2f, null, 0);
            return;
        }

        if (distance < 15)
        {
            animationCount = (int)Mathf.Repeat((float)animationCount + 1, (float)attacks.Length);
            string attackString = IsLastHit()
                ? attacks[UnityEngine.Random.Range(0, attacks.Length)]
                : attacks[animationCount];
            AttackType(attackString, attackCooldown, target, .65f);
        }
        else
        {
            lockedTarget = null;
            AttackType("TrRaSwing", .2f, null, 0);
        }

        //if (impulseSource != null)
            //impulseSource.m_ImpulseDefinition.m_AmplitudeGain = Mathf.Max(3, 1 * distance);
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
    void CounterCheck()
    {
        if (isCountering || isAttackingEnemy || !enemyManager.AnEnemyIsPreparingAttack())
            return;

        lockedTarget = ClosestCounterEnemy();
        OnCounterAttack?.Invoke(lockedTarget);

        if (TargetDistance(lockedTarget) > 2)
        {
            Attack(lockedTarget, TargetDistance(lockedTarget));
            return;
        }

        float duration = .2f;
        animator.SetTrigger("Dodge");
        transform.DOLookAt(lockedTarget.transform.position, .2f);
        transform.DOMove(transform.position + lockedTarget.transform.forward, duration);

        if (counterCoroutine != null) StopCoroutine(counterCoroutine);
        counterCoroutine = StartCoroutine(CounterCoroutine(duration));

        IEnumerator CounterCoroutine(float dur)
        {
            isCountering = true;
            yield return new WaitForSeconds(dur);
            Attack(lockedTarget, TargetDistance(lockedTarget));
            isCountering = false;
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
        animator.SetTrigger("Hit");
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
        => Vector3.Distance(transform.position, target.transform.position);

    public Vector3 TargetOffset(Transform target)
        => Vector3.MoveTowards(target.position, transform.position, .95f);

    EnemyScript ClosestCounterEnemy()
    {
        float minDistance = 100f;
        int finalIndex    = 0;

        for (int i = 0; i < enemyManager.allEnemies.Length; i++)
        {
            EnemyScript enemy = enemyManager.allEnemies[i].enemyScript;
            if (enemy.IsPreparingAttack())
            {
                float d = Vector3.Distance(transform.position, enemy.transform.position);
                if (d < minDistance) { minDistance = d; finalIndex = i; }
            }
        }

        return enemyManager.allEnemies[finalIndex].enemyScript;
    }

    bool IsLastHit()
    {
        if (lockedTarget == null) return false;
        return enemyManager.AliveEnemyCount() == 1 && lockedTarget.health <= 1;
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

    IEnumerator Ra360()    { isAttackingEnemy = true; animator.SetTrigger("TrRa360");   yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Sw360()    { isAttackingEnemy = true; animator.SetTrigger("TrSw360");   yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator SwSwing()  { isAttackingEnemy = true; animator.SetTrigger("TrSwSwing"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Martelo()  { isAttackingEnemy = true; animator.SetTrigger("TrMartelo"); yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }
    IEnumerator Chut()     { isAttackingEnemy = true; animator.SetTrigger("TrChut");    yield return new WaitForSeconds(1.3f); isAttackingEnemy = false; }

    // ══════════════════════════════════════════════════════════
    // HITBOXES — llamadas desde Animation Events
    // ══════════════════════════════════════════════════════════
    public void EnableRapierHitbox()     { RapierCollider.enabled = true;      Rapier.Reactivar(); }
    public void EnableLightSaberHitbox() { LightSaberCollider.enabled = true;  Rapier.Reactivar(); }
    public void EnableLeftKickHitbox()   { LeftKickCollider.enabled = true;    Rapier.Reactivar(); }
    public void EnableRightKickHitbox()  { RightKickCollider.enabled = true;   Rapier.Reactivar(); }
    public void DisableRapierHitbox()     { RapierCollider.enabled = false; }
    public void DisableLightSaberHitbox() { LightSaberCollider.enabled = false; }
    public void DisableLeftKickHitbox()   { LeftKickCollider.enabled = false; }
    public void DisableRightKickHitbox()  { RightKickCollider.enabled = false; }
}