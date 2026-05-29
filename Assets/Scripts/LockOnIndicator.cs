using UnityEngine;

/// <summary>
/// Put this script on the ROOT GameObject of the LockOn prefab.
/// The prefab hierarchy should be:
///
///   LockOnIndicator  (this script)
///     └── Canvas     (Render Mode = World Space, size e.g. 1×1 units)
///           └── Icon  (Image component, assign a sprite or leave as white square)
///
/// The SimpleWalk script instantiates this prefab as a child of the
/// locked enemy and handles positioning / billboarding.
/// </summary>
public class LockOnIndicator : MonoBehaviour
{
    [Header("Scale animation")]
    public float appearDuration = 0.25f;
    public float pulseSpeed     = 2f;
    public float pulseAmount    = 0.15f;
    public Vector3 baseScale    = new Vector3(1f, 1f, 1f);

    [Header("Range Indicator")]
    public RangeIndicator rangeIndicator;

    private float _timer;
    private Canvas _canvas;

    void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>();
        // Start from zero scale for pop-in effect
        transform.localScale = Vector3.zero;

        // Auto-find RangeIndicator in children
        rangeIndicator = GetComponentInChildren<RangeIndicator>();
    }

    void OnEnable()
    {
        _timer = 0f;
    }

    void Update()
    {
        _timer += Time.deltaTime;

        // ── Phase 1: Pop-in (scale from 0 → 1) ──
        if (_timer < appearDuration)
        {
            float t = _timer / appearDuration;
            t = t * t; // ease-in
            transform.localScale = Vector3.Lerp(Vector3.zero, baseScale, t);
            return;
        }

        // ── Phase 2: Idle pulse ──
        float pulse = Mathf.Sin(_timer * pulseSpeed * Mathf.PI * 2f) * pulseAmount;
        transform.localScale = baseScale + Vector3.one * pulse;
    }
}