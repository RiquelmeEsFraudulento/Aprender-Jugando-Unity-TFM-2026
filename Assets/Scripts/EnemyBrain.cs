using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private EnemyScript[] enemies;
public List<EnemyStruct> allEnemies = new List<EnemyStruct>();
    private List<int> enemyIndexes;

    [Header("Main AI Loop - Settings")]
    private Coroutine AI_Loop_Coroutine;

    public int aliveEnemyCount;
    void Start()
    {
        enemies = GetComponentsInChildren<EnemyScript>();

        allEnemies.Clear();
        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyStruct entry = new EnemyStruct();
            entry.enemyScript = enemies[i];
            entry.enemyAvailability = true;
            allEnemies.Add(entry);
        }

        StartAI();
    }

    public void StartAI()
    {
        AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
    }

    IEnumerator AI_Loop(EnemyScript enemy)
    {
        if (AliveEnemyCount() == 0)
        {
            StopCoroutine(AI_Loop(null));
            yield break;
        }

        yield return new WaitForSeconds(Random.Range(.5f,1.5f));

        EnemyScript attackingEnemy = RandomEnemyExcludingOne(enemy);

        if (attackingEnemy == null)
            attackingEnemy = RandomEnemy();

        if (attackingEnemy == null)
            yield break;
            
        yield return new WaitUntil(()=>attackingEnemy.IsRetreating() == false);
        yield return new WaitUntil(() => attackingEnemy.IsLockedTarget() == false);
        yield return new WaitUntil(() => attackingEnemy.IsStunned() == false);

        attackingEnemy.SetAttack();

        yield return new WaitUntil(() => attackingEnemy.IsPreparingAttack() == false);

        attackingEnemy.SetRetreat();

        yield return new WaitForSeconds(Random.Range(0,.5f));

        if (AliveEnemyCount() > 0)
            AI_Loop_Coroutine = StartCoroutine(AI_Loop(attackingEnemy));
    }

    public EnemyScript RandomEnemy()
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyAvailability && allEnemies[i].enemyScript != null && allEnemies[i].enemyScript.isActiveAndEnabled)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        EnemyScript randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = allEnemies[enemyIndexes[randomIndex]].enemyScript;

        return randomEnemy;
    }

    public EnemyScript RandomEnemyExcludingOne(EnemyScript exclude)
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyAvailability && allEnemies[i].enemyScript != null 
            && allEnemies[i].enemyScript.isActiveAndEnabled
            && allEnemies[i].enemyScript != exclude)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        EnemyScript randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = allEnemies[enemyIndexes[randomIndex]].enemyScript;

        return randomEnemy;
    }

    public int AvailableEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyAvailability && allEnemies[i].enemyScript != null && allEnemies[i].enemyScript.isActiveAndEnabled)
                count++;
        }
        return count;
    }

    public bool AnEnemyIsPreparingAttack()
    {
        foreach (EnemyStruct enemyStruct in allEnemies)
        {
            if (enemyStruct.enemyScript != null && enemyStruct.enemyScript.isActiveAndEnabled && enemyStruct.enemyAvailability && enemyStruct.enemyScript.IsPreparingAttack())
            {
                return true;
            }
        }
        return false;
    }


    public int AliveEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Count; i++)
        {
            // Ahora allEnemies[i].enemyScript puede ser null después de morir, así que comprobamos
            if (allEnemies[i].enemyScript != null && allEnemies[i].enemyScript.isActiveAndEnabled)
                count++;
        }
        aliveEnemyCount = count;
        return count;
    }

    public void SetEnemyAvailiability(EnemyScript enemy, bool state)
    {
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyScript == enemy)
            {
                EnemyStruct entry = allEnemies[i];
                entry.enemyAvailability = state;
                allEnemies[i] = entry;
                break; // opcional, pero como cada EnemyScript aparece una vez, podemos salir
            }
        }

        if (FindAnyObjectByType<EnemyDetection>().CurrentTarget() == enemy)
            FindAnyObjectByType<EnemyDetection>().SetCurrentTarget(null);
    }

    public void RemoveEnemy(EnemyScript enemy)
    {
        for (int i = allEnemies.Count - 1; i >= 0; i--)
        {
            if (allEnemies[i].enemyScript == enemy)
            {
                allEnemies.RemoveAt(i);
                break;
            }
        }
    }


        

}

[System.Serializable]
public struct EnemyStruct
{
    public EnemyScript enemyScript;
    public bool enemyAvailability;
}
