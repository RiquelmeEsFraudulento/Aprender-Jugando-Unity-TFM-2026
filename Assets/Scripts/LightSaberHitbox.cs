using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponHitbox : MonoBehaviour
{
    [Header("Damage settings")]
    public int damage = 1;

    [Tooltip("Root transform of the character that owns this hitbox (player).")]
    public Transform ownerRoot;

    [Tooltip("Color damage type (Red/Blue/Gray).")]
    public DamageType damageType = DamageType.Red;

    // One hit per enemy per swing
    HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

    // ── Referencia al jugador para leer el estado seleccionado ─
    private SimpleWalk playerCombat;

    void Awake()
    {
        if (playerCombat == null)
            playerCombat = FindAnyObjectByType<SimpleWalk>();
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
                        Debug.Log($"[WeaponHitbox] SLEEP aplicado a '{damageable.name}' tras 8 golpes.");
                    }
                    break;

                case Damageable.EstadoEspecial.Confused:
                    if (damageable.AplicarConfused())
                    {
                        EnemyScript enemy = damageable.GetComponent<EnemyScript>();
                        if (enemy != null)
                            enemy.ActivarConfused();
                        Debug.Log($"[WeaponHitbox] CONFUSED aplicado a '{damageable.name}' tras 8 golpes.");
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