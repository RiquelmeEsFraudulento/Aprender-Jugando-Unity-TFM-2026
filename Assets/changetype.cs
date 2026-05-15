using UnityEngine;

// ============================================================
// RapierColorController.cs
// ============================================================
// DÓNDE COLOCARLO:
//   En el GameObject de la HOJA del Rapier (el que tiene el
//   Renderer / material visible). NO en la hitbox.
//
// QUÉ HACE:
//   - Lee las teclas 4 / 5 / 6 para cambiar el modo del Rapier.
//   - Cambia el color del material instanciado (solo esta hoja).
//   - Notifica al RapierHitbox (hijo) cuál es el DamageType activo.
//
// SEPARACIÓN DE RESPONSABILIDADES (igual que el LightSaber):
//   RapierColorController  → solo se preocupa del MATERIAL y del INPUT.
//   RapierHitbox           → solo se preocupa de DETECTAR GOLPES.
//   Ambos hablan a través del campo damageType de RapierHitbox.
//
// NOTA PARA ALUMNO DE C++:
//   - [RequireComponent] = assert en tiempo de diseño; garantiza
//     que este GameObject tenga un Renderer.
//   - rend.material crea una copia única del material para esta
//     instancia → en C++: mat = new Material(*shared);
//   - GetComponentInChildren<T>() busca T en los hijos del árbol.
// ============================================================

[RequireComponent(typeof(Renderer))]
public class RapierColorController : MonoBehaviour
{
    // ── Teclas configurables desde el Inspector ──────────────
    [Header("Teclas de modo")]
    public KeyCode teclaNormal   = KeyCode.Alpha4;  // 4 → Normal
    public KeyCode teclaVeneno   = KeyCode.Alpha5;  // 5 → Veneno
    public KeyCode teclaSangrado = KeyCode.Alpha6;  // 6 → Sangrado

    // ── Estado visible en el Inspector durante el juego ──────
    [Header("Estado actual (lectura en tiempo real)")]
    public DamageType currentDamageType = DamageType.Normal;

    // ── Referencias internas ─────────────────────────────────
    // El alumno no necesita tocar estas variables.
    Renderer    rend;
    public Material    instancedMaterial;  // copia única para esta hoja
    [SerializeField] private RapierHitbox hitbox; // ← Arrastra aquí el GameObject con WeaponHitbox

    

    // ── Colores de cada modo ─────────────────────────────────
    // Modifica los valores RGBA para ajustar la intensidad del tinte.
    // El cuarto valor (alpha) no cambia en materiales opacos;
    // los tres primeros son R, G, B en rango [0..1].
    static readonly Color colorNormal   = new Color(0.78f, 0.78f, 0.78f); // acero claro
    static readonly Color colorVeneno   = new Color(0.72f, 0.52f, 0.95f); // morado suave
    static readonly Color colorSangrado = new Color(0.95f, 0.42f, 0.42f); // rojo suave

    // ────────────────────────────────────────────────────────
    // Awake — equivale al constructor en C++.
    // Se ejecuta ANTES que Start(), lo que garantiza que las
    // referencias estén listas cuando otros scripts las busquen.
    // ────────────────────────────────────────────────────────
    void Awake()
    {
        ObtenerReferenciaDelRenderer();
        CrearMaterialInstanciado();
        AplicarColorActual();
    }

    // ────────────────────────────────────────────────────────
    // Update — detecta input y cambia modo si corresponde.
    // En C++: void update() { if (input.justPressed(...)) ... }
    // ────────────────────────────────────────────────────────
    void Update()
    {
        DetectarCambioDeModo();
    }

    // ============================================================
    // BLOQUE A — INPUT
    // Funciones que encapsulan Input.GetKeyDown.
    // El alumno ve nombres claros en vez de código de Unity.
    // ============================================================

    bool PulsaNormal()   { return Input.GetKeyDown(teclaNormal); }
    bool PulsaVeneno()   { return Input.GetKeyDown(teclaVeneno); }
    bool PulsaSangrado() { return Input.GetKeyDown(teclaSangrado); }

    void DetectarCambioDeModo()
    {
        if (PulsaNormal())   EstablecerModo(DamageType.Normal);
        if (PulsaVeneno())   EstablecerModo(DamageType.Poison);
        if (PulsaSangrado()) EstablecerModo(DamageType.Bleed);
    }

    // ============================================================
    // BLOQUE B — CAMBIO DE MODO
    // Punto de entrada público: cualquier otro script puede llamar
    // a EstablecerModo(...) para forzar un cambio desde fuera.
    // ============================================================

    public void EstablecerModo(DamageType tipo)
    {
        currentDamageType = tipo;
        AplicarColorActual();
        NotificarHitbox();
        MostrarMensajeDeModo();
    }

    // ============================================================
    // BLOQUE C — COLOR
    // Elige el color según el modo y lo aplica al material.
    // El switch aquí es SOLO visual; la lógica de daño está
    // en RapierHitbox + Damageable.
    // ============================================================

    void AplicarColorActual()
    {
        if (instancedMaterial == null) return;

        Color colorElegido = ElegirColorSegunModo(currentDamageType);
        AsignarColorAlMaterial(colorElegido);
    }

    Color ElegirColorSegunModo(DamageType tipo)
    {
        switch (tipo)
        {
            case DamageType.Normal:  return colorNormal;
            case DamageType.Poison:  return colorVeneno;
            case DamageType.Bleed:   return colorSangrado;
            default:                 return colorNormal;
        }
    }

    // Funciona con URP (_BaseColor) y con el shader Standard (_Color).
    // En C++: material->setColor(color);
    void AsignarColorAlMaterial(Color color)
    {
        if (instancedMaterial.HasProperty("_BaseColor"))
            instancedMaterial.SetColor("_BaseColor", color);
        else if (instancedMaterial.HasProperty("_Color"))
            instancedMaterial.SetColor("_Color", color);
    }

    // ============================================================
    // BLOQUE D — SINCRONIZACIÓN CON LA HITBOX
    // El RapierHitbox (hijo) necesita saber el modo activo para
    // que cuando golpee, aplique el efecto correcto.
    // ============================================================

    void NotificarHitbox()
    {
        if (hitbox != null){
            hitbox.damageType = currentDamageType;
            Debug.Log("CAMBIE DE TIPO");

        }
            
    }

    // ============================================================
    // BLOQUE E — INICIALIZACIÓN INTERNA
    // Todo lo "raro" de Unity queda aquí. El alumno no necesita
    // entender estas funciones para completar los ejercicios.
    // ============================================================

    void ObtenerReferenciaDelRenderer()
    {
        rend = GetComponent<Renderer>();
    }

    void CrearMaterialInstanciado()
    {
        // rend.material crea automáticamente una copia del material
        // compartido y la asigna SOLO a este objeto.
        // Usar rend.sharedMaterial cambiaría el color de TODOS los
        // objetos que usen ese mismo material.
        if (rend != null)
            instancedMaterial = rend.material;
    }


    void MostrarMensajeDeModo()
    {
        Debug.Log("[Rapier] Modo activo: " + currentDamageType);
    }
}