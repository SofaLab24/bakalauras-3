using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class BossControllerTests
{
    private GameObject bossGo;
    private GameObject sliderGo;
    private BossController boss;
    private EnemyHealthManager healthManager;

    private bool childrenSpawnedFired;
    private int spawnedChildCount;

    private void OnChildrenSpawned(int count)
    {
        childrenSpawnedFired = true;
        spawnedChildCount = count;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        childrenSpawnedFired = false;
        spawnedChildCount = -1;

        BossController.OnBossSpawnedChildren += OnChildrenSpawned;

        sliderGo = new GameObject("Slider");
        var slider = sliderGo.AddComponent<Slider>();

        bossGo = new GameObject("Boss");
        bossGo.AddComponent<Rigidbody2D>();
        bossGo.AddComponent<SpriteRenderer>();
        healthManager = bossGo.AddComponent<EnemyHealthManager>();

        typeof(EnemyHealthManager)
            .GetField("healthBar", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(healthManager, slider);

        boss = bossGo.AddComponent<BossController>();

        // Initialize while active so EnemyController.Awake has already resolved healthManager,
        // then deactivate — Start (and its event subscription) is deferred until SetActive(true).
        boss.Initialize(1, new List<Vector2> { new Vector2(100f, 100f) }, 1f, 1);
        bossGo.SetActive(false);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        BossController.OnBossSpawnedChildren -= OnChildrenSpawned;
        Object.Destroy(sliderGo);
        if (bossGo != null) Object.Destroy(bossGo);
        yield return null;
    }

    private List<(EnemyTypeDefinition type, EnemyStats stats)> MakePool(int count)
    {
        var pool = new List<(EnemyTypeDefinition type, EnemyStats stats)>();
        for (int i = 0; i < count; i++)
            pool.Add((new EnemyTypeDefinition(), new EnemyStats()));
        return pool;
    }

    // --- Child pool with entries ---

    [UnityTest]
    public IEnumerator BossKilledByTower_WithChildPool_FiresOnBossSpawnedChildrenEvent()
    {
        boss.SetChildPool(MakePool(2));
        bossGo.SetActive(true);
        yield return null; // Start runs; HandleSelfDeath subscribed to OnEnemyDeath

        healthManager.TakeDamage(healthManager.currentHealth);
        yield return null; // Destroy executes

        Assert.IsTrue(childrenSpawnedFired, "OnBossSpawnedChildren should fire when boss is killed by a tower.");
        Assert.AreEqual(2, spawnedChildCount, "Event should report the full child pool count.");
    }

    // --- Null / empty pool guards ---

    [UnityTest]
    public IEnumerator BossKilledByTower_WithNullChildPool_DoesNotFireEvent()
    {
        boss.SetChildPool(null);
        bossGo.SetActive(true);
        yield return null;

        healthManager.TakeDamage(healthManager.currentHealth);
        yield return null;

        Assert.IsFalse(childrenSpawnedFired, "OnBossSpawnedChildren should not fire when child pool is null.");
    }

    [UnityTest]
    public IEnumerator BossKilledByTower_WithEmptyPool_DoesNotFireEvent()
    {
        boss.SetChildPool(new List<(EnemyTypeDefinition, EnemyStats)>());
        bossGo.SetActive(true);
        yield return null;

        healthManager.TakeDamage(healthManager.currentHealth);
        yield return null;

        Assert.IsFalse(childrenSpawnedFired, "OnBossSpawnedChildren should not fire when child pool is empty.");
    }

    // --- ReachedEnd death reason ---

    [UnityTest]
    public IEnumerator BossKilledByReachedEnd_DoesNotFireEvent()
    {
        boss.SetChildPool(MakePool(3));
        bossGo.SetActive(true);
        yield return null; // Start runs

        healthManager.ReachEnd();
        yield return null;

        Assert.IsFalse(childrenSpawnedFired, "OnBossSpawnedChildren should not fire when the boss reaches the end.");
    }
}
