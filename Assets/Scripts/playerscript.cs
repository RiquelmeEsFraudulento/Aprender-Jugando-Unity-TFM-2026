using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(CharacterController))]
public class SimpleWalk : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Original movement & references
    // ─────────────────────────────────────────────
    public float speed = 2f;
    public Animator animator;

    public GameObject ballPrefab;
    private GameObject currentBall;
    public Transform ballSpawnPoint;

    public Collider RapierCollider;
    public Collider LightSaberCollider;
    public Collider RightKickCollider;
    public Collider LeftKickCollider;
    public RapierHitbox Rapier;
    public WeaponHitbox LightSaber;
    public KickHitbox RightKick;
    public KickHitbox LeftKick;

    private CharacterController controller;
    private Vector3 moveDirection;

    // ─────────────────────────────────────────────
    //  Lock-On System
    // ─────────────────────────────────────────────
    [Header("Lock-On System")]

    [Tooltip("Radius in which enemies can be detected for lock-on.")]
    public float detectionRadius = 8f;

    [Tooltip("Minimum angle difference (degrees) before re-triggering DOTween rotation.")]
    public float rotationThreshold = 2f;

    [Tooltip("DOTween rotation duration in seconds.")]
    public float rotationDuration = 0.15f;

    /// <summary>The enemy currently locked onto. Null when unlocked.</summary>
    private Transform lockedEnemy;

    /// <summary>Whether the lock-on system is currently active.</summary>
    private bool isLockedOn = false;

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────
    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        // ── Lock-On Toggle (Tab key) ──────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isLockedOn)
                UnlockTarget();
            else
                TryLockOn();
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

        // ── Read raw input axes ───────────────────────────────────────────────
        float horizontal = Input.GetAxisRaw("Horizontal"); // A / D
        float vertical   = Input.GetAxisRaw("Vertical");   // W / S

        // ── Movement & Rotation ───────────────────────────────────────────────
        if (isLockedOn && lockedEnemy != null)
        {
            // --- Lock-On Movement: relative to the locked enemy ---------------
            // W  → toward enemy  |  S  → away from enemy  |  A/D → strafe
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

            if (moveDirection.magnitude >= 0.1f)
            {
                // Rotate character toward movement direction (instant, original)
                transform.forward = moveDirection;

                // Move
                controller.Move(moveDirection * speed * Time.deltaTime);
            }
        }

        // ── Animator parameter ────────────────────────────────────────────────
        bool isWalking = moveDirection.magnitude > 0f;
        animator.SetBool("isWalking", isWalking);

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
    }

    // ─────────────────────────────────────────────
    //  Lock-On Helpers
    // ─────────────────────────────────────────────

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
        Rapier.Reactivar();
    }

    public void EnableRightKickHitbox()
    {
        RightKickCollider.enabled = true;
        Rapier.Reactivar();
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
        controller.Move(Vector3.up * 2f);
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
}