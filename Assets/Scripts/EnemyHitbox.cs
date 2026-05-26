using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private EnemyScript _enemyAI;
    private PlayerHealth _pendingTarget;         // captured during hitbox window
    private bool _hitboxWindowOpen = false;
    private const bool LOG_HITBOX = true;

    void Awake()
    {
        _enemyAI = GetComponentInParent<EnemyScript>();
        if (_enemyAI == null)
            Debug.LogWarning($"[EnemyHitbox] '{name}' no encontró EnemyScript en el padre.");
    }

    /// <summary>
    /// Called via Animation Event at the START of the active frames.
    /// Opens the hitbox window — any player entering the trigger is captured.
    /// </summary>
    public void OpenHitboxWindow()
    {
        _hitboxWindowOpen = true;
        _pendingTarget = null;
        DebugHitbox($"[Hitbox] ► Window OPEN");
    }

    /// <summary>
    /// Called via Animation Event at the END of the active frames.
    /// Closes the hitbox window so no more targets are captured.
    /// </summary>
    public void CloseHitboxWindow()
    {
        _hitboxWindowOpen = false;
        DebugHitbox($"[Hitbox] ■ Window CLOSE | pending={(_pendingTarget ? _pendingTarget.name : "null")}");
    }

    /// <summary>
    /// Called via Animation Event at the exact impact frame.
    /// Applies damage only to the target captured during the window.
    /// </summary>
    public void HitEvent()
    {
        if (_pendingTarget == null)
        {
            DebugHitbox("[Hitbox] HitEvent — no target captured → miss");
            return;
        }

        if (_enemyAI != null)
        {
            _enemyAI.GolpearNinja(_pendingTarget);

            // Also call the visual/feedback path
            SimpleWalk playerCombat = FindAnyObjectByType<SimpleWalk>();
            if (playerCombat != null)
                playerCombat.RecibirDanyo();
        }

        DebugHitbox($"[Hitbox] HitEvent → hit '{_pendingTarget.name}'");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_hitboxWindowOpen) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null) return;

        // Capture the target (last one wins if somehow two overlap)
        _pendingTarget = ph;
        DebugHitbox($"[Hitbox] Target captured: '{ph.name}'");
    }

    void OnTriggerExit(Collider other)
    {
        // Optional: clear target so whiffed swings truly miss
        // Comment out if you want "lingering target" behavior
        if (_pendingTarget != null && other.gameObject == _pendingTarget.gameObject)
        {
            DebugHitbox($"[Hitbox] Target left hitbox area → cleared");
            _pendingTarget = null;
        }
    }

    void DebugHitbox(string msg) { if (LOG_HITBOX) Debug.Log(msg); }
}