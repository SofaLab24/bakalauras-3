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

    private int enemiesToGenerate;
    public int totalEnemies;
    // public for debugging
    Stack<(int health, float moveSpeed)> enemyPool;
    private EnemyGenerator enemyGenerator;
    private List<Path> currentPaths;

    PathGenerator pathGenerator;
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
        totalEnemies = enemiesToGenerate;

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
                enemyAmountPerPath = Random.Range(1, enemiesToGenerate - currentPaths.Count + i);
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

        Debug.Log("Total enemies: " + newEnemies);
        Debug.Log("Total stat points: " + totalStatPoints);
        Debug.Log("Total paths: " + currentPaths.Count);

        while (totalStatPoints > newEnemies)
        {
            if (newEnemies <= 1)
            {
                Debug.Log("Last enemy. Stat points left: " + totalStatPoints);
                (int health, float moveSpeed) lastEnemy = enemyGenerator.GenerateEnemy(totalStatPoints);
                enemyPool.Push(lastEnemy);
                newEnemies--;
                break;
            }

            int statPoints = Random.Range(1, totalStatPoints);
            (int health, float moveSpeed) enemy = enemyGenerator.GenerateEnemy(statPoints);
            enemyPool.Push(enemy);
            totalStatPoints -= statPoints;
            newEnemies--;
            Debug.Log("Stat points left: " + totalStatPoints);
            Debug.Log("Enemies left: " + newEnemies);
        }

        if (newEnemies > 0)
        {
            Debug.Log("No stats left. Enemies left: " + newEnemies);
            for (int i = 0; i < newEnemies; i++)
            {
                (int health, float moveSpeed) enemy = enemyGenerator.GenerateEnemy(1);
                enemyPool.Push(enemy);
            }
        }
        Debug.Log("Finished generating enemies");
    }
    private IEnumerator SpawnEnemies(List<Vector2> enemyTargets, int enemyAmountPerPath, Vector3 initPosition)
    {
        for (int j = 0; j < enemyAmountPerPath; j++)
        {
            Debug.Log("Spawning enemy " + j);
            (int health, float moveSpeed) enemyStats = enemyPool.Pop();
            EnemyController enemy = Instantiate(enemyPrefab, initPosition, Quaternion.identity);
            enemy.SetTargets(enemyTargets);
            enemy.SetMoveSpeed(enemyStats.moveSpeed);
            enemy.SetHealth(enemyStats.health);
            yield return new WaitForSeconds(enemySpawnInterval + Random.Range(-enemySpawnIntervalVariance, enemySpawnIntervalVariance));
        }
    }
}
