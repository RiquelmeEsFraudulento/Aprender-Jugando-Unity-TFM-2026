// ═══════════════════════════════════════════════════════════════════════════
// StoryTextData.cs — FINAL CLEAN VERSION
//
// ScriptableObject que almacena toda la historia del videojuego.
// Los textos se rellenan automáticamente desde el menú del editor.
//
// CREAR: TIC Andalucía > Crear Story Data (completo)
// ═══════════════════════════════════════════════════════════════════════════

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StoryTextData", menuName = "TIC Andalucía/Story Text Data")]
public class StoryTextData : ScriptableObject
{
    [System.Serializable]
    public class SceneTexts
    {
        [Header("Identificación")]
        public string sceneName = "Nueva Escena";
        public int sceneIndex = 0;

        [Header("Tipo")]
        public SceneType sceneType = SceneType.Battle;

        [Header("Textos INTRO (pantalla negra, ENTER para avanzar)")]
        [TextArea(3, 15)]
        public List<string> introTexts = new List<string>();

        [Header("Textos OUTRO (pantalla negra, ENTER para avanzar)")]
        [TextArea(3, 15)]
        public List<string> outroTexts = new List<string>();

        [Header("Descripción de la batalla")]
        [TextArea(2, 5)]
        public string battleDescription = "";

        [Header("Mensaje temático")]
        [TextArea(2, 4)]
        public string thematicMessage = "";

        [Header("Música sugerida")]
        public string musicSuggestion = "";

        [Header("Enemigo")]
        public string enemyName = "";
        public string enemyDescription = "";
    }

    public enum SceneType
    {
        Testing,
        Battle,
        Boss,
        Final
    }

    [Header("Textos de todas las escenas")]
    public List<SceneTexts> allSceneTexts = new List<SceneTexts>();

    [Header("Texto de muerte del jugador")]
    [TextArea(2, 4)]
    public string deathText = "No me rendiré...\n\n[ENTER para continuar]";

    [Header("Mensaje global del juego")]
    [TextArea(3, 6)]
    public string globalMessage = "Los niños pueden tener peluche.\n" +
        "Las niñas pueden ser fans de Fernando Alonso.\n" +
        "Solo alcanzaremos la paz cuando aceptemos a todos.";

    [Header("Arco del Ninja (referencia)")]
    [TextArea(3, 8)]
    public string narrativeArc = "";

    [Header("Arco del Dummy (referencia)")]
    [TextArea(3, 8)]
    public string dummyArc = "";

    // ── API ──────────────────────────────────────────────────

    public SceneTexts GetSceneTexts(int sceneIndex)
    {
        for (int i = 0; i < allSceneTexts.Count; i++)
            if (allSceneTexts[i].sceneIndex == sceneIndex)
                return allSceneTexts[i];
        return null;
    }

    public SceneTexts GetSceneTexts(string sceneName)
    {
        for (int i = 0; i < allSceneTexts.Count; i++)
            if (allSceneTexts[i].sceneName == sceneName)
                return allSceneTexts[i];
        return null;
    }

    public string[] GetIntroArray(int sceneIndex)
    {
        SceneTexts st = GetSceneTexts(sceneIndex);
        return st != null ? st.introTexts.ToArray() : new string[0];
    }

    public string[] GetOutroArray(int sceneIndex)
    {
        SceneTexts st = GetSceneTexts(sceneIndex);
        return st != null ? st.outroTexts.ToArray() : new string[0];
    }

    public string GetThematicMessage(int sceneIndex)
    {
        SceneTexts st = GetSceneTexts(sceneIndex);
        return st != null ? st.thematicMessage : "";
    }

    public int GetSceneCount() => allSceneTexts.Count;
}