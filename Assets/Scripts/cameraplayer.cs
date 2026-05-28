// ===== FILE: ./cameraplayer.cs =====

using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Ultimate auto-configuring Cinemachine 3.1.6 camera controller.
/// Place this on an EMPTY GameObject in your scene.
///
/// What it does on Awake:
///   1. Auto-finds the player (GameObject tagged "Player").
///   2. Creates a CinemachineCamera on this GameObject.
///   3. Adds CinemachineFollow for smooth position tracking.
///   4. Adds CinemachineBasicMultiChannelPerlin for shake (off by default).
///   5. Sets FOV, deprioritizes other VCams, snaps to initial position.
///
/// NOTE: Your player MUST have the tag "Player".
/// NOTE: Scene must have a MainCamera with CinemachineBrain.
/// </summary>
[DisallowMultipleComponent]
public class CameraManager : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════
    // INSPECTOR
    // ══════════════════════════════════════════════════════════

    [Header("Target")]
    [Tooltip("Leave null to auto-find GameObject tagged 'Player'.")]
    public Transform target;

    [Header("Follow Offset (relative to target pivot)")]
    public Vector3 followOffset = new Vector3(0f, 3.5f, -7f);

    [Header("Aim")]
    [Tooltip("How far above ground to look at (head/chest height).")]
    public float aimOffsetY = 1.5f;

    [Header("FOV")]
    public float defaultFOV = 55f;
    public float maxFOV = 75f;
    public float fovSpeedScale = 8f;

    [Header("Camera Shake (0 = perfectly still)")]
    public float shakeAmplitude = 0f;
    public float shakeFrequency = 0f;

    // ══════════════════════════════════════════════════════════
    // PRIVATE
    // ══════════════════════════════════════════════════════════

    private CinemachineCamera _vcam;
    private CinemachineFollow _follow;
    private CinemachineBasicMultiChannelPerlin _noise;
    private bool _initialized;

    // ══════════════════════════════════════════════════════════
    // LIFECYCLE
    // ══════════════════════════════════════════════════════════

    void Awake()
    {
        AutoConfigure();
    }

    void LateUpdate()
    {
        if (!_initialized || _vcam == null) return;
        ApplyDynamicFOV();
    }

    // ══════════════════════════════════════════════════════════
    // AUTO-CONFIGURE
    // ══════════════════════════════════════════════════════════

    void AutoConfigure()
    {
        // ── Step 1 — Find target ─────────────────────────────
        if (target == null)
        {
            GameObject playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                target = playerGO.transform;
                Debug.Log($"[CameraManager] Auto-found target: '{target.name}'.");
            }
            else
            {
                Debug.LogError("[CameraManager] No target assigned AND no 'Player' tagged GameObject found!");
                return;
            }
        }

        // ── Step 2 — Ensure CinemachineBrain ─────────────────
        EnsureCinemachineBrain();

        // ── Step 3 — Set up CinemachineCamera on this GO ─────
        _vcam = GetComponent<CinemachineCamera>();
        if (_vcam == null)
            _vcam = gameObject.AddComponent<CinemachineCamera>();

        _vcam.Priority = 10;
        _vcam.Follow = target;
        _vcam.LookAt = target;

        // ── Step 4 — Remove stale components ─────────────────
        SafeDestroy<CinemachineFollow>();
        SafeDestroy<CinemachineBasicMultiChannelPerlin>();

        // ── Step 5 — Add & configure CinemachineFollow ───────
        _follow = gameObject.AddComponent<CinemachineFollow>();
        _follow.FollowOffset = followOffset;

        // ── Step 6 — Add & configure noise (shake) ──────────
        _noise = gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
        _noise.AmplitudeGain = shakeAmplitude;
        _noise.FrequencyGain = shakeFrequency;

        // ── Step 7 — Set lens FOV ────────────────────────────
        _vcam.Lens = new LensSettings
        {
            FieldOfView = defaultFOV,
            NearClipPlane = 0.01f,
            FarClipPlane = 1000f
        };

        // ── Step 8 — Snap to initial position ────────────────
        SnapToTarget();

        // ── Step 9 — Deprioritize other VCams ────────────────
        DeprioritizeOtherVCams();

        _initialized = true;
        Debug.Log("[CameraManager] Auto-configuration complete. Camera is live.");
    }

    // ══════════════════════════════════════════════════════════
    // ENSURE BRAIN
    // ══════════════════════════════════════════════════════════

    void EnsureCinemachineBrain()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject go = new GameObject("MainCamera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 1000f;
        }
        if (!cam.TryGetComponent<CinemachineBrain>(out _))
            cam.gameObject.AddComponent<CinemachineBrain>();
    }

    // ══════════════════════════════════════════════════════════
    // SAFE DESTROY
    // ══════════════════════════════════════════════════════════

    void SafeDestroy<T>() where T : Behaviour
    {
        if (TryGetComponent<T>(out var comp))
            DestroyImmediate(comp);
    }

    // ══════════════════════════════════════════════════════════
    // SNAP TO TARGET (no initial lerp)
    // ══════════════════════════════════════════════════════════

    void SnapToTarget()
    {
        if (target == null) return;
        Vector3 pos = target.position + followOffset;
        transform.SetPositionAndRotation(
            pos,
            Quaternion.LookRotation(
                (target.position + Vector3.up * aimOffsetY) - pos
            )
        );
    }

    // ══════════════════════════════════════════════════════════
    // DEPRIORITIZE OTHER VCAMS
    // ══════════════════════════════════════════════════════════

    void DeprioritizeOtherVCams()
    {
        CinemachineCamera[] all = FindObjectsByType<CinemachineCamera>();
        foreach (var other in all)
        {
            if (other != _vcam)
                other.Priority = 0;
        }
    }

    // ══════════════════════════════════════════════════════════
    // DYNAMIC FOV — subtle sense of speed
    // ══════════════════════════════════════════════════════════

    void ApplyDynamicFOV()
    {
        if (target == null) return;

        CharacterController cc = target.GetComponent<CharacterController>();
        float speed = 0f;

        if (cc != null && cc.enabled)
            speed = new Vector3(cc.velocity.x, 0f, cc.velocity.z).magnitude;

        float t = Mathf.Clamp01(speed / fovSpeedScale);
        float goalFOV = Mathf.Lerp(defaultFOV, maxFOV, t);

        LensSettings lens = _vcam.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, goalFOV, Time.deltaTime * 3f);
        _vcam.Lens = lens;
    }

    // ══════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════

    /// <summary>Change the follow offset at runtime.</summary>
    public void SetFollowOffset(Vector3 offset)
    {
        followOffset = offset;
        if (_follow != null)
            _follow.FollowOffset = offset;
    }

    /// <summary>Smoothly transition to a new FOV over duration seconds.</summary>
    public void SetTargetFOV(float fov, float duration = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(FOVTransition(fov, duration));
    }

    private System.Collections.IEnumerator FOVTransition(float goal, float duration)
    {
        float start = _vcam.Lens.FieldOfView;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            LensSettings lens = _vcam.Lens;
            lens.FieldOfView = Mathf.Lerp(start, goal, t);
            _vcam.Lens = lens;
            yield return null;
        }
    }

    /// <summary>Set camera shake (0,0 = off).</summary>
    public void SetShake(float amplitude, float frequency)
    {
        if (_noise != null)
        {
            _noise.AmplitudeGain = amplitude;
            _noise.FrequencyGain = frequency;
        }
    }

    /// <summary>Shake strongly for duration seconds, then fade out.</summary>
    public void ShakeForDuration(float amplitude, float frequency,
                                 float duration, float fadeOut = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(amplitude, frequency, duration, fadeOut));
    }

    private System.Collections.IEnumerator ShakeRoutine(
        float amp, float freq, float duration, float fade)
    {
        SetShake(amp, freq);
        yield return new WaitForSeconds(duration);

        float elapsed = 0f;
        while (elapsed < fade)
        {
            elapsed += Time.deltaTime;
            SetShake(amp * (1f - elapsed / fade), freq * (1f - elapsed / fade));
            yield return null;
        }
        SetShake(0f, 0f);
    }

    /// <summary>Switch the follow target (e.g. cutscene).</summary>
    public void SetFollowTarget(Transform newTarget)
    {
        target = newTarget;
        if (_vcam != null)
        {
            _vcam.Follow = newTarget;
            _vcam.LookAt = newTarget;
        }
    }
}