using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [System.Serializable]
    public struct EnemyStruct
    {
        public EnemyScript enemyScript;
        public bool enemyAvailability;
    }

    public List<EnemyStruct> allEnemies = new List<EnemyStruct>();
    private List<int> enemyIndexes;

    [Header("Main AI Loop - Settings")]
    private Coroutine AI_Loop_Coroutine;

    [Header("Rondas")]
    public int totalRounds = 3;
    [SerializeField] private int currentRound = 1;

    public int aliveEnemyCount;

    // ══════════════════════════════════════════════════════════
    // START
    // ══════════════════════════════════════════════════════════

    void Start()
    {
        Debug.Log($"[EnemyManager] '{name}' Start() llamado.");

        RegisterAllEnemiesInChildren();

        Debug.Log($"[EnemyManager] '{name}' tiene {allEnemies.Count} enemigos registrados.");

        StartAI();
    }

    // ══════════════════════════════════════════════════════════
    // REGISTRATION
    // ══════════════════════════════════════════════════════════

    public void RegisterAllEnemiesInChildren()
    {
        allEnemies.Clear();

        // Find ALL EnemyScript in children (including inactive)
        EnemyScript[] found = GetComponentsInChildren<EnemyScript>(true);

        Debug.Log($"[EnemyManager] '{name}' encontró {found.Length} EnemyScript en hijos.");

        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] == null)
            {
                Debug.LogWarning($"[EnemyManager] '{name}' encontrado EnemyScript NULL en índice {i}.");
                continue;
            }

            EnemyStruct entry;
            entry.enemyScript = found[i];
            entry.enemyAvailability = true;
            allEnemies.Add(entry);

            Debug.Log($"[EnemyManager] '{name}' registrado: '{found[i].name}' (índice {i})");
        }

        Debug.Log($"[EnemyManager] '{name}' total registrados: {allEnemies.Count}");
    }

    // ══════════════════════════════════════════════════════════
    // AI LOOP
    // ══════════════════════════════════════════════════════════

    public void StartAI()
    {
        StopAllCoroutines();
        AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
    }

    public void StopAI()
    {
        if (AI_Loop_Coroutine != null)
        {
            StopCoroutine(AI_Loop_Coroutine);
            AI_Loop_Coroutine = null;
        }
    }

    IEnumerator AI_Loop(EnemyScript lastAttacker)
    {
        Debug.Log($"[EnemyManager] '{name}' AI_Loop iniciado con {AliveEnemyCount()} enemigos vivos.");

        if (AliveEnemyCount() == 0)
        {
            Debug.LogWarning($"[EnemyManager] '{name}' AI_Loop: NO HAY ENEMIGOS VIVOS. Saliendo.");
            AI_Loop_Coroutine = null;
            yield break;
        }

        while (true)
        {
            int alive = AliveEnemyCount();
            Debug.Log($"[EnemyManager] '{name}' AI_Loop tick — {alive} enemigos vivos.");

            if (alive == 0)
            {
                Debug.Log($"[EnemyManager] '{name}' todos muertos. AI_Loop termina.");
                AI_Loop_Coroutine = null;
                yield break;
            }

            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

            EnemyScript attacker = RandomEnemyExcludingOne(lastAttacker);
            if (attacker == null)
                attacker = RandomEnemy();

            if (attacker == null)
            {
                Debug.LogWarning($"[EnemyManager] '{name}' no encontró atacante. Reintentando...");
                yield return new WaitForSeconds(1f);
                continue;
            }

            Debug.Log($"[EnemyManager] '{name}' → '{attacker.name}' seleccionado para atacar.");

            yield return new WaitUntil(() => !attacker.IsRetreating());
            yield return new WaitUntil(() => !attacker.IsLockedTarget());
            yield return new WaitUntil(() => !attacker.IsStunned());

            attacker.SetAttack();
            yield return new WaitUntil(() => !attacker.IsPreparingAttack());
            attacker.SetRetreat();

            yield return new WaitForSeconds(Random.Range(0f, 0.5f));
            lastAttacker = attacker;
        }
    }

    // ══════════════════════════════════════════════════════════
    // QUERIES
    // ══════════════════════════════════════════════════════════

    public EnemyScript RandomEnemy()
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyAvailability
                && allEnemies[i].enemyScript != null
                && allEnemies[i].enemyScript.isActiveAndEnabled)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
        {
            Debug.LogWarning($"[EnemyManager] '{name}' RandomEnemy: no hay enemigos disponibles.");
            return null;
        }

        int randomIndex = Random.Range(0, enemyIndexes.Count);
        return allEnemies[enemyIndexes[randomIndex]].enemyScript;
    }

    public EnemyScript RandomEnemyExcludingOne(EnemyScript exclude)
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyAvailability
                && allEnemies[i].enemyScript != null
                && allEnemies[i].enemyScript.isActiveAndEnabled
                && allEnemies[i].enemyScript != exclude)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        int randomIndex = Random.Range(0, enemyIndexes.Count);
        return allEnemies[enemyIndexes[randomIndex]].enemyScript;
    }

    public int AliveEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyScript != null
                && allEnemies[i].enemyScript.isActiveAndEnabled)
                count++;
        }
        aliveEnemyCount = count;
        return count;
    }

    public int AvailableEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyAvailability
                && allEnemies[i].enemyScript != null
                && allEnemies[i].enemyScript.isActiveAndEnabled)
                count++;
        }
        return count;
    }

    public bool AnEnemyIsPreparingAttack()
    {
        foreach (EnemyStruct es in allEnemies)
        {
            if (es.enemyScript != null
                && es.enemyScript.isActiveAndEnabled
                && es.enemyAvailability
                && es.enemyScript.IsPreparingAttack())
                return true;
        }
        return false;
    }

    // ══════════════════════════════════════════════════════════
    // MUTATION
    // ══════════════════════════════════════════════════════════

    public void SetEnemyAvailability(EnemyScript enemy, bool state)
    {
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyScript == enemy)
            {
                EnemyStruct entry = allEnemies[i];
                entry.enemyAvailability = state;
                allEnemies[i] = entry;
                break;
            }
        }

        EnemyDetection det = FindAnyObjectByType<EnemyDetection>();
        if (det != null && det.CurrentTarget() == enemy)
            det.SetCurrentTarget(null);
    }

    public void RemoveEnemy(EnemyScript enemy)
    {
        for (int i = allEnemies.Count - 1; i >= 0; i--)
        {
            if (allEnemies[i].enemyScript == enemy)
            {
                allEnemies.RemoveAt(i);
                Debug.Log($"[EnemyManager] '{name}' removió '{enemy.name}'. Quedan {allEnemies.Count}.");
                break;
            }
        }
    }

    // ══════════════════════════════════════════════════════════
    // ROUNDS API
    // ══════════════════════════════════════════════════════════

    public int GetTotalRounds() => totalRounds;
    public int GetCurrentRound() => currentRound;
    public void SetCurrentRound(int round) => currentRound = round;
}