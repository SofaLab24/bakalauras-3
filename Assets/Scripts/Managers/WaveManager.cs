using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyTypeDefinition
{
    public string id;
    public EnemyController prefab;
    public int pointCost = 1;
    [Header("Base Stats")]
    public int baseHealth = 100;
    public float baseMoveSpeed = 5f;
    public int baseDamage = 1;
}

public class WaveManager : MonoBehaviour, IRunDataPersistence
{
    [SerializeField] List<EnemyTypeDefinition> enemyTypes;
    [SerializeField] MoabController moabPrefab;
    [SerializeField] int moabWaveInterval = 10;
    [SerializeField] float enemyAmountMultiplier = 1.5f;
    [SerializeField] float enemySpawnInterval = 1.5f;
    [SerializeField] float enemySpawnIntervalVariance = 0.5f;
    [SerializeField] float enemySpawnPathDelayVariance = 0.75f;
    [SerializeField] float statScalePerTenWaves = 1.2f;
    public int waveNumber;

    public static event Action<int> OnWaveCompleted;

    private int enemiesToGenerate;
    public int enemiesLeftToDie;
    private Stack<(EnemyController prefab, int damage, int health, float moveSpeed)> enemyPool;
    private Dictionary<string, EnemyStats> currentStats;

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
        DataPersistenceManager.Instance.LoadRun();
    }
    public void StartNextWave(float splitChance)
    {
        waveNumber++;
        TryScaleStats();
        // Save run (including potentially updated stats) before spawning
        DataPersistenceManager.Instance.SaveRun();
        pathGenerator.GenerateNextPaths(splitChance);
        // this generates all enemies for all paths
        GenerateEnemyPool();
        enemiesLeftToDie = enemiesToGenerate;

        TrySpawnMoabs();

        for (int i = 0; i < pathGenerator.GetPaths.Count; i++)
        {
            int enemyAmountPerPath;

            // if it's the last path, then all enemies are for this path
            if (i == pathGenerator.GetPaths.Count - 1)
            {
                enemyAmountPerPath = enemiesToGenerate;
            }
            else
            {
                int remainingPathsAfter = pathGenerator.GetPaths.Count - 1 - i;
                int maxCanAssign = enemiesToGenerate - remainingPathsAfter;
                enemyAmountPerPath = maxCanAssign >= 1
                    ? UnityEngine.Random.Range(1, maxCanAssign + 1)
                    : 0;
            }
            enemiesToGenerate -= enemyAmountPerPath;
            List<Vector2> enemyTargets = pathGenerator.GetPaths[i].enemyWalkPoints;
            Vector3 initPosition = new Vector3(enemyTargets[^1].x, enemyTargets[^1].y);
            StartCoroutine(SpawnEnemies(enemyTargets, enemyAmountPerPath, initPosition));
        }
    }

    private void GenerateEnemyPool()
    {
        enemiesToGenerate = 1 + (int)(enemyAmountMultiplier * waveNumber);
        int remainingBudget = enemiesToGenerate;

        enemyPool = new Stack<(EnemyController prefab, int damage, int health, float moveSpeed)>();

        EnemyTypeDefinition cheapestType = enemyTypes.OrderBy(t => t.pointCost).First();

        for (int i = 0; i < enemiesToGenerate; i++)
        {
            List<EnemyTypeDefinition> eligible = enemyTypes.Where(t => t.pointCost <= remainingBudget).ToList();
            EnemyTypeDefinition chosenType = eligible.Count > 0
                ? eligible[UnityEngine.Random.Range(0, eligible.Count)]
                : cheapestType;

            EnemyStats stats = currentStats.TryGetValue(chosenType.id, out EnemyStats saved)
                ? saved
                : new EnemyStats { id = chosenType.id, health = chosenType.baseHealth, moveSpeed = chosenType.baseMoveSpeed, damage = chosenType.baseDamage };

            enemyPool.Push((chosenType.prefab, stats.damage, stats.health, stats.moveSpeed));
            remainingBudget -= chosenType.pointCost;
        }
    }

    private void TryScaleStats()
    {
        if (waveNumber % 10 != 0) return;
        foreach (EnemyStats stats in currentStats.Values)
        {
            stats.health = Mathf.RoundToInt(stats.health * statScalePerTenWaves);
            stats.damage = Mathf.RoundToInt(stats.damage * statScalePerTenWaves);
            stats.moveSpeed *= statScalePerTenWaves;
        }
    }
    private IEnumerator SpawnEnemies(List<Vector2> enemyTargets, int enemyAmountPerPath, Vector3 initPosition)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, enemySpawnPathDelayVariance));
        for (int j = 0; j < enemyAmountPerPath; j++)
        {
            (EnemyController prefab, int damage, int health, float moveSpeed) enemyStats = enemyPool.Pop();
            EnemyController enemy = Instantiate(enemyStats.prefab, initPosition, Quaternion.identity);
            enemy.Initialize(enemyStats.damage, enemyTargets, enemyStats.moveSpeed, enemyStats.health);
            yield return new WaitForSeconds(enemySpawnInterval + UnityEngine.Random.Range(-enemySpawnIntervalVariance, enemySpawnIntervalVariance));
        }
    }
    private void TrySpawnMoabs()
    {
        if (moabPrefab == null) return;
        if (moabWaveInterval <= 0) return;
        if (waveNumber % moabWaveInterval != 0) return;

        int moabCount = waveNumber / moabWaveInterval;
        enemiesLeftToDie += moabCount;

        List<Vector2> moabTargets = pathGenerator.GetPaths[0].enemyWalkPoints;
        Vector3 initPosition = new Vector3(moabTargets[^1].x, moabTargets[^1].y);
        StartCoroutine(SpawnMoabs(moabTargets, moabCount, initPosition));
    }

    private IEnumerator SpawnMoabs(List<Vector2> moabTargets, int moabCount, Vector3 initPosition)
    {
        for (int i = 0; i < moabCount; i++)
        {
            MoabController moab = Instantiate(moabPrefab, initPosition, Quaternion.identity);
            moab.Initialize(0, moabTargets, 0f, 0);
            yield return new WaitForSeconds(enemySpawnInterval + UnityEngine.Random.Range(-enemySpawnIntervalVariance, enemySpawnIntervalVariance));
        }
    }

    public void RegisterExtraEnemies(int count)
    {
        enemiesLeftToDie += count;
    }

    private void HandleEnemyCounter(EnemyHealthManager enemy, EnemyHealthManager.DeathReason reason)
    {
        enemiesLeftToDie--;
        if (enemiesLeftToDie <= 0)
        {
            OnWaveCompleted?.Invoke(waveNumber);
        }
    }

    public void LoadData(RunData data)
    {
        this.waveNumber = data.currentWave;

        currentStats = new Dictionary<string, EnemyStats>();
        if (data.enemyTypeStats != null && data.enemyTypeStats.Count > 0)
        {
            foreach (EnemyStats saved in data.enemyTypeStats)
                currentStats[saved.id] = saved;
        }
        else
        {
            // Seed from inspector defaults on a fresh run
            foreach (EnemyTypeDefinition type in enemyTypes)
            {
                currentStats[type.id] = new EnemyStats
                {
                    id = type.id,
                    health = type.baseHealth,
                    moveSpeed = type.baseMoveSpeed,
                    damage = type.baseDamage
                };
            }
        }

        OnWaveCompleted?.Invoke(waveNumber);
    }

    public void SaveData(ref RunData data)
    {
        data.currentWave = this.waveNumber;
        data.enemyTypeStats = new List<EnemyStats>(currentStats.Values);
    }
}
