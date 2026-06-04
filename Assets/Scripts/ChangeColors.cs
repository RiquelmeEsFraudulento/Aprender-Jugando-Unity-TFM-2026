// ============================================================
// LightsaberColorController.cs — VERSIÓN ENCAPSULADA COMPLETA
// CON EJERCICIOS DIDÁCTICOS PARA BACHILLERATO TIC 1
// ============================================================
// Controla el color del Sable de Luz.
//
// TECLAS:
//   1 → Rojo
//   2 → Azul
//   3 → Gris
//
// ORGANIZACIÓN EN TRES BLOQUES:
//   BLOQUE 1 — Inicialización de variables + constantes
//   BLOQUE 2 — Ejercicios en estilo C++ (el alumno trabaja aquí)
//   BLOQUE 3 — Funciones de trabajo sucio Unity/C# (alumno NO toca)
//
// TODA la funcionalidad original se mantiene íntegra.
// ============================================================

using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LightsaberColorController : MonoBehaviour
{
    // =====================================================================
    // BLOQUE 1 — INICIALIZACIÓN DE VARIABLES Y ESTRUCTURAS
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │  1A — CONSTANTES DEL ALUMNO                             │
    // └─────────────────────────────────────────────────────────┘

    public const int COLOR_ROJO  = 0;
    public const int COLOR_AZUL  = 1;
    public const int COLOR_GRIS  = 2;
    public const int COLOR_BLANCO = 3;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1B — ESTRUCTURA DE DATOS DE LA FICHA DEL SABLE         │
    // └─────────────────────────────────────────────────────────┘

    [System.Serializable]
    public struct FichaColorSable
    {
        public int colorActual;         // 0=Rojo, 1=Azul, 2=Gris
        public int tipoDanyo;           // DamageType correspondiente
    }

    public FichaColorSable ficha;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1C — VARIABLES DE DEBUG PARA EJERCICIOS [EJ]           │
    // └─────────────────────────────────────────────────────────┘

    [Header("═══ DEBUG EJERCICIOS [EJ] ═══")]
    public int debugEJ_colorElegido;
    public int debugEJ_tipoDanyo;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1D — INSPECTOR (variables originales)                   │
    // └─────────────────────────────────────────────────────────┘

    [Header("Input")]
    public KeyCode redKey  = KeyCode.Alpha1;
    public KeyCode blueKey = KeyCode.Alpha2;
    public KeyCode grayKey = KeyCode.Alpha3;

    [Header("Current State (read‑only at runtime)")]
    public DamageType currentDamageType = DamageType.Red;

    Renderer rend;
    Material instancedMaterial;
    [SerializeField] private WeaponHitbox hitbox;

    // ══════════════════════════════════════════════════════════
    // AWAKE
    // ══════════════════════════════════════════════════════════

    void Awake()
    {
        rend = GetComponent<Renderer>();
        instancedMaterial = rend.material;
        ApplyCurrentColor();

        ficha.colorActual = COLOR_ROJO;
        ficha.tipoDanyo = COLOR_ROJO;
    }

    // ══════════════════════════════════════════════════════════
    // UPDATE
    // ══════════════════════════════════════════════════════════

    void Update()
    {
        /*
        if (Input.GetKeyDown(redKey))
            SetSaberColor(DamageType.Red);
        if (Input.GetKeyDown(blueKey))
            SetSaberColor(DamageType.Blue);
        if (Input.GetKeyDown(grayKey))
            SetSaberColor(DamageType.Gray);

        SincronizarFicha();
        */

        if (Input.GetKeyDown(redKey))
        {
            int color = ColorSegunTecla(redKey);
            if (color != -1) SetSaberColor(ConstanteAColor(color));
        }
        if (Input.GetKeyDown(blueKey))
        {
            int color = ColorSegunTecla(blueKey);
            if (color != -1) SetSaberColor(ConstanteAColor(color));
        }
        if (Input.GetKeyDown(grayKey))
        {
            int color = ColorSegunTecla(grayKey);
            if (color != -1) SetSaberColor(ConstanteAColor(color));
        }

        SincronizarFicha();
    }

    void SincronizarFicha()
    {
        switch (currentDamageType)
        {
            case DamageType.Red:   ficha.colorActual = COLOR_ROJO; break;
            case DamageType.Blue:  ficha.colorActual = COLOR_AZUL; break;
            case DamageType.Gray:  ficha.colorActual = COLOR_GRIS; break;
            default:               ficha.colorActual = COLOR_ROJO; break;
        }
        ficha.tipoDanyo = ficha.colorActual;
    }

    // =====================================================================
    // BLOQUE 2 — EJERCICIOS EN ESTILO C++
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │         HERRAMIENTAS QUE PUEDES USAR LIBREMENTE         │
    // │         (ya están implementadas, no las toques)         │
    // └─────────────────────────────────────────────────────────┘

    /// <summary>
    /// Convierte un DamageType de color a su constante int.
    /// </summary>
    int ColorAConstante(DamageType tipo)
    {
        switch (tipo)
        {
            case DamageType.Red:  return COLOR_ROJO;
            case DamageType.Blue: return COLOR_AZUL;
            case DamageType.Gray: return COLOR_GRIS;
            default:              return COLOR_BLANCO;
        }
    }

    /// <summary>
    /// Convierte una constante int a su DamageType correspondiente.
    /// </summary>
    DamageType ConstanteAColor(int constante)
    {
        switch (constante)
        {
            case COLOR_ROJO:  return DamageType.Red;
            case COLOR_AZUL:  return DamageType.Blue;
            case COLOR_GRIS:  return DamageType.Gray;
            default:          return DamageType.Red;
        }
    }

    /// <summary>
    /// Devuelve TRUE si la tecla corresponde al color Rojo.
    /// </summary>
    bool EsTeclaRojo(KeyCode tecla)
    {
        return tecla == KeyCode.Alpha1;
    }

    /// <summary>
    /// Devuelve TRUE si la tecla corresponde al color Azul.
    /// </summary>
    bool EsTeclaAzul(KeyCode tecla)
    {
        return tecla == KeyCode.Alpha2;
    }

    /// <summary>
    /// Devuelve TRUE si la tecla corresponde al color Gris.
    /// </summary>
    bool EsTeclaGris(KeyCode tecla)
    {
        return tecla == KeyCode.Alpha3;
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 1 — ApplyCurrentColor: ¿Qué color aplicar? (switch/case)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El sable de luz puede ser Rojo, Azul o Gris.
    //  Cada color tiene un valor RGB diferente. Este ejercicio
    //  decide qué color aplicar según el tipo de daño.
    //
    //  EJEMPLO REAL:
    //  - DamageType.Red   → RGB(1, 0, 0) = Rojo puro
    //  - DamageType.Blue  → RGB(0, 1, 1) = Cyan (azul claro)
    //  - DamageType.Gray  → RGB(0.5, 0.5, 0.5) = Gris medio
    //
    //  OBJETIVO: Recibe un tipo de daño y devuelve el color como
    //  tres valores RGB (R, G, B) en un struct.
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: ColorAConstante(), switch/case
    //  ❌ No uses: Color, UnityEngine directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Color RGB representado con tres floats.
    /// </summary>
    public struct ColorRGB
    {
        public float R;  // Rojo (0 a 1)
        public float G;  // Verde (0 a 1)
        public float B;  // Azul (0 a 1)
    }

    /// <param name="tipo">Tipo de daño de color.</param>
    /// <returns>Color RGB correspondiente.</returns>
    ColorRGB ApplyCurrentColor(DamageType tipo)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        ColorRGB color;

        int colorConstante = ColorAConstante(tipo);

        switch (colorConstante)
        {
            case COLOR_ROJO:
                color.R = 1.0f;
                color.G = 0.0f;
                color.B = 0.0f;
                break;

            case COLOR_AZUL:
                // Cian (azul claro)
                color.R = 0.0f;
                color.G = 1.0f;
                color.B = 1.0f;
                break;

            case COLOR_GRIS:
                color.R = 0.5f;
                color.G = 0.5f;
                color.B = 0.5f;
                break;

            default:
                // Blanco por defecto
                color.R = 1.0f;
                color.G = 1.0f;
                color.B = 1.0f;
                break;
        }

        return color;

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 2 — ¿Qué color según la tecla? (if/else)
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador pulsa 1, 2 o 3 para cambiar el color.
    //  Este ejercicio decide qué color activar según la tecla.
    //
    //  OBJETIVO: Recibe una tecla y devuelve el color correspondiente:
    //    Tecla 1 → 0 (Rojo)
    //    Tecla 2 → 1 (Azul)
    //    Tecla 3 → 2 (Gris)
    //    Otra tecla → -1 (no cambiar)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: EsTeclaRojo(), EsTeclaAzul(), EsTeclaGris(), if/else
    //  ❌ NO uses: Input, KeyCode directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="tecla">Tecla pulsada.</param>
    /// <returns>Color a activar: 0=Rojo, 1=Azul, 2=Gris, -1=Ninguno</returns>
    int ColorSegunTecla(KeyCode tecla)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        if (EsTeclaRojo(tecla))
        {
            return COLOR_ROJO;
        }
        else if (EsTeclaAzul(tecla))
        {
            return COLOR_AZUL;
        }
        else if (EsTeclaGris(tecla))
        {
            return COLOR_GRIS;
        }
        else
        {
            return -1;  // tecla no reconocida
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // =====================================================================
    // BLOQUE 3 — TRABAJO SUCIO UNITY/C# (alumno NO toca)
    // =====================================================================

    public void SetSaberColor(DamageType type)
    {
        currentDamageType = type;
        ApplyCurrentColor();
    }

    void ApplyCurrentColor()
    {
        if (instancedMaterial == null) return;

        // ── USAMOS LA FUNCIÓN DEL EJERCICIO 1 ──
        ColorRGB colorRGB = ApplyCurrentColor(currentDamageType);

        // Convertir a Color de Unity
        Color c = new Color(colorRGB.R, colorRGB.G, colorRGB.B, 1f);

        debugEJ_colorElegido = ColorAConstante(currentDamageType);

        if (instancedMaterial.HasProperty("_BaseColor"))
            instancedMaterial.SetColor("_BaseColor", c);
        else if (instancedMaterial.HasProperty("_Color"))
            instancedMaterial.SetColor("_Color", c);

        if (hitbox != null)
        {
            hitbox.damageType = currentDamageType;
            Debug.Log("CAMBIE DE COLOR");
        }
    }
}