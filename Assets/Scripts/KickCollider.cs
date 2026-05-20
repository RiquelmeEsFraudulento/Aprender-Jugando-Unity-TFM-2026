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

    public void Reactivar(){
        alreadyHit.Clear();
    }

}