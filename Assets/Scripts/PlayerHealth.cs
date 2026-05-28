// ============================================================
// PlayerHealth.cs — MODIFICADO
// ============================================================
// Añade detección de muerte que notifica al GameManager.
// Reemplaza tu PlayerHealth.cs existente con este.
// ============================================================

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida del jugador")]
    public float vida = 10f;
    public float vidaMax = 20f;

    public float lottery;
    public float lootbox;

    public PlayerHUDController hud;

    public float level = 1f;
    public float XP = 0f;

    private bool isDead = false;

    void Start()
    {
        level = 1f;
        XP = 0f;
        lootbox = 0f;
        isDead = false;
    }

    void Update()
    {
        vida = Mathf.Clamp(vida, 0f, vidaMax);

        // Detectar muerte
        if (vida <= 0f && !isDead)
        {
            isDead = true;
            OnDeath();
        }
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

        if (XP >= level * 10)
        {
            level += 1f;
            vidaMax += 2f;
            Debug.Log("[Player] Subió al nivel " + level + "! Vida máxima aumentada a " + vidaMax);
        }
    }

    void OnDeath()
    {
        Debug.Log("[Player] ¡El jugador ha muerto!");

        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied();
        }
    }

    public void ResetDeath()
    {
        isDead = false;
    }
}