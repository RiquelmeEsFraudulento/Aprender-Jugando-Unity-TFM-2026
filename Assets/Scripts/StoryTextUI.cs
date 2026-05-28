using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PREMIUM StoryTextUI — High quality full-screen black overlay with text.
/// Features:
///   - Smooth fade in/out transitions
///   - Crisp, anti-aliased text
///   - ENTER or SPACE to advance paragraphs
///   - CLICK to skip
/// </summary>
public class StoryTextUI_Canvas : MonoBehaviour
{
    public static StoryTextUI_Canvas Instance { get; private set; }

    [Header("Text Settings")]
    public Color textColor = Color.white;
    public int fontSize = 36;

    [Header("Background Settings")]
    public Color backgroundColor = new Color(0, 0, 0, 1f);

    [Header("Animation Settings")]
    public float fadeDuration = 0.4f;

    [Header("Skip Hint")]
    public string skipHint = "[ Click to skip ]";
    public Color skipHintColor = new Color(1, 1, 1, 0.4f);
    public int skipHintSize = 18;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private Image _background;
    private Text _text;
    private Text _skipHint;
    private bool _isVisible = false;
    private Coroutine _currentCoroutine;

    // ══════════════════════════════════════════════════════════
    // SINGLETON
    // ══════════════════════════════════════════════════════════

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CreateUI();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ══════════════════════════════════════════════════════════
    // UPDATE — Check for input here for reliability
    // ══════════════════════════════════════════════════════════

    void Update()
    {
        // Only process input when visible
        if (!_isVisible) return;

        // Check for skip (any mouse button)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            _awaitingInput = false;
            Debug.Log("[StoryTextUI] Skip requested (mouse click).");
        }

        // Check for advance (ENTER or SPACE)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            _awaitingInput = false;
            Debug.Log("[StoryTextUI] Advance requested (ENTER/SPACE).");
        }
    }

    // ══════════════════════════════════════════════════════════
    // UI CREATION
    // ══════════════════════════════════════════════════════════

    void CreateUI()
    {
        Debug.Log("[StoryTextUI] Creating PREMIUM Canvas UI...");

        // ── Create Canvas ────────────────────────────────────
        GameObject canvasGO = new GameObject("StoryTextCanvas");
        DontDestroyOnLoad(canvasGO);

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── CanvasGroup for fade ─────────────────────────────
        _canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        // ── Create background ────────────────────────────────
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        _background = bgGO.AddComponent<Image>();
        _background.color = backgroundColor;
        _background.raycastTarget = true;

        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // ── Create Text Container ────────────────────────────
        GameObject textContainer = new GameObject("TextContainer");
        textContainer.transform.SetParent(canvasGO.transform, false);

        RectTransform containerRect = textContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.1f, 0.25f);
        containerRect.anchorMax = new Vector2(0.9f, 0.75f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        // ── Create main text ─────────────────────────────────
        GameObject textGO = new GameObject("StoryText");
        textGO.transform.SetParent(textContainer.transform, false);

        _text = textGO.AddComponent<Text>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null) _text.font = font;

        _text.fontSize = fontSize;
        _text.fontStyle = FontStyle.Bold;
        _text.color = textColor;
        _text.alignment = TextAnchor.MiddleCenter;
        _text.horizontalOverflow = HorizontalWrapMode.Wrap;
        _text.verticalOverflow = VerticalWrapMode.Overflow;
        _text.raycastTarget = false;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        // ── Create skip hint ─────────────────────────────────
        GameObject hintGO = new GameObject("SkipHint");
        hintGO.transform.SetParent(textContainer.transform, false);

        _skipHint = hintGO.AddComponent<Text>();
        _skipHint.text = skipHint;
        _skipHint.font = _text.font;
        _skipHint.fontSize = skipHintSize;
        _skipHint.fontStyle = FontStyle.Italic;
        _skipHint.color = skipHintColor;
        _skipHint.alignment = TextAnchor.MiddleCenter;
        _skipHint.raycastTarget = false;

        RectTransform hintRect = hintGO.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.sizeDelta = new Vector2(400, 40);
        hintRect.anchoredPosition = new Vector2(0, 20);

        // ── Hide by default ─────────────────────────────────
        canvasGO.SetActive(false);

        Debug.Log("[StoryTextUI] PREMIUM Canvas UI created.");
    }

    // ══════════════════════════════════════════════════════════
    // PUBLIC API
    // ══════════════════════════════════════════════════════════

    private bool _awaitingInput = false;

    public void ShowText(string text)
    {
        if (_text == null)
        {
            Debug.LogError("[StoryTextUI] UI not initialized!");
            return;
        }

        _text.text = text;
        _text.gameObject.SetActive(true);
        _text.transform.parent.gameObject.SetActive(true);
        _text.transform.parent.parent.gameObject.SetActive(true);
        _isVisible = true;
        _awaitingInput = false;

        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        _currentCoroutine = StartCoroutine(FadeIn());

        Debug.Log($"[StoryTextUI] Showing: {text.Substring(0, Mathf.Min(50, text.Length))}...");
    }

    public void HideText()
    {
        if (_canvas == null) return;

        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        _currentCoroutine = StartCoroutine(FadeOut());

        Debug.Log("[StoryTextUI] Hiding...");
    }

    public bool IsVisible()
    {
        return _isVisible;
    }

    /// <summary>
    /// Wait for player input (ENTER/SPACE/CLICK).
    /// This is the COROUTINE that GameManager will wait on.
    /// </summary>
    public IEnumerator WaitForInput()
    {
        Debug.Log("[StoryTextUI] Waiting for input...");

        // Reset input flag
        _awaitingInput = false;

        // Use unscaled time so it works during cutscenes (Time.timeScale = 0)
        float timeout = 0.1f;
        float elapsed = 0f;

        // Small delay to prevent instant skip from previous key press
        while (elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.Log("[StoryTextUI] Ready for input. Waiting...");

        // Wait until input is detected (set by Update)
        yield return new WaitUntil(() => !_isVisible || _awaitingInput == false);

        // Actually, let's do it properly — wait for the flag change
        _awaitingInput = true; // Reset to true, Update() will set to false

        // Wait until Update() detects input and sets _awaitingInput = false
        yield return new WaitUntil(() => _awaitingInput == false);

        Debug.Log("[StoryTextUI] Input received! Continuing...");
    }

    // ══════════════════════════════════════════════════════════
    // ANIMATIONS
    // ══════════════════════════════════════════════════════════

    IEnumerator FadeIn()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvas.gameObject.SetActive(true);
        _canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            t = t * t; // Ease in
            _canvasGroup.alpha = t;
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _isVisible = true;

        Debug.Log("[StoryTextUI] Fade in complete.");
    }

    IEnumerator FadeOut()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        float startAlpha = _canvasGroup.alpha;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            t = 1f - ((1f - t) * (1f - t)); // Ease out
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _canvas.gameObject.SetActive(false);
        _isVisible = false;

        Debug.Log("[StoryTextUI] Fade out complete.");
    }
}