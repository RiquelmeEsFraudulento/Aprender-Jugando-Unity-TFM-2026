// ============================================================
// RapierColorController.cs — VERSIÓN ENCAPSULADA COMPLETA
// CON EJERCICOS DIDÁCTICOS PARA BACHILLERATO TIC 1
// ============================================================
// Controla el modo/color del Rapier.
//
// TECLAS:
//   4 → Normal
//   5 → Veneno
//   6 → Sangrado
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
public class RapierColorController : MonoBehaviour
{
    // =====================================================================
    // BLOQUE 1 — INICIALIZACIÓN DE VARIABLES Y ESTRUCTURAS
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │  1A — CONSTANTES DEL ALUMNO                             │
    // └─────────────────────────────────────────────────────────┘

    public const int MODO_NORMAL   = 0;
    public const int MODO_VENENO   = 1;
    public const int MODO_SANGRADO = 2;

    public const int COLOR_BLANCO  = 0;
    public const int COLOR_MORADO  = 1;
    public const int COLOR_ROJO    = 2;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1B — ESTRUCTURA DE DATOS DE LA FICHA DEL RAPIER        │
    // └─────────────────────────────────────────────────────────┘

    [System.Serializable]
    public struct FichaRapier
    {
        public int modoActual;          // 0=Normal, 1=Veneno, 2=Sangrado
        public int colorActual;         // 0=Blanco, 1=Morado, 2=Rojo
        public int tipoDanyo;           // DamageType como constante
    }

    public FichaRapier ficha;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1C — VARIABLES DE DEBUG PARA EJERCICIOS [EJ]           │
    // └─────────────────────────────────────────────────────────┘

    [Header("═══ DEBUG EJERCICIOS [EJ] ═══")]
    public int debugEJ_modoElegido;
    public int debugEJ_colorCalculado;
    public int debugEJ_tipoDanyo;

    // ┌─────────────────────────────────────────────────────────┐
    // │  1D — INSPECTOR (variables originales)                   │
    // └─────────────────────────────────────────────────────────┘

    [Header("Teclas de modo")]
    public KeyCode teclaNormal   = KeyCode.Alpha4;
    public KeyCode teclaVeneno   = KeyCode.Alpha5;
    public KeyCode teclaSangrado = KeyCode.Alpha6;

    [Header("Estado actual (lectura en tiempo real)")]
    public DamageType currentDamageType = DamageType.Normal;

    Renderer rend;
    public Material instancedMaterial;
    [SerializeField] private RapierHitbox hitbox;

    // ══════════════════════════════════════════════════════════
    // AWAKE
    // ══════════════════════════════════════════════════════════

    void Awake()
    {
        ObtenerReferenciaDelRenderer();
        CrearMaterialInstanciado();
        AplicarColorActual();

        ficha.modoActual = MODO_NORMAL;
        ficha.colorActual = COLOR_BLANCO;
        ficha.tipoDanyo = MODO_NORMAL;
    }

    // ══════════════════════════════════════════════════════════
    // UPDATE
    // ══════════════════════════════════════════════════════════

    void Update()
    {
        DetectarCambioDeModo();
        SincronizarFicha();
    }

    void SincronizarFicha()
    {
        ficha.modoActual = TipoAConstante(currentDamageType);
        ficha.tipoDanyo = ficha.modoActual;
        ficha.colorActual = ColorSegunModo(ficha.modoActual);
    }

    // =====================================================================
    // BLOQUE 2 — EJERCICIOS EN ESTILO C++
    // =====================================================================

    // ┌─────────────────────────────────────────────────────────┐
    // │         HERRAMIENTAS QUE PUEDES USAR LIBREMENTE         │
    // │         (ya están implementadas, no las toques)         │
    // └─────────────────────────────────────────────────────────┘

    int TipoAConstante(DamageType tipo)
    {
        switch (tipo)
        {
            case DamageType.Normal:  return MODO_NORMAL;
            case DamageType.Poison:  return MODO_VENENO;
            case DamageType.Bleed:   return MODO_SANGRADO;
            default:                 return MODO_NORMAL;
        }
    }

    DamageType ConstanteATipo(int constante)
    {
        switch (constante)
        {
            case MODO_NORMAL:   return DamageType.Normal;
            case MODO_VENENO:   return DamageType.Poison;
            case MODO_SANGRADO: return DamageType.Bleed;
            default:            return DamageType.Normal;
        }
    }

