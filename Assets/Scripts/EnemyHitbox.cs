// ============================================================
// EnemyHitbox.cs  —  COLLIDER DEL PUÑO DEL ENEMIGO
// ============================================================
// Pon este script en el GameObject HIJO que tiene el Collider
// Trigger del puño/arma del enemigo.
//
// JERARQUÍA NECESARIA EN EL PREFAB:
//   EnemyPrefab (Root)  ← tiene EnemyAI.cs
//   └─ PunioHitbox      ← tiene este script + Collider (Is Trigger = true)
//
// CÓMO FUNCIONA:
//   1. La animación de ataque activa/desactiva el Collider del puño
//      (en el Animator, en el frame del golpe: Enable/Disable Collider)
//   2. Cuando el Collider toca al ninja, este script llama a
//      EnemyAI.GolpearNinja() para aplicar el daño.
// ============================================================
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    // ── Referencia al EnemyAI padre ───────────────────────────
    // Se rellena automáticamente en Awake buscando en el padre.
    private EnemyScript _enemyAI;

    // ── Debug switch ──────────────────────────────────────────
    private const bool LOG_HITBOX = true;

    void Awake()
    {
        // Buscamos el EnemyAI en el GameObject padre (el root del prefab)
        _enemyAI = GetComponentInParent<EnemyScript>();

        if (_enemyAI == null)
            Debug.LogWarning($"[EnemyHitbox] '{name}' no encontró EnemyScript en el padre. " +
                              "Comprueba la jerarquía del prefab.");
    }

    // Se llama cuando el Collider Trigger toca otro Collider
    // Equivale a ComprobarColision() en C++ básico
    void OnTriggerEnter(Collider otro)
    {
        // ¿Es el ninja? (tiene el Tag "Player")
        if (!otro.CompareTag("Player")) return;

        // Buscamos el componente de vida del ninja
        PlayerHealth vidaNinja = otro.GetComponent<PlayerHealth>();
        if (vidaNinja == null) return;

        DebugHitbox($"[Hitbox] '{name}' tocó al ninja");

        // Delegamos el daño al EnemyAI para que aplique la lógica correcta
        if (_enemyAI != null)
            _enemyAI.GolpearNinja(vidaNinja);
    }

    // ── Debug ─────────────────────────────────────────────────
    void DebugHitbox(string msg) { if (LOG_HITBOX) Debug.Log(msg); }
}