using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUDController : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;   // drag your Player GameObject here

    // Cached visual elements
    private VisualElement _healthFill;
    private VisualElement _xpFill;
    private Label         _levelLabel;
    private VisualElement _stateCircle;

    // USS class names for the state circle
    private const string STATE_NONE     = "state-none";
    private const string STATE_SLEEP    = "state-sleep";
    private const string STATE_CONFUSED = "state-confused";

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        _healthFill  = root.Q<VisualElement>("health-fill");
        _xpFill      = root.Q<VisualElement>("xp-fill");
        _levelLabel  = root.Q<Label>("level-label");
        _stateCircle = root.Q<VisualElement>("state-circle");
    }

    void Update()
    {
        if (playerHealth == null) return;

        // ── Health bar fill (0-100%) ──
        float healthPct = playerHealth.vida / playerHealth.vidaMax;
        _healthFill.style.width = new StyleLength(Length.Percent(healthPct * 100f));

        // ── XP bar fill ──
        // XP needed for next level = level * 10  (matches your GanarXP logic)
        float xpNeeded = playerHealth.level * 10f;
        float xpPct    = Mathf.Clamp01(playerHealth.XP / xpNeeded);
        _xpFill.style.width = new StyleLength(Length.Percent(xpPct * 100f));

        // ── Level label ──
        _levelLabel.text = ((int)playerHealth.level).ToString();

        // ── State circle: keys 7 / 8 / 9 ──
        if (Input.GetKeyDown(KeyCode.Alpha7))
            SetState(STATE_SLEEP);
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            SetState(STATE_CONFUSED);
        else if (Input.GetKeyDown(KeyCode.Alpha9))
            SetState(STATE_NONE);
    }

    // Swap USS class so only the active state class is applied
    private void SetState(string newStateClass)
    {
        _stateCircle.RemoveFromClassList(STATE_NONE);
        _stateCircle.RemoveFromClassList(STATE_SLEEP);
        _stateCircle.RemoveFromClassList(STATE_CONFUSED);
        _stateCircle.AddToClassList(newStateClass);
    }
}