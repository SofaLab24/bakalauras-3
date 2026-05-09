using System.Reflection;
using NUnit.Framework;
using UnityEngine;
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
    public void DamageValue_DefaultIsNotNegative()
    {
        Assert.GreaterOrEqual(healthManager.damageValue, 0);
    }
}
