using System.Collections.Generic;
using UnityEngine;

public class MoabController : EnemyController
{
    [Header("MOAB settings")]
    [SerializeField] int fixedHealth = 500;
    [SerializeField] float fixedMoveSpeed = 2f;
    [SerializeField] int fixedDamage = 20;

    [Header("Child Enemies")]
    [SerializeField] EnemyController childEnemyPrefab;
    [SerializeField] int childCount = 10;
    [SerializeField] int childDamage = 1;
    [SerializeField] int childHealth = 10;
    [SerializeField] float childMoveSpeed = 2f;
    [SerializeField] float childSpawnSpacing = 0.35f;

    private EnemyHealthManager healthManager;
    private bool childrenSpawned = false;

    void Start()
    {
        healthManager = GetComponent<EnemyHealthManager>();
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
        if (childEnemyPrefab == null || targets == null || targets.Count == 0) return;

        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            // The MOAB itself already counts as one death; each child will trigger
            // another OnEnemyDeath, so register the extra children so the wave
            // counter stays in sync.
            waveManager.RegisterExtraEnemies(childCount);
        }

        Vector3 basePosition = transform.position;
        for (int i = 0; i < childCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-childSpawnSpacing, childSpawnSpacing),
                Random.Range(-childSpawnSpacing, childSpawnSpacing),
                0f);
            EnemyController child = Instantiate(childEnemyPrefab, basePosition + offset, Quaternion.identity);
            child.Initialize(childDamage, targets, childMoveSpeed, childHealth);
        }
    }
}
