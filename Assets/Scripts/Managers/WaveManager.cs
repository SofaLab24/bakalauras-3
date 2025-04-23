using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] EnemyController enemyPrefab;
    [SerializeField] float enemyStatMultiplier = 5.3f;
    [SerializeField] float enemyAmountMultiplier = 1.5f;
    [SerializeField] float enemySpawnInterval = 1.5f;
    [SerializeField] float enemySpawnIntervalVariance = 0.5f;
    public int waveNumber = 0;

    public static event Action<int> OnWaveCompleted;

    private int enemiesToGenerate;
    public int enemiesLeftToDie;
    private Stack<(int health, float moveSpeed)> enemyPool;
    private EnemyGenerator enemyGenerator;
    private List<Path> currentPaths;

    PathGenerator pathGenerator;

    void OnEnable()
    {
        EnemyHealthManager.OnEnemyDeath += HandleEnemyCounter;
    }

    void OnDisable()
    {
        EnemyHealthManager.OnEnemyDeath -= HandleEnemyCounter;
    }

    private void Start()
    {
        pathGenerator = GetComponent<PathGenerator>();
        enemyGenerator = GetComponent<EnemyGenerator>();
        waveNumber = 0;
    }
    public void StartNextWave(float splitChance)
    {
        waveNumber++;
        currentPaths = pathGenerator.GenerateNextPaths(splitChance);
        // this generates all enemies for all paths
        GenerateEnemyPool();
        enemiesLeftToDie = enemiesToGenerate;

        for (int i = 0; i < currentPaths.Count; i++)
        {
            int enemyAmountPerPath;

            // if it's the last path, then all enemies are for this path
            if (i == currentPaths.Count - 1)
            {
                enemyAmountPerPath = enemiesToGenerate;
            }
            else // this leaves at least 1 enemy for the last path
            {
                enemyAmountPerPath = UnityEngine.Random.Range(1, enemiesToGenerate - currentPaths.Count + i);
            }
            enemiesToGenerate -= enemyAmountPerPath;
            List<Vector2> enemyTargets = currentPaths[i].enemyWalkPoints;
            Vector3 initPosition = new Vector3(enemyTargets[^1].x, enemyTargets[^1].y);
            StartCoroutine(SpawnEnemies(enemyTargets, enemyAmountPerPath, initPosition));
        }
    }

    private void GenerateEnemyPool()
    {
        int newEnemies = 1 + (int)(enemyAmountMultiplier * waveNumber);
        enemiesToGenerate = newEnemies;
        
        int totalStatPoints = 1 + (int)(enemyStatMultiplier * waveNumber);
        enemyPool = new Stack<(int health, float moveSpeed)>();

        while (totalStatPoints > newEnemies)
        {
            if (newEnemies <= 1)
            {
                (int health, float moveSpeed) lastEnemy = enemyGenerator.GenerateEnemy(totalStatPoints);
                enemyPool.Push(lastEnemy);
                newEnemies--;
                break;
            }

            int statPoints = UnityEngine.Random.Range(1, totalStatPoints);
            (int health, float moveSpeed) enemy = enemyGenerator.GenerateEnemy(statPoints);
            enemyPool.Push(enemy);
            totalStatPoints -= statPoints;
            newEnemies--;
        }

        if (newEnemies > 0)
        {
            for (int i = 0; i < newEnemies; i++)
            {
                (int health, float moveSpeed) enemy = enemyGenerator.GenerateEnemy(1);
                enemyPool.Push(enemy);
            }
        }
    }
    private IEnumerator SpawnEnemies(List<Vector2> enemyTargets, int enemyAmountPerPath, Vector3 initPosition)
    {
        for (int j = 0; j < enemyAmountPerPath; j++)
        {
            (int health, float moveSpeed) enemyStats = enemyPool.Pop();
            EnemyController enemy = Instantiate(enemyPrefab, initPosition, Quaternion.identity);
            enemy.SetTargets(enemyTargets);
            enemy.SetMoveSpeed(enemyStats.moveSpeed);
            enemy.SetHealth(enemyStats.health);
            yield return new WaitForSeconds(enemySpawnInterval + UnityEngine.Random.Range(-enemySpawnIntervalVariance, enemySpawnIntervalVariance));
        }
    }
    private void HandleEnemyCounter(EnemyHealthManager enemy, EnemyHealthManager.DeathReason reason)
    {
        enemiesLeftToDie--;
        Debug.Log("Enemies left: " + enemiesLeftToDie);
        if (enemiesLeftToDie <= 0)
        {
            OnWaveCompleted?.Invoke(waveNumber);
        }
    }
}
