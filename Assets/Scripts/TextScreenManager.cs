// ============================================================
// TextScreenManager.cs
// ============================================================
// Gestiona las pantallas de texto (intro/outro/muerte).
// Pantalla negra con texto centrado, ENTER para avanzar.
//
// Se crea dinámicamente por el GameManager si no existe.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextScreenManager : MonoBehaviour
{
    private Canvas canvas;
    private Text messageText;
    private CanvasGroup canvasGroup;
    private bool isVisible = false;

    public void Initialize()
    {
        // Crear Canvas
        GameObject canvasGO = new GameObject("TextCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Siempre encima
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        canvasGroup = canvasGO.AddComponent<CanvasGroup>();

        // Fondo negro
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform);
        Image bg = bgGO.AddComponent<Image>();
        bg.color = Color.black;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Texto
        GameObject textGO = new GameObject("MessageText");
        textGO.transform.SetParent(canvasGO.transform);
        messageText = textGO.AddComponent<Text>();
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.fontSize = 28;
        messageText.color = Color.white;
        messageText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = messageText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.2f);
        textRect.anchorMax = new Vector2(0.9f, 0.8f);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // Iniciar oculto
        HideText();

        DontDestroyOnLoad(canvasGO);
        DontDestroyOnLoad(gameObject);
    }

    public void ShowText(string text)
    {
        if (messageText != null)
        {
            messageText.text = text;
            canvasGroup.alpha = 1f;
            isVisible = true;
        }
    }

    public void HideText()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            isVisible = false;
        }
    }

    public bool IsVisible() => isVisible;
}