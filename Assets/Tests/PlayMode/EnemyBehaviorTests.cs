using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class EnemyBehaviorTests
{
    private GameObject enemyGo;
    private GameObject sliderGo;
    private EnemyController enemy;
    private EnemyHealthManager healthManager;

    private bool deathFired;
    private EnemyHealthManager.DeathReason lastDeathReason;

    private void HandleDeath(EnemyHealthManager mgr, EnemyHealthManager.DeathReason reason)
    {
        deathFired = true;
        lastDeathReason = reason;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        deathFired = false;
        EnemyHealthManager.OnEnemyDeath += HandleDeath;

        sliderGo = new GameObject("Slider");
        var slider = sliderGo.AddComponent<Slider>();

        enemyGo = new GameObject("Enemy");
        enemyGo.AddComponent<Rigidbody2D>();
        enemyGo.AddComponent<SpriteRenderer>();
        healthManager = enemyGo.AddComponent<EnemyHealthManager>();

        typeof(EnemyHealthManager)
            .GetField("healthBar", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(healthManager, slider);

        enemy = enemyGo.AddComponent<EnemyController>();

        enemyGo.SetActive(false);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        EnemyHealthManager.OnEnemyDeath -= HandleDeath;
        Object.Destroy(sliderGo);
        if (enemyGo != null) Object.Destroy(enemyGo);
        yield return null;
    }

    // --- Movement ---

    [UnityTest]
    public IEnumerator Enemy_MovesTowardTarget_AfterFixedUpdates()
    {
        var targets = new List<Vector2>
        {
            new Vector2(0f, -10f), // far first waypoint (not reached during test)
            new Vector2(5f,  0f)   // current target (targets[^1])
        };

        enemyGo.transform.position = Vector3.zero;
        enemy.Initialize(1, targets, 5f, 100);
        enemyGo.SetActive(true); // safe: targets is now set before first FixedUpdate

        Vector3 startPos = enemyGo.transform.position;

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        Assert.AreNotEqual(startPos, enemyGo.transform.position,
            "Enemy should have moved toward the target waypoint after physics updates.");
    }

    [UnityTest]
    public IEnumerator Enemy_CallsReachEnd_WhenPlacedAtLastWaypoint()
    {
        // Single waypoint and enemy is already there — distance == 0 < distanceOffset
        var endpoint = new Vector2(0f, 0f);
        var targets = new List<Vector2> { endpoint };

        enemyGo.transform.position = endpoint;
        enemy.Initialize(10, targets, 5f, 100);
        enemyGo.SetActive(true); // safe: targets is set before first FixedUpdate

        yield return new WaitForFixedUpdate(); // Move() detects distance == 0, calls ReachEnd
        yield return null;                     // Let Destroy(gameObject) execute

        Assert.IsTrue(deathFired, "OnEnemyDeath should have been fired.");
        Assert.AreEqual(EnemyHealthManager.DeathReason.ReachedEnd, lastDeathReason);
    }

    // --- Poison ---

    [UnityTest]
    public IEnumerator Poison_DealsDamageAfterOneTick()
    {
        // Place enemy far from endpoint so it never triggers ReachEnd during the test
        var targets = new List<Vector2> { new Vector2(100f, 0f) };
        enemy.Initialize(1, targets, 0f, 50);
        enemyGo.SetActive(true); // safe: targets is set; Update loop can now run

        int initialHealth = healthManager.currentHealth;
        healthManager.SetPoisonDamage(10);

        yield return new WaitForSeconds(1.1f); // poisonTickRate defaults to 1 s

        Assert.Less(healthManager.currentHealth, initialHealth,
            "Health should have decreased after one poison tick.");
    }

    [UnityTest]
    public IEnumerator Poison_KillsEnemy_WhenHealthDepleted()
    {
        var targets = new List<Vector2> { new Vector2(100f, 0f) };
        enemy.Initialize(1, targets, 0f, 1); // health = 1; one tick is enough to kill
        enemyGo.SetActive(true); // safe: targets is set before first Update/FixedUpdate

        healthManager.SetPoisonDamage(10);

        yield return new WaitForSeconds(1.1f);
        yield return null; // Let Destroy(gameObject) execute

        Assert.IsTrue(deathFired, "Enemy should have died from poison damage.");
        Assert.AreEqual(EnemyHealthManager.DeathReason.KilledByTower, lastDeathReason);
    }
}
