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
    [SerializeField] BossController bossPrefab;
    [SerializeField] string bossId = "Boss";
    [SerializeField] int bossWaveInterval = 10;
    [SerializeField] float enemyAmountMultiplier = 1.5f;
    [SerializeField] float enemySpawnInterval = 1.5f;
    [SerializeField] float enemySpawnIntervalVariance = 0.5f;
    [SerializeField] float enemySpawnPathDelayVariance = 0.75f;
    [SerializeField] int statScaleInterval = 10;
    [SerializeField] float statScaleMultiplier = 1.2f;
    public int waveNumber;

    public static event Action<int> OnWaveCompleted;
    public static event Action<int> OnWaveRestored;
    public static event Action<int> OnWaveStarted;
    public static event Action<Dictionary<string, int>> OnWavePoolGenerated;

    private int enemiesToGenerate;
    public int enemiesLeftToDie;
    private Stack<(EnemyTypeDefinition type, EnemyStats stats)> enemyPool;
    private Dictionary<string, EnemyStats> currentStats;
    private List<List<(EnemyTypeDefinition type, EnemyStats stats)>> pendingBossChildPools;

    PathGenerator pathGenerator;

    void OnEnable()
    {
        EnemyHealthManager.OnEnemyDeath += HandleEnemyCounter;
        BossController.OnBossSpawnedChildren += RegisterExtraEnemies;
    }

    void OnDisable()
    {
        EnemyHealthManager.OnEnemyDeath -= HandleEnemyCounter;
        BossController.OnBossSpawnedChildren -= RegisterExtraEnemies;
    }

    private void Start()
    {
        pathGenerator = GetComponent<PathGenerator>();
        DataPersistenceManager.Instance.LoadRun();
    }
    public void PrepareNextWave(float splitChance)
    {
        waveNumber++;
        TryScaleStats();
        pathGenerator.PlanNextPaths(splitChance);
        pathGenerator.PreviewPendingPaths();
        GenerateEnemyPool();
    }

    public void StartWave()
    {
        DataPersistenceManager.Instance.SaveRun();
        pathGenerator.CommitPendingPaths();
        enemiesLeftToDie = enemiesToGenerate;
        OnWaveStarted?.Invoke(waveNumber);

        TrySpawnBosses();

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

        enemyPool = new Stack<(EnemyTypeDefinition type, EnemyStats stats)>();
        Dictionary<string, int> composition = new Dictionary<string, int>();

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

            enemyPool.Push((chosenType, stats));
            remainingBudget -= chosenType.pointCost;

            if (composition.ContainsKey(chosenType.id))
                composition[chosenType.id]++;
            else
                composition[chosenType.id] = 1;
        }

        pendingBossChildPools = new List<List<(EnemyTypeDefinition type, EnemyStats stats)>>();
        if (bossPrefab != null && bossWaveInterval > 0 && waveNumber % bossWaveInterval == 0)
        {
            int bossCount = waveNumber / bossWaveInterval;
            composition[bossId] = bossCount;
            int childPoolSize = waveNumber;
            for (int m = 0; m < bossCount; m++)
                pendingBossChildPools.Add(GenerateChildPool(childPoolSize));
        }

        OnWavePoolGenerated?.Invoke(composition);
    }

    private void TryScaleStats()
    {
        if (waveNumber % statScaleInterval != 0) return;
        foreach (EnemyStats stats in currentStats.Values)
        {
            stats.health = Mathf.Max(stats.health + 1, Mathf.RoundToInt(stats.health * statScaleMultiplier));
            stats.damage = Mathf.Max(stats.damage + 1, Mathf.RoundToInt(stats.damage * statScaleMultiplier));
            stats.moveSpeed *= statScaleMultiplier;
        }
    }
    private List<(EnemyTypeDefinition type, EnemyStats stats)> GenerateChildPool(int count)
    {
        var pool = new List<(EnemyTypeDefinition type, EnemyStats stats)>();
        EnemyTypeDefinition cheapestType = enemyTypes.OrderBy(t => t.pointCost).First();
        int budget = count;
        for (int i = 0; i < count; i++)
        {
            List<EnemyTypeDefinition> eligible = enemyTypes.Where(t => t.pointCost <= budget).ToList();
            EnemyTypeDefinition chosenType = eligible.Count > 0
                ? eligible[UnityEngine.Random.Range(0, eligible.Count)]
                : cheapestType;
            EnemyStats stats = currentStats.TryGetValue(chosenType.id, out EnemyStats saved)
                ? saved
                : new EnemyStats { id = chosenType.id, health = chosenType.baseHealth, moveSpeed = chosenType.baseMoveSpeed, damage = chosenType.baseDamage };
            pool.Add((chosenType, stats));
            budget -= chosenType.pointCost;
        }
        return pool;
    }

    private IEnumerator SpawnEnemies(List<Vector2> enemyTargets, int enemyAmountPerPath, Vector3 initPosition)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, enemySpawnPathDelayVariance));
        for (int j = 0; j < enemyAmountPerPath; j++)
        {
            (EnemyTypeDefinition type, EnemyStats stats) entry = enemyPool.Pop();
            EnemyController enemy = Instantiate(entry.type.prefab, initPosition, Quaternion.identity);
            enemy.Initialize(entry.stats.damage, enemyTargets, entry.stats.moveSpeed, entry.stats.health);
            enemy.SetTypeId(entry.type.id);
            yield return new WaitForSeconds(enemySpawnInterval + UnityEngine.Random.Range(-enemySpawnIntervalVariance, enemySpawnIntervalVariance));
        }
    }
    private void TrySpawnBosses()
    {
        if (bossPrefab == null) return;
        if (bossWaveInterval <= 0) return;
        if (waveNumber % bossWaveInterval != 0) return;

        int bossCount = waveNumber / bossWaveInterval;
        enemiesLeftToDie += bossCount;

        List<Vector2> bossTargets = pathGenerator.GetPaths[0].enemyWalkPoints;
        Vector3 initPosition = new Vector3(bossTargets[^1].x, bossTargets[^1].y);
        StartCoroutine(SpawnBosses(bossTargets, bossCount, initPosition));
    }

    private IEnumerator SpawnBosses(List<Vector2> bossTargets, int bossCount, Vector3 initPosition)
    {
        for (int i = 0; i < bossCount; i++)
        {
            BossController boss = Instantiate(bossPrefab, initPosition, Quaternion.identity);
            boss.Initialize(0, bossTargets, 0f, 0);
            boss.SetTypeId(bossId);
            if (pendingBossChildPools != null && i < pendingBossChildPools.Count)
                boss.SetChildPool(pendingBossChildPools[i]);
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

        if (waveNumber == 0)
        {
            // Fresh game — plan the first wave.
            OnWaveCompleted?.Invoke(waveNumber);
        }
        else
        {
            // Existing save — restore the exact pool that was saved so the preview
            // is deterministic across sessions, falling back to a fresh roll if the
            // save pre-dates pool persistence.
            if (data.savedEnemyPoolIds != null && data.savedEnemyPoolIds.Count > 0)
                RestoreEnemyPool(data.savedEnemyPoolIds, data.savedEnemiesToGenerate);
            else
                GenerateEnemyPool();
            OnWaveRestored?.Invoke(waveNumber);
        }
    }

    public void SaveData(ref RunData data)
    {
        data.currentWave = this.waveNumber;
        data.enemyTypeStats = new List<EnemyStats>(currentStats.Values);
        // Stack enumerates top→bottom; save in that order so RestoreEnemyPool can push bottom→top.
        data.savedEnemyPoolIds = enemyPool != null
            ? enemyPool.Select(e => e.type.id).ToList()
            : new List<string>();
        data.savedEnemiesToGenerate = enemiesToGenerate;
    }

    private void RestoreEnemyPool(List<string> poolIds, int poolSize)
    {
        enemiesToGenerate = poolSize;
        Dictionary<string, EnemyTypeDefinition> typeMap = enemyTypes.ToDictionary(t => t.id);

        enemyPool = new Stack<(EnemyTypeDefinition type, EnemyStats stats)>();
        // poolIds is top→bottom; push bottom→top so the top element is restored correctly.
        for (int i = poolIds.Count - 1; i >= 0; i--)
        {
            if (!typeMap.TryGetValue(poolIds[i], out EnemyTypeDefinition def)) continue;
            EnemyStats stats = currentStats.TryGetValue(def.id, out EnemyStats s)
                ? s
                : new EnemyStats { id = def.id, health = def.baseHealth, moveSpeed = def.baseMoveSpeed, damage = def.baseDamage };
            enemyPool.Push((def, stats));
        }

        Dictionary<string, int> composition = new Dictionary<string, int>();
        foreach (string id in poolIds)
        {
            if (!composition.ContainsKey(id)) composition[id] = 0;
            composition[id]++;
        }

        pendingBossChildPools = new List<List<(EnemyTypeDefinition type, EnemyStats stats)>>();
        if (bossPrefab != null && bossWaveInterval > 0 && waveNumber % bossWaveInterval == 0)
        {
            int bossCount = waveNumber / bossWaveInterval;
            composition[bossId] = bossCount;
            int childPoolSize = waveNumber;
            for (int m = 0; m < bossCount; m++)
                pendingBossChildPools.Add(GenerateChildPool(childPoolSize));
        }

        OnWavePoolGenerated?.Invoke(composition);
    }
}
