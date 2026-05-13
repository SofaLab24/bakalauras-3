using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class BaseManagerIntegrationTests
{
    private GameObject baseManagerGo;
    private BaseManager baseManager;

    private bool healthChangeFired;
    private int lastHealthChangeValue;

    private void OnHealthChange(int health)
    {
        healthChangeFired = true;
        lastHealthChangeValue = health;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        baseManagerGo = new GameObject("BaseManager");
        baseManager = baseManagerGo.AddComponent<BaseManager>();

        baseManager.LoadData(new RunData { currentHealth = 100 });

        yield return null;

        healthChangeFired = false;
        lastHealthChangeValue = -1;
        BaseManager.OnBaseHealthChange += OnHealthChange;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        BaseManager.OnBaseHealthChange -= OnHealthChange;
        Object.Destroy(baseManagerGo);
        yield return null;
    }

    // --- TakeDamage ---

    [UnityTest]
    public IEnumerator TakeDamage_ReducesCurrentHealth()
    {
        baseManager.TakeDamage(20);

        yield return null;

        Assert.AreEqual(80, baseManager.GetCurrentHealth());
    }

    [UnityTest]
    public IEnumerator OnBaseHealthChange_FiresWithCorrectValue()
    {
        baseManager.TakeDamage(30);

        yield return null;

        Assert.IsTrue(healthChangeFired,
            "OnBaseHealthChange should fire when TakeDamage is called.");
        Assert.AreEqual(70, lastHealthChangeValue);
    }

    // --- LoadData ---

    [UnityTest]
    public IEnumerator LoadData_ResetsToMaxHealth_WhenRunDataHealthIsZero()
    {
        // Unsubscribe temporarily so the LoadData event doesn't interfere with assertions
        BaseManager.OnBaseHealthChange -= OnHealthChange;

        baseManager.LoadData(new RunData { currentHealth = 0 });

        yield return null;

        Assert.AreEqual(baseManager.GetMaxHealth(), baseManager.GetCurrentHealth(),
            "When saved health is 0 (new run), base should restore to max health.");

        // Re-subscribe for TearDown to clean up cleanly
        BaseManager.OnBaseHealthChange += OnHealthChange;
    }

    // --- Multiple damage calls ---

    [UnityTest]
    public IEnumerator TakeDamage_MultipleCalls_AccumulatesDamageCorrectly()
    {
        baseManager.TakeDamage(30);
        baseManager.TakeDamage(40);

        yield return null;

        Assert.AreEqual(30, baseManager.GetCurrentHealth(),
            "Health should reflect the sum of all damage taken.");
    }

    // --- OnBaseDamaged event ---

    [UnityTest]
    public IEnumerator OnBaseDamaged_FiresWhenTakeDamage()
    {
        bool damagedFired = false;
        void Handler() { damagedFired = true; }

        BaseManager.OnBaseDamaged += Handler;
        baseManager.TakeDamage(10);
        BaseManager.OnBaseDamaged -= Handler;

        yield return null;

        Assert.IsTrue(damagedFired,
            "OnBaseDamaged should fire whenever TakeDamage is called.");
    }

    // --- SaveData ---

    [UnityTest]
    public IEnumerator SaveData_PersistsCurrentHealth()
    {
        baseManager.TakeDamage(25);

        var data = new RunData();
        baseManager.SaveData(ref data);

        yield return null;

        Assert.AreEqual(75, data.currentHealth,
            "SaveData should store the current health after damage.");
    }

    // --- Integration: enemy reaches end ---

    [UnityTest]
    public IEnumerator Enemy_ReachEnd_DamagesBase_ByEnemyDamageValue()
    {
        // Build a minimal enemy — no EnemyController movement needed, only the health manager
        var enemyGo = new GameObject("Enemy");
        var sliderGo = new GameObject("Slider");
        sliderGo.transform.SetParent(enemyGo.transform); // child → destroyed with parent
        var slider = sliderGo.AddComponent<Slider>();

        var enemyHealth = enemyGo.AddComponent<EnemyHealthManager>();
        typeof(EnemyHealthManager)
            .GetField("healthBar", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(enemyHealth, slider);

        enemyHealth.damageValue = 15;

        int healthBefore = baseManager.GetCurrentHealth();

        // ReachEnd → Die(ReachedEnd) → OnEnemyDeath fired synchronously
        // → BaseManager.HandleEnemyAttack → TakeDamage(15)
        // enemyGo is scheduled for Destroy inside Die()
        enemyHealth.ReachEnd();

        yield return null; // Let Destroy execute

        Assert.AreEqual(healthBefore - 15, baseManager.GetCurrentHealth(),
            "Base health should decrease by the enemy's damage value when it reaches the end.");
    }
}
