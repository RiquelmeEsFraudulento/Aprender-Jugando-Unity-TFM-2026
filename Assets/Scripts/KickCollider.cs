using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class KickHitbox : MonoBehaviour
{
    [Header("Damage settings")]
    public int damage = 1;

    [Tooltip("Root transform of the character that owns this hitbox (player).")]
    public Transform ownerRoot;

    [Tooltip("Color damage type (Red/Blue/Gray).")]
    public DamageType damageType = DamageType.Red;

    // One hit per enemy per swing
    readonly HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

    // ── Referencia al jugador para leer el estado seleccionado ─
    private SimpleWalk playerCombat;

    void Awake()
    {
        if (playerCombat == null)
            playerCombat = FindAnyObjectByType<SimpleWalk>();
    }

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

        // Prevent double hits on same enemy during this swing
        if (!alreadyHit.Add(damageable))
            return;

        // Check if this enemy accepts THIS color + THIS weapon tag
        string weaponTag = gameObject.tag;
        if (!damageable.CanBeDamagedBy(damageType, weaponTag))
            return;

        // Finally apply damage
        GameObject source = ownerRoot != null ? ownerRoot.gameObject : gameObject;
        damageable.TakeDamage(damage, damageType, source);

        // ── Acumular golpe para estado especial ──────────────
        ProcesarEstadoEspecial(damageable);
    }

    // ══════════════════════════════════════════════════════════
    // ESTADO ESPECIAL — Acumular golpes y aplicar si llega a 8
    // ══════════════════════════════════════════════════════════

    void ProcesarEstadoEspecial(Damageable damageable)
    {
        // Leer el estado seleccionado por el jugador
        if (playerCombat == null)
            playerCombat = FindAnyObjectByType<SimpleWalk>();

        Damageable.EstadoEspecial estadoSeleccionado = playerCombat.GetEstadoSeleccionado();

        // Si no hay estado seleccionado, no hacer nada
        if (estadoSeleccionado == Damageable.EstadoEspecial.None)
            return;

        // Si el enemigo ya está en un estado especial, no acumular
        if (damageable.GetEstadoEspecial() != Damageable.EstadoEspecial.None)
            return;

        // Registrar golpe
        bool alcanzoUmbral = damageable.RegistrarGolpeParaEstado();

        if (alcanzoUmbral)
        {
            // Aplicar el estado correspondiente
            switch (estadoSeleccionado)
            {
                case Damageable.EstadoEspecial.Sleep:
                    if (damageable.AplicarSleep())
                    {
                        // Notificar al EnemyScript para que active la animación
                        EnemyScript enemy = damageable.GetComponent<EnemyScript>();
                        if (enemy != null)
                            enemy.ActivarSleep();
                        Debug.Log($"[KickHitbox] SLEEP aplicado a '{damageable.name}' tras 8 golpes.");
                    }
                    break;

                case Damageable.EstadoEspecial.Confused:
                    if (damageable.AplicarConfused())
                    {
                        EnemyScript enemy = damageable.GetComponent<EnemyScript>();
                        if (enemy != null)
                            enemy.ActivarConfused();
                        Debug.Log($"[KickHitbox] CONFUSED aplicado a '{damageable.name}' tras 8 golpes.");
                    }
                    break;
            }
        }
    }

    public void Reactivar()
    {
        alreadyHit.Clear();
    }
}