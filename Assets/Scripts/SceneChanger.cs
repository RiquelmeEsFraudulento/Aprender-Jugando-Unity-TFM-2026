// ============================================================
// SceneChanger.cs
// ============================================================
// Cambiador de escena por trigger.
// Colócalo en un GameObject con Collider (IsTrigger = true).
// El jugador al entrar cambia de escena.
//
// Setup:
//   1. GameObject con Collider (IsTrigger = true)
//   2. Asignar el nombre de la escena destino
//   3. Asignar el spawn point de la escena destino
// ============================================================

using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    [Header("Escena Destino")]
    [Tooltip("Nombre exacto de la escena en Build Settings.")]
    public string targetSceneName;

    [Header("Spawn Point en la escena destino")]
    [Tooltip("Nombre del GameObject que será el spawn point en la escena destino.")]
    public string spawnPointName = "SpawnPoint";

    [Header("¿Requiere destruir algo primero?")]
    public bool requiresCubeDestroy = false;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        if (requiresCubeDestroy)
        {
            // Para la escena de testing: verificar que el cubo fue destruido
            GameObject cube = GameObject.FindGameObjectWithTag("TestCube");
            if (cube != null)
            {
                Debug.Log("[SceneChanger] ¡Destruye el cubo primero!");
                return;
            }
        }

        hasTriggered = true;
        ChangeScene();
    }

    void ChangeScene()
    {
        if (GameManager.Instance != null)
        {
            // Usar el GameManager para cambiar de escena
            // El GameManager guarda el estado del jugador
            GameManager.Instance.SavePlayerStats();

            // Buscar spawn point en la escena destino
            // (se hará en OnSceneLoaded del GameManager)
            PlayerSpawnPoint.targetSpawnName = spawnPointName;
        }

        Debug.Log($"[SceneChanger] Cambiando a escena: {targetSceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
    }
}

// Helper para comunicar el spawn point entre escenas
public static class PlayerSpawnPoint
{
    public static string targetSpawnName = "SpawnPoint";
}