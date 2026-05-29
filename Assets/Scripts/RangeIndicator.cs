// ============================================================
// RangeIndicator.cs
// ============================================================
// Attach to the LockOn prefab root.
// In the Inspector, click "Generate Range Sprites" to
// auto-create and configure all 3 child sprite objects.
// ============================================================
using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    [Header("Range thresholds (must match SimpleWalk)")]
    public float kickRange       = 1.5f;
    public float rapierRange     = 3f;
    public float lightSaberRange = 5f;

    [Header("Range sprites [circle, square, triangle]")]
    [Tooltip("Auto-filled by 'Generate Range Sprites' button")]
    public GameObject[] rangeSprites;

    [Header("Visual")]
    public float spriteScale     = 0.4f;
    public float pulseSpeed      = 3f;
    public float pulseAmount     = 0.08f;
    public Color inRangeColor    = Color.white;
    public Color farRangeColor   = new Color(0.5f, 0.5f, 0.5f, 0.6f);
    public Color outOfRangeColor = new Color(0.3f, 0.3f, 0.3f, 0.2f);

    [Header("Debug")]
    [SerializeField] private int _currentRangesInRange;
    [SerializeField] private string _activeWeapon;

    private Transform _target;
    private Transform _player;
    private Vector3[] _baseScales;
    private float _timer;
    private string[] _weaponNames = { "LightSaber", "Rapier+Lightsaber", "Kick+Rapier+Lightsaber" };

    void Awake()
    {
        // Ensure array exists at runtime
        if (rangeSprites == null)
            rangeSprites = new GameObject[3];

        _baseScales = new Vector3[3];
    }

    public void Initialize(Transform target, Transform player)
    {
        _target = target;
        _player = player;
        _timer = 0f;

        if (rangeSprites == null)
            rangeSprites = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            if (rangeSprites[i] != null)
            {
                rangeSprites[i].SetActive(false);
                rangeSprites[i].transform.localScale = Vector3.one * spriteScale;
                _baseScales[i] = Vector3.one * spriteScale;
            }
            else
            {
                _baseScales[i] = Vector3.one * spriteScale;
            }
        }
    }

    void Update()
    {
        if (_target == null || _player == null) return;

        _timer += Time.deltaTime;

        float dist = Vector3.Distance(_player.position, _target.position);

        int rangesInRange = 0;
        if (dist <= lightSaberRange) rangesInRange++;
        if (dist <= rapierRange) rangesInRange++;
        if (dist <= kickRange) rangesInRange++;

        _currentRangesInRange = rangesInRange;

        int activeIndex = -1;
        bool anyInRange = true;

        switch (rangesInRange)
        {
            case 3: activeIndex = 2; _activeWeapon = _weaponNames[2]; break;
            case 2: activeIndex = 1; _activeWeapon = _weaponNames[1]; break;
            case 1: activeIndex = 0; _activeWeapon = _weaponNames[0]; break;
            default: anyInRange = false; _activeWeapon = "None"; break;
        }

        for (int i = 0; i < rangeSprites.Length; i++)
        {
            if (rangeSprites[i] == null) continue;

            bool isActive = (i == activeIndex && anyInRange);
            rangeSprites[i].SetActive(isActive);

            if (isActive)
            {
                float pulse = Mathf.Sin(_timer * pulseSpeed * Mathf.PI * 2f) * pulseAmount;
                rangeSprites[i].transform.localScale = _baseScales[i] + Vector3.one * pulse;

                SpriteRenderer sr = rangeSprites[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = (dist <= kickRange) ? inRangeColor
                              : (dist <= rapierRange) ? farRangeColor
                              : outOfRangeColor;
                }
            }
        }
    }
}