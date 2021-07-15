using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    CombatScript playerCombat;

    private EnemyScript[] enemies;
    public EnemyStruct[] allEnemies;
    private List<int> enemyIndexes;

    Coroutine ScanEnemiesCoroutine;

    void Start()
    {
        playerCombat = FindObjectOfType<CombatScript>();
        enemies = GetComponentsInChildren<EnemyScript>();

        allEnemies = new EnemyStruct[enemies.Length];

        for (int i = 0; i < allEnemies.Length; i++)
        {
            allEnemies[i].enemyScript = enemies[i];
            allEnemies[i].enemyAvailability = true;
        }
    }

    IEnumerator ScanEnemies()
    {
        yield return new WaitForSeconds(1);

        float minDistance = 100;
        int enemyIndex = 0;

        for (int i = 0; i < allEnemies.Length; i++)
        {

        }
    }

    public EnemyScript RandomEnemy()
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyAvailability)
                enemyIndexes.Add(i);
        }

        EnemyScript randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = allEnemies[enemyIndexes[randomIndex]].enemyScript;

        return randomEnemy;
    }

    public int AvailableEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyAvailability)
                count++;
        }
        return count;
    }

    public int AliveEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyScript.isActiveAndEnabled)
                count++;
        }
        return count;
    }

    public void SetEnemyAvailiability (EnemyScript enemy, bool state)
    {
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyScript == enemy)
                allEnemies[i].enemyAvailability = state;
        }

        if (FindObjectOfType<EnemyDetection>().CurrentTarget() == enemy)
            FindObjectOfType<EnemyDetection>().SetCurrentTarget(null);
    }
}


[System.Serializable]
public struct EnemyStruct
{
    public EnemyScript enemyScript;
    public bool enemyAvailability;
}
