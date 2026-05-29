using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    public EnemyManager enemyPrefab;

    [Header("Posición relativa al spawn point")]
    public Vector3 spawnOffset = Vector3.zero;

    [Header("¿Respawnear en cada ronda?")]
    public bool respawnOnEachRound = true;

    private EnemyManager currentManager;

    void Reset()
    {
        gameObject.tag = "Respawn";
    }

    public EnemyManager Spawn()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"[EnemySpawner] '{name}' NO TIENE enemyPrefab ASIGNADO!");
            return null;
        }

        // Check if we already have a live manager
        if (currentManager != null)
        {
            if (currentManager.aliveEnemyCount > 0)
            {
                Debug.Log($"[EnemySpawner] '{name}' ya tiene EnemyManager vivo con {currentManager.aliveEnemyCount} enemigos.");
                return currentManager;
            }
            else
            {
                Debug.Log($"[EnemySpawner] '{name}' manager anterior muerto, respawneando...");
                Destroy(currentManager.gameObject);
                currentManager = null;
            }
        }

        Vector3 pos = transform.position + spawnOffset;
        Quaternion rot = transform.rotation;

        // Spawn the EnemyManager prefab
        currentManager = Instantiate(enemyPrefab, pos, rot, null);

        if (currentManager == null)
        {
            Debug.LogError($"[EnemySpawner] '{name}' Instantiate devolvió NULL!");
            return null;
        }

        Debug.Log($"[EnemySpawner] '{name}' spawned EnemyManager '{currentManager.name}' at {pos}.");

        // Force the manager to register its children immediately
        currentManager.RegisterAllEnemiesInChildren();

        Debug.Log($"[EnemySpawner] '{name}' manager tiene {currentManager.allEnemies.Count} enemigos registrados.");

        return currentManager;
    }

    public EnemyManager GetSpawnedManager()
    {
        return currentManager;
    }

    public void ClearCurrentManager()
    {
        if (currentManager != null)
        {
            Destroy(currentManager.gameObject);
            currentManager = null;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + spawnOffset, 0.5f);
        Gizmos.DrawRay(transform.position + spawnOffset, transform.forward * 2f);
    }
}