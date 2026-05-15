// ============================================================
// PlayerHealth.cs
// ============================================================
// Componente mínimo de vida del jugador.
// Añádelo al mismo GameObject que SimpleWalk.
// La Potion lo busca con GetComponentInParent<PlayerHealth>().
//
// Puedes expandirlo con eventos, UI, animaciones de daño, etc.
// ============================================================

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida del jugador")]
    public float vida    = 10f;
    public float vidaMax = 20f;

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

    public void Curar(float cantidad)
    {
        vida += cantidad;
        Debug.Log("[Player] Curación: +" + cantidad + " | Vida: " + vida);
    }
}
