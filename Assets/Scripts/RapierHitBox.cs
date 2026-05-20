/*using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RapierHitbox : MonoBehaviour
{
    [Header("Damage settings")]
    public int damage = 1;

    [Tooltip("Root transform of the character that owns this hitbox (player).")]
    public Transform ownerRoot;

    [Tooltip("Color damage type (Red/Blue/Gray).")]
    public DamageType damageType = DamageType.Red;

    // One hit per enemy per swing
    readonly HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

    void OnEnable()
    {
        alreadyHit.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignore self-hits
        if (ownerRoot != null && other.transform.root == ownerRoot)
            return;

        // Find enemy health component
        Damageable damageable = other.GetComponentInParent<Damageable>();
        if (damageable == null)
            return;
        Debug.Log("EO");
        // Prevent double hits on same enemy during this swing
        if (!alreadyHit.Add(damageable))
            return;

        // Check if this enemy accepts THIS color + THIS weapon tag
        string weaponTag = gameObject.tag; // e.g. Kick, Rapier, LightSaber
        if (!damageable.CanBeDamagedBy(damageType, weaponTag))
            return;

        // Finally apply damage
        GameObject source = ownerRoot != null ? ownerRoot.gameObject : gameObject;
        damageable.TakeDamage(damage, damageType, source);
    }
}

*/

// ============================================================
// RapierHitBox.cs
// ============================================================
// Gestiona el hitbox del Rapier y su modo de daño activo.
//
// TECLAS DEL ALUMNO:
//   4  → modo Normal   (color base del material, sin efecto)
//   5  → modo Veneno   (tinte morado, activa veneno al impactar)
//   6  → modo Sangrado (tinte rojo extra, acumula golpes)
//
// NOTA TÉCNICA (para alumno de C++):
// - La clase lee el material del Renderer propio cada vez que
//   cambia el modo, y le aplica un tinte multiplicativo.
// - El campo damageType le dice al Damageable qué efecto aplicar.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RapierHitbox : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────
    [Header("Daño base")]
    public int damage = 1;

    [Tooltip("Transform raíz del personaje que posee este hitbox.")]
    public Transform ownerRoot;

    [Header("Teclas de modo del Rapier")]
    public KeyCode teclaModoNormal   = KeyCode.Alpha4;
    public KeyCode teclaModoVeneno   = KeyCode.Alpha5;
    public KeyCode teclaModoSangrado = KeyCode.Alpha6;

    // ── Estado interno ───────────────────────────────────────
    // El tipo de daño activo se lee desde fuera (p.ej. Damageable).
    [HideInInspector] public DamageType damageType = DamageType.Red;

    // Referencia al material de la hoja para poder tintar el color.
    private Renderer hojaRenderer;
    private Material materialInstanciado;

    // Colores de tinte que se mezclan sobre el material original.
    // Ajusta los valores RGBA para que el tinte sea sutil.
    private static readonly Color tintNormal   = new Color(1.00f, 1.00f, 1.00f, 1f); // sin cambio
    private static readonly Color tintVeneno   = new Color(0.80f, 0.60f, 1.00f, 1f); // morado suave
    private static readonly Color tintSangrado = new Color(1.00f, 0.55f, 0.55f, 1f); // rojo suave

    // Registro para evitar doble-hit en el mismo swing.
    readonly HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

    // ── Inicialización ───────────────────────────────────────
    void Awake()
    {
        ObtenerMaterialDeLaHoja();
        AplicarModoNormal();
    }

    // ── Loop principal ───────────────────────────────────────
    void Update()
    {
        DetectarCambioDeModo();
    }

    // ── Inicialización del material ──────────────────────────
    void ObtenerMaterialDeLaHoja()
    {
        hojaRenderer = GetComponent<Renderer>();

        if (hojaRenderer != null)
        {
            // .material crea una instancia única → no afecta a otros objetos.
            // En C++: material = new Material(*sharedMaterial);
            materialInstanciado = hojaRenderer.material;
        }
    }

    // ── Detección de teclas ──────────────────────────────────
    // En C++: if (input.justPressed(Key::4)) { ... }
    void DetectarCambioDeModo()
    {
        if (Input.GetKeyDown(teclaModoNormal))   AplicarModoNormal();
        if (Input.GetKeyDown(teclaModoVeneno))   AplicarModoVeneno();
        if (Input.GetKeyDown(teclaModoSangrado)) AplicarModoSangrado();
    }

    // ── Acciones de modo ─────────────────────────────────────

    void AplicarModoNormal()
    {
        damageType = DamageType.Normal;      // Daño estándar; Red pasa filtros normales
        AplicarTinteAlMaterial(tintNormal);
        Debug.Log("[Rapier] Modo: Normal");
    }

    void AplicarModoVeneno()
    {
        damageType = DamageType.Poison;
        AplicarTinteAlMaterial(tintVeneno);
        Debug.Log("[Rapier] Modo: Veneno");
    }

    void AplicarModoSangrado()
    {
        damageType = DamageType.Bleed;
        AplicarTinteAlMaterial(tintSangrado);
        Debug.Log("[Rapier] Modo: Sangrado");
    }

    // ── Aplicar tinte al material ────────────────────────────
    // Usa _BaseColor (URP) o _Color (Standard shader).
    void AplicarTinteAlMaterial(Color tinte)
    {
        if (materialInstanciado == null) return;

        if (materialInstanciado.HasProperty("_BaseColor"))
            materialInstanciado.SetColor("_BaseColor", tinte);
        else if (materialInstanciado.HasProperty("_Color"))
            materialInstanciado.SetColor("_Color", tinte);
    }
    public void Reactivar()
    {
        Debug.Log("OnEnable de Rapier ha sido activado");
        alreadyHit.Clear();
    }

    // ── Detección de colisión ────────────────────────────────
    // OnTriggerEnter = "entró algo en la zona de impacto".
    // En C++ equivale a tu callback de colisión del motor.
    void OnTriggerEnter(Collider other)
    {
        if (EsGolpePropio(other)) return;

        Damageable damageable = BuscarVidaEnEnemigo(other);
        if (damageable == null) return;

        if (YaFueGolpeadoEnEsteSwing(damageable)) return;
        
        Debug.Log("Se va al objeto damagable");


        string etiquetaArma = ObtenerEtiquetaPropia();
        if (!damageable.CanBeDamagedBy(damageType, etiquetaArma)) return;

        Debug.Log("Se va al objeto damageable");

        GameObject fuente = ownerRoot != null ? ownerRoot.gameObject : gameObject;
        damageable.TakeDamage(damage, damageType, fuente);
        // Aqui quiero agregar el onPlayerHit del EnemyScript
        Debug.Log("Ha sido dañado");

    }

    // ── Helpers de colisión ──────────────────────────────────

    bool EsGolpePropio(Collider other)
    {
        return ownerRoot != null && other.transform.root == ownerRoot;
    }

    Damageable BuscarVidaEnEnemigo(Collider other)
    {
        return other.GetComponentInParent<Damageable>();
    }

    bool YaFueGolpeadoEnEsteSwing(Damageable damageable)
    {
        return !alreadyHit.Add(damageable);
    }

    string ObtenerEtiquetaPropia()
    {
        return gameObject.tag; // "Rapier"
    }
}