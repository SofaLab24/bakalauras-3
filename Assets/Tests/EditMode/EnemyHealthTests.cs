using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class EnemyHealthTests
{
    private GameObject go;
    private EnemyHealthManager healthManager;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        var sliderGo = new GameObject();
        var slider = sliderGo.AddComponent<Slider>();

        healthManager = go.AddComponent<EnemyHealthManager>();

        var field = typeof(EnemyHealthManager)
            .GetField("healthBar", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(healthManager, slider);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void DefaultCurrentHealth_Is100()
    {
        Assert.AreEqual(100, healthManager.currentHealth);
    }

    [Test]
    public void SetMoneyValue_MultipliesByMoneyMultiplier()
    {
        // moneyMultiplier field default = 10
        healthManager.SetMoneyValue(5);
        Assert.AreEqual(50, healthManager.moneyValue);
    }

    [Test]
    public void SetMoneyValue_Zero_SetsMoneyValueToZero()
    {
        healthManager.SetMoneyValue(0);
        Assert.AreEqual(0, healthManager.moneyValue);
    }

    [Test]
    public void SetMoneyValue_DoesNotAffectHealth()
    {
        healthManager.SetMoneyValue(99);
        Assert.AreEqual(100, healthManager.currentHealth);
    }

    [Test]
    public void TakeDamage_ReducesCurrentHealth()
    {
        healthManager.TakeDamage(30);
        Assert.AreEqual(70, healthManager.currentHealth);
    }

    [Test]
    public void TakeDamage_AccumulatesAcrossMultipleCalls()
    {
        healthManager.TakeDamage(20);
        healthManager.TakeDamage(30);
        Assert.AreEqual(50, healthManager.currentHealth);
    }

    [Test]
    public void SetPoisonDamage_DoesNotChangeCurrentHealth()
    {
        healthManager.SetPoisonDamage(15);
        Assert.AreEqual(100, healthManager.currentHealth);
    }

    [Test]
    public void DamageValue_DefaultIs10()
    {
        Assert.AreEqual(10, healthManager.damageValue);
    }

    // --- Initialize ---

    [Test]
    public void Initialize_SetsCurrentHealthToGivenValue()
    {
        healthManager.Initialize(50);
        Assert.AreEqual(50, healthManager.currentHealth);
    }

    [Test]
    public void Initialize_DoesNotAffectMoneyValue()
    {
        healthManager.Initialize(50);
        Assert.AreEqual(0, healthManager.moneyValue);
    }

    [Test]
    public void TakeDamage_ExactHealth_FiresOnEnemyDeathEvent()
    {
        bool deathFired = false;
        EnemyHealthManager.DeathReason capturedReason = default;

        void Handler(EnemyHealthManager mgr, EnemyHealthManager.DeathReason reason)
        {
            deathFired = true;
            capturedReason = reason;
        }

        // Die() calls Destroy(gameObject), which is invalid in EditMode.
        // Declare the expected error so the test runner does not treat it as a failure.
        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Destroy may not be called from edit mode"));

        EnemyHealthManager.OnEnemyDeath += Handler;
        try
        {
            healthManager.TakeDamage(healthManager.currentHealth); // deal exactly 100 damage
        }
        finally
        {
            EnemyHealthManager.OnEnemyDeath -= Handler;
        }

        Assert.IsTrue(deathFired, "OnEnemyDeath should fire when health reaches zero.");
        Assert.AreEqual(EnemyHealthManager.DeathReason.KilledByTower, capturedReason);
    }
}