    bool EsTeclaNormal(KeyCode tecla)   { return tecla == KeyCode.Alpha4; }
    bool EsTeclaVeneno(KeyCode tecla)   { return tecla == KeyCode.Alpha5; }
    bool EsTeclaSangrado(KeyCode tecla) { return tecla == KeyCode.Alpha6; }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 1 — DetectarCambioDeModo: ¿Qué modo elegir?
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: El jugador pulsa 4, 5 o 6 para cambiar el modo del Rapier.
    //
    //  OBJETIVO: Recibe una tecla y devuelve el modo correspondiente:
    //    Tecla 4 → 0 (Normal)
    //    Tecla 5 → 1 (Veneno)
    //    Tecla 6 → 2 (Sangrado)
    //    Otra tecla → -1 (no cambiar)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: EsTeclaNormal(), EsTeclaVeneno(), EsTeclaSangrado(), if/else
    //  ❌ NO uses: Input, KeyCode directamente
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="tecla">Tecla pulsada.</param>
    /// <returns>Modo a activar: 0=Normal, 1=Veneno, 2=Sangrado, -1=Ninguno</returns>
    int DetectarCambioDeModo(KeyCode tecla)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        if (EsTeclaNormal(tecla))
        {
            return MODO_NORMAL;
        }
        else if (EsTeclaVeneno(tecla))
        {
            return MODO_VENENO;
        }
        else if (EsTeclaSangrado(tecla))
        {
            return MODO_SANGRADO;
        }
        else
        {
            return -1;
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // ══════════════════════════════════════════════════════════════════════
    //  EJERCICIO 2 — ¿Qué color según el modo?
    // ══════════════════════════════════════════════════════════════════════
    //
    //  CONTEXTO: Cada modo del Rapier tiene un color:
    //   - Normal → Blanco (color base)
    //   - Veneno → Morado
    //   - Sangrado → Rojo
    //
    //  OBJETIVO: Recibe un modo y devuelve el color correspondiente:
    //    Modo 0 (Normal)   → 0 (Blanco)
    //    Modo 1 (Veneno)   → 1 (Morado)
    //    Modo 2 (Sangrado) → 2 (Rojo)
    //
    //  REGLAS DE ORO:
    //  ✅ Usa: switch/case
    //  ❌ No uses: nada de Unity
    //
    // ══════════════════════════════════════════════════════════════════════

    /// <param name="modo">Modo del Rapier (0=Normal, 1=Veneno, 2=Sangrado).</param>
    /// <returns>Color correspondiente (0=Blanco, 1=Morado, 2=Rojo)</returns>
    int ColorSegunModo(int modo)
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼  ESCRIBE TU CÓDIGO AQUÍ  ▼▼▼▼▼▼▼▼▼▼▼▼

        switch (modo)
        {
            case MODO_NORMAL:
                return COLOR_BLANCO;

            case MODO_VENENO:
                return COLOR_MORADO;

            case MODO_SANGRADO:
                return COLOR_ROJO;

            default:
                return COLOR_BLANCO;
        }

        // ▲▲▲▲▲▲▲▲▲▲▲▲  FIN DE TU CÓDIGO  ▲▲▲▲▲▲▲▲▲▲▲▲
    }


    // =====================================================================
    // BLOQUE 3 — TRABAJO SUCIO UNITY/C# (alumno NO toca)
    // =====================================================================

    void DetectarCambioDeModo()
    {
        bool pulsaNormal   = Input.GetKeyDown(teclaNormal);
        bool pulsaVeneno   = Input.GetKeyDown(teclaVeneno);
        bool pulsaSangrado = Input.GetKeyDown(teclaSangrado);

        if (pulsaNormal)
        {
            int modo = DetectarCambioDeModo(teclaNormal);
            EstablecerModo(modo);
        }
        else if (pulsaVeneno)
        {
            int modo = DetectarCambioDeModo(teclaVeneno);
            EstablecerModo(modo);
        }
        else if (pulsaSangrado)
        {
            int modo = DetectarCambioDeModo(teclaSangrado);
            EstablecerModo(modo);
        }
    }

    public void EstablecerModo(int tipo)
    {
        currentDamageType = ConstanteATipo(tipo);
        AplicarColorActual();
        NotificarHitbox();
        MostrarMensajeDeModo();

        debugEJ_modoElegido = tipo;
    }

    void AplicarColorActual()
    {
        if (instancedMaterial == null) return;

        int modo = TipoAConstante(currentDamageType);
        int colorElegido = ColorSegunModo(modo);

        debugEJ_colorCalculado = colorElegido;

        Color c;
        switch (colorElegido)
        {
            case COLOR_MORADO:
                c = new Color(0.80f, 0.60f, 1.00f, 1f);
                break;
            case COLOR_ROJO:
                c = new Color(1.00f, 0.55f, 0.55f, 1f);
                break;
            default:
                c = new Color(0.78f, 0.78f, 0.78f, 1f);
                break;
        }

        if (instancedMaterial.HasProperty("_BaseColor"))
            instancedMaterial.SetColor("_BaseColor", c);
        else if (instancedMaterial.HasProperty("_Color"))
            instancedMaterial.SetColor("_Color", c);
    }

    void NotificarHitbox()
    {
        if (hitbox != null)
        {
            hitbox.damageType = currentDamageType;
            Debug.Log("CAMBIE DE TIPO");
        }
    }

    void ObtenerReferenciaDelRenderer()
    {
        rend = GetComponent<Renderer>();
    }

    void CrearMaterialInstanciado()
    {
        if (rend != null)
            instancedMaterial = rend.material;
    }

    void MostrarMensajeDeModo()
    {
        Debug.Log("[Rapier] Modo activo: " + currentDamageType);
    }
}