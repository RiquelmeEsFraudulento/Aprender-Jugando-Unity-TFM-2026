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
    [HideInInspector] public DamageType damageType = DamageType.Red;

    private Renderer hojaRenderer;
    private Material materialInstanciado;

    private static readonly Color tintNormal   = new Color(1.00f, 1.00f, 1.00f, 1f);
    private static readonly Color tintVeneno   = new Color(0.80f, 0.60f, 1.00f, 1f);
    private static readonly Color tintSangrado = new Color(1.00f, 0.55f, 0.55f, 1f);

    readonly HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

    // ── Referencia al jugador para leer el estado seleccionado ─
    private SimpleWalk playerCombat;

    // ── Inicialización ───────────────────────────────────────
    void Awake()
    {
        ObtenerMaterialDeLaHoja();
        AplicarModoNormal();
        if (playerCombat == null)
            playerCombat = FindAnyObjectByType<SimpleWalk>();
    }

    // ── Loop principal ───────────────────────────────────────
    void Update()
    {
        DetectarCambioDeModo();
    }

    void ObtenerMaterialDeLaHoja()
    {
        hojaRenderer = GetComponent<Renderer>();
        if (hojaRenderer != null)
        {
            materialInstanciado = hojaRenderer.material;
        }
    }

    void DetectarCambioDeModo()
    {
        if (Input.GetKeyDown(teclaModoNormal))   AplicarModoNormal();
        if (Input.GetKeyDown(teclaModoVeneno))   AplicarModoVeneno();
        if (Input.GetKeyDown(teclaModoSangrado)) AplicarModoSangrado();
    }

    void AplicarModoNormal()
    {
        damageType = DamageType.Normal;
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
    void OnTriggerEnter(Collider other)
    {
        if (EsGolpePropio(other)) return;

        Damageable damageable = BuscarVidaEnEnemigo(other);
        if (damageable == null) return;

        if (YaFueGolpeadoEnEsteSwing(damageable)) return;

        string etiquetaArma = ObtenerEtiquetaPropia();
        if (!damageable.CanBeDamagedBy(damageType, etiquetaArma)) return;

        GameObject fuente = ownerRoot != null ? ownerRoot.gameObject : gameObject;
        damageable.TakeDamage(damage, damageType, fuente);

        // ── Acumular golpe para estado especial ──────────────
        ProcesarEstadoEspecial(damageable);
    }

    // ══════════════════════════════════════════════════════════
    // ESTADO ESPECIAL — Acumular golpes y aplicar si llega a 8
    // ══════════════════════════════════════════════════════════

    void ProcesarEstadoEspecial(Damageable damageable)
    {
        if (playerCombat == null)
            playerCombat = FindAnyObjectByType<SimpleWalk>();

        Damageable.EstadoEspecial estadoSeleccionado = playerCombat.GetEstadoSeleccionado();

        if (estadoSeleccionado == Damageable.EstadoEspecial.None)
            return;

        if (damageable.GetEstadoEspecial() != Damageable.EstadoEspecial.None)
            return;

        bool alcanzoUmbral = damageable.RegistrarGolpeParaEstado();

        if (alcanzoUmbral)
        {
            switch (estadoSeleccionado)
            {
                case Damageable.EstadoEspecial.Sleep:
                    if (damageable.AplicarSleep())
                    {
                        EnemyScript enemy = damageable.GetComponent<EnemyScript>();
                        if (enemy != null)
                            enemy.ActivarSleep();
                        Debug.Log($"[RapierHitbox] SLEEP aplicado a '{damageable.name}' tras 8 golpes.");
                    }
                    break;

                case Damageable.EstadoEspecial.Confused:
                    if (damageable.AplicarConfused())
                    {
                        EnemyScript enemy = damageable.GetComponent<EnemyScript>();
                        if (enemy != null)
                            enemy.ActivarConfused();
                        Debug.Log($"[RapierHitbox] CONFUSED aplicado a '{damageable.name}' tras 8 golpes.");
                    }
                    break;
            }
        }
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
        return gameObject.tag;
    }
}