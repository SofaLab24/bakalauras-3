using System;
using System.Collections.Generic;
using UnityEngine;

public class BossController : EnemyController
{
    public static event Action<int> OnBossSpawnedChildren;
    [Header("Boss settings")]
    [SerializeField] int fixedHealth = 500;
    [SerializeField] float fixedMoveSpeed = 2f;
    [SerializeField] int fixedDamage = 20;

    [Header("Child Enemies")]
    [SerializeField] float childSpawnSpacing = 0.35f;

    private List<(EnemyTypeDefinition type, EnemyStats stats)> childPool;
    private bool childrenSpawned = false;

    void Start()
    {
        EnemyHealthManager.OnEnemyDeath += HandleSelfDeath;
    }

    void OnDestroy()
    {
        EnemyHealthManager.OnEnemyDeath -= HandleSelfDeath;
    }

    public override void Initialize(int damage, List<Vector2> targets, float moveSpeed, int health)
    {
        base.Initialize(fixedDamage, targets, fixedMoveSpeed, fixedHealth);
    }

    public void SetChildPool(List<(EnemyTypeDefinition type, EnemyStats stats)> pool)
    {
        childPool = pool;
    }

    private void HandleSelfDeath(EnemyHealthManager deadEnemy, EnemyHealthManager.DeathReason reason)
    {
        if (deadEnemy != healthManager) return;
        if (childrenSpawned) return;
        if (reason != EnemyHealthManager.DeathReason.KilledByTower) return;

        childrenSpawned = true;
        SpawnChildren();
    }

    private void SpawnChildren()
    {
        if (childPool == null || childPool.Count == 0 || targets == null || targets.Count == 0) return;

        // The boss itself already counts as one death; each child will trigger
        // another OnEnemyDeath, so notify the wave counter to stay in sync.
        OnBossSpawnedChildren?.Invoke(childPool.Count);

        Vector3 basePosition = transform.position;
        foreach ((EnemyTypeDefinition type, EnemyStats stats) entry in childPool)
        {
            if (entry.type.prefab == null) continue;
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-childSpawnSpacing, childSpawnSpacing),
                UnityEngine.Random.Range(-childSpawnSpacing, childSpawnSpacing),
                0f);
            EnemyController child = Instantiate(entry.type.prefab, basePosition + offset, Quaternion.identity);
            child.Initialize(entry.stats.damage, targets, entry.stats.moveSpeed, entry.stats.health);
            child.SetTypeId(entry.type.id);
        }
    }
}
