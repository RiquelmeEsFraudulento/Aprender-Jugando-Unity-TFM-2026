// ============================================================
// PlayerHealth.cs
// ============================================================
// Componente mínimo de vida del jugador.
// Añádelo al mismo GameObject que SimpleWalk.
// La Potion lo busca con GetComponentInParent<PlayerHealth>().
//
// Puedes expandirlo con eventos, UI, animaciones de daño, etc.
// ============================================================

using NUnit.Framework;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida del jugador")]
    public float vida    = 10f;
    public float vidaMax = 20f;

    public float lottery;
    public float lootbox;

    public PlayerHUDController hud;

    public float level; // Nivel del jugador, para escalar daño o XP
    public float XP; // Puntos de experiencia del jugador

    void Start()
    {
        level = 1f;
        XP = 0f;
        lootbox = 0f;
    }
    void Update()
    {
        // Clamp: nunca salirse del rango [0, vidaMax].
        // En C++: vida = std::clamp(vida, 0f, vidaMax);
        vida = Mathf.Clamp(vida, 0f, vidaMax);
    }

    public void RecibirDanio(float cantidad)
    {
        vida -= cantidad;
        Debug.Log("[Player] Daño recibido: -" + cantidad + " | Vida: " + vida);
    }

    public void Lootbox()
    {
        hud.GrantLoot();
        Debug.Log("[Player] Ha recibido una lootbox!");
    }

    public void Curar(float cantidad)
    {
        vida += cantidad;
        Debug.Log("[Player] Curación: +" + cantidad + " | Vida: " + vida);
    }

    public void TakeDamage(float cantidad)
    {
        vida -= cantidad;
        Debug.Log("[Player] Daño recibido: -" + cantidad + " | Vida: " + vida);
    }
    public void GanarXP(float cantidad)
    {
        XP += cantidad;
        Debug.Log("[Player] XP ganado: +" + cantidad + " | XP total: " + XP);
        // Aquí podrías añadir lógica para subir de nivel, mejorar stats, etc.
        if (XP >= level * 10) // Ejemplo: necesitas 10 XP para el nivel 1, 20 XP para el nivel 2, etc.
        {
            level += 1f;
            vidaMax += 2f; // Ejemplo: cada nivel aumenta la vida máxima
            Debug.Log("[Player] Subió al nivel " + level + "! Vida máxima aumentada a " + vidaMax);
        }
    }
}
