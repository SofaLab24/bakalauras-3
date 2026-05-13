using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BaseTowerTests
{
    // Minimal concrete subclass used only for testing
    private class TestTower : BaseTower
    {
        public int GetDamage() => damage;

        public override void DealDamage(Vector3 targetPosition, Transform target) { }
    }

    private GameObject towerGo;
    private GameObject economyGo;
    private TestTower tower;
    private PlayerEconomyManager economy;
    private BuildingSettings settings;

    [SetUp]
    public void Setup()
    {
        towerGo = new GameObject();
        economyGo = new GameObject();
        tower = towerGo.AddComponent<TestTower>();
        economy = economyGo.AddComponent<PlayerEconomyManager>();

        settings = ScriptableObject.CreateInstance<BuildingSettings>();
        settings.towerDamage = 20;
        settings.towerRange = 5f;
        settings.buildingCost = 150;
        settings.towerShootingDelay = 1f;
        settings.towerProjectileSpeed = 10f;
        settings.towerName = "TestTower";
        tower.Initialize(settings);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(towerGo);
        Object.DestroyImmediate(economyGo);
        Object.DestroyImmediate(settings);
    }

    private void SetMoney(int amount)
    {
        var data = new RunData { currentMoney = amount };
        economy.LoadData(data);
    }

    private TowerUpgrade DamageUpgrade => tower.GetUpgrades()[0];
    private TowerUpgrade SpecialtyUpgrade => tower.GetUpgrades()[1];

    // --- Initialize ---

    [Test]
    public void Initialize_SetsDamageFromSettings()
    {
        Assert.AreEqual(20, tower.GetDamage());
    }

    [Test]
    public void Initialize_SetsRangeFromSettings()
    {
        Assert.AreEqual(5f, tower.Range);
    }

    [Test]
    public void Initialize_DamageUpgrade_IsNotPurchasedByDefault()
    {
        Assert.IsFalse(DamageUpgrade.IsPurchased);
    }

    [Test]
    public void Initialize_SpecialtyUpgrade_IsNotPurchasedByDefault()
    {
        Assert.IsFalse(SpecialtyUpgrade.IsPurchased);
    }

    // --- Damage upgrade (index 0) ---

    [Test]
    public void DamageUpgrade_TryPurchase_ReturnsFalse_WhenInsufficientFunds()
    {
        SetMoney(50); // DamageCost = 100
        Assert.IsFalse(DamageUpgrade.TryPurchase(economy));
    }

    [Test]
    public void DamageUpgrade_TryPurchase_ReturnsTrue_WhenExactFunds()
    {
        SetMoney(100); // DamageCost = 100
        Assert.IsTrue(DamageUpgrade.TryPurchase(economy));
    }

    [Test]
    public void DamageUpgrade_TryPurchase_ReturnsTrue_WhenSufficientFunds()
    {
        SetMoney(500);
        Assert.IsTrue(DamageUpgrade.TryPurchase(economy));
    }

    [Test]
    public void DamageUpgrade_TryPurchase_IncreasesDamageByOnePointFiveTimes()
    {
        SetMoney(500);
        DamageUpgrade.TryPurchase(economy);
        Assert.AreEqual(Mathf.RoundToInt(20 * 1.5f), tower.GetDamage());
    }

    [Test]
    public void DamageUpgrade_TryPurchase_SetsPurchasedFlag()
    {
        SetMoney(500);
        DamageUpgrade.TryPurchase(economy);
        Assert.IsTrue(DamageUpgrade.IsPurchased);
    }

    [Test]
    public void DamageUpgrade_TryPurchase_DeductsExactCostFromBalance()
    {
        SetMoney(250);
        DamageUpgrade.TryPurchase(economy); // DamageCost = 100
        Assert.AreEqual(150, economy.playerMoney);
    }

    [Test]
    public void DamageUpgrade_TryPurchase_ReturnsFalse_WhenAlreadyPurchased()
    {
        SetMoney(500);
        DamageUpgrade.TryPurchase(economy);
        Assert.IsFalse(DamageUpgrade.TryPurchase(economy));
    }

    [Test]
    public void DamageUpgrade_TryPurchase_DoesNotDeductMoney_WhenAlreadyPurchased()
    {
        SetMoney(500);
        DamageUpgrade.TryPurchase(economy);
        int balanceAfterFirst = economy.playerMoney;
        DamageUpgrade.TryPurchase(economy);
        Assert.AreEqual(balanceAfterFirst, economy.playerMoney);
    }

    // --- Specialty upgrade (index 1) ---

    [Test]
    public void SpecialtyUpgrade_TryPurchase_ReturnsFalse_WhenInsufficientFunds()
    {
        SetMoney(50); // SpecialtyCost = buildingCost = 150
        Assert.IsFalse(SpecialtyUpgrade.TryPurchase(economy));
    }

    [Test]
    public void SpecialtyUpgrade_TryPurchase_ReturnsTrue_WhenExactFunds()
    {
        SetMoney(150); // SpecialtyCost = 150
        Assert.IsTrue(SpecialtyUpgrade.TryPurchase(economy));
    }

    [Test]
    public void SpecialtyUpgrade_TryPurchase_SetsPurchasedFlag()
    {
        SetMoney(500);
        SpecialtyUpgrade.TryPurchase(economy);
        Assert.IsTrue(SpecialtyUpgrade.IsPurchased);
    }

    [Test]
    public void SpecialtyUpgrade_TryPurchase_DeductsSpecialtyCostFromBalance()
    {
        SetMoney(400);
        SpecialtyUpgrade.TryPurchase(economy); // SpecialtyCost = 150
        Assert.AreEqual(250, economy.playerMoney);
    }

    [Test]
    public void SpecialtyUpgrade_TryPurchase_ReturnsFalse_WhenAlreadyPurchased()
    {
        SetMoney(500);
        SpecialtyUpgrade.TryPurchase(economy);
        Assert.IsFalse(SpecialtyUpgrade.TryPurchase(economy));
    }

    [Test]
    public void SpecialtyUpgrade_TryPurchase_DoesNotDeductMoney_WhenAlreadyPurchased()
    {
        SetMoney(500);
        SpecialtyUpgrade.TryPurchase(economy);
        int balanceAfterFirst = economy.playerMoney;
        SpecialtyUpgrade.TryPurchase(economy);
        Assert.AreEqual(balanceAfterFirst, economy.playerMoney);
    }

    // --- LoadUpgrades ---

    [Test]
    public void LoadUpgrades_DamageTrue_AppliesDamageMultiplier()
    {
        tower.LoadUpgrades(new List<bool> { true, false });
        Assert.AreEqual(Mathf.RoundToInt(20 * 1.5f), tower.GetDamage());
    }

    [Test]
    public void LoadUpgrades_DamageTrue_SetsPurchasedFlag()
    {
        tower.LoadUpgrades(new List<bool> { true, false });
        Assert.IsTrue(DamageUpgrade.IsPurchased);
    }

    [Test]
    public void LoadUpgrades_DamageFalse_DoesNotChangeDamage()
    {
        tower.LoadUpgrades(new List<bool> { false, false });
        Assert.AreEqual(20, tower.GetDamage());
    }

    [Test]
    public void LoadUpgrades_SpecialtyTrue_SetsPurchasedFlag()
    {
        tower.LoadUpgrades(new List<bool> { false, true });
        Assert.IsTrue(SpecialtyUpgrade.IsPurchased);
    }
}
