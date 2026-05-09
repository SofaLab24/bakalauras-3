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
    public void Initialize_DamageUpgraded_IsFalseByDefault()
    {
        Assert.IsFalse(tower.DamageUpgraded);
    }

    [Test]
    public void Initialize_SpecialtyUpgraded_IsFalseByDefault()
    {
        Assert.IsFalse(tower.SpecialtyUpgraded);
    }

    // --- TryUpgradeDamage ---

    [Test]
    public void TryUpgradeDamage_ReturnsFalse_WhenInsufficientFunds()
    {
        SetMoney(50); // DamageCost = 100
        Assert.IsFalse(tower.TryUpgradeDamage(economy));
    }

    [Test]
    public void TryUpgradeDamage_ReturnsTrue_WhenExactFunds()
    {
        SetMoney(100); // DamageCost = 100
        Assert.IsTrue(tower.TryUpgradeDamage(economy));
    }

    [Test]
    public void TryUpgradeDamage_ReturnsTrue_WhenSufficientFunds()
    {
        SetMoney(500);
        Assert.IsTrue(tower.TryUpgradeDamage(economy));
    }

    [Test]
    public void TryUpgradeDamage_IncreasesDamageByOnePointFiveTimes()
    {
        SetMoney(500);
        tower.TryUpgradeDamage(economy);
        Assert.AreEqual(Mathf.RoundToInt(20 * 1.5f), tower.GetDamage());
    }

    [Test]
    public void TryUpgradeDamage_SetsDamageUpgradedFlag()
    {
        SetMoney(500);
        tower.TryUpgradeDamage(economy);
        Assert.IsTrue(tower.DamageUpgraded);
    }

    [Test]
    public void TryUpgradeDamage_DeductsExactCostFromBalance()
    {
        SetMoney(250);
        tower.TryUpgradeDamage(economy); // DamageCost = 100
        Assert.AreEqual(150, economy.playerMoney);
    }

    [Test]
    public void TryUpgradeDamage_ReturnsFalse_WhenAlreadyUpgraded()
    {
        SetMoney(500);
        tower.TryUpgradeDamage(economy);
        Assert.IsFalse(tower.TryUpgradeDamage(economy));
    }

    [Test]
    public void TryUpgradeDamage_DoesNotDeductMoney_WhenAlreadyUpgraded()
    {
        SetMoney(500);
        tower.TryUpgradeDamage(economy);
        int balanceAfterFirst = economy.playerMoney;
        tower.TryUpgradeDamage(economy);
        Assert.AreEqual(balanceAfterFirst, economy.playerMoney);
    }

    // --- TryUpgradeSpecialty ---

    [Test]
    public void TryUpgradeSpecialty_ReturnsFalse_WhenInsufficientFunds()
    {
        SetMoney(50); // SpecialtyCost = buildingCost = 150
        Assert.IsFalse(tower.TryUpgradeSpecialty(economy));
    }

    [Test]
    public void TryUpgradeSpecialty_ReturnsTrue_WhenExactFunds()
    {
        SetMoney(150); // SpecialtyCost = 150
        Assert.IsTrue(tower.TryUpgradeSpecialty(economy));
    }

    [Test]
    public void TryUpgradeSpecialty_SetsSpecialtyUpgradedFlag()
    {
        SetMoney(500);
        tower.TryUpgradeSpecialty(economy);
        Assert.IsTrue(tower.SpecialtyUpgraded);
    }

    [Test]
    public void TryUpgradeSpecialty_DeductsSpecialtyCostFromBalance()
    {
        SetMoney(400);
        tower.TryUpgradeSpecialty(economy); // SpecialtyCost = 150
        Assert.AreEqual(250, economy.playerMoney);
    }

    [Test]
    public void TryUpgradeSpecialty_ReturnsFalse_WhenAlreadyUpgraded()
    {
        SetMoney(500);
        tower.TryUpgradeSpecialty(economy);
        Assert.IsFalse(tower.TryUpgradeSpecialty(economy));
    }

    [Test]
    public void TryUpgradeSpecialty_DoesNotDeductMoney_WhenAlreadyUpgraded()
    {
        SetMoney(500);
        tower.TryUpgradeSpecialty(economy);
        int balanceAfterFirst = economy.playerMoney;
        tower.TryUpgradeSpecialty(economy);
        Assert.AreEqual(balanceAfterFirst, economy.playerMoney);
    }

    // --- LoadUpgrades ---

    [Test]
    public void LoadUpgrades_DamageTrue_AppliesDamageMultiplier()
    {
        tower.LoadUpgrades(damageUpgraded: true, specialtyUpgraded: false);
        Assert.AreEqual(Mathf.RoundToInt(20 * 1.5f), tower.GetDamage());
    }

    [Test]
    public void LoadUpgrades_DamageTrue_SetsDamageUpgradedFlag()
    {
        tower.LoadUpgrades(damageUpgraded: true, specialtyUpgraded: false);
        Assert.IsTrue(tower.DamageUpgraded);
    }

    [Test]
    public void LoadUpgrades_DamageFalse_DoesNotChangeDamage()
    {
        tower.LoadUpgrades(damageUpgraded: false, specialtyUpgraded: false);
        Assert.AreEqual(20, tower.GetDamage());
    }

    [Test]
    public void LoadUpgrades_SpecialtyTrue_SetsSpecialtyUpgradedFlag()
    {
        tower.LoadUpgrades(damageUpgraded: false, specialtyUpgraded: true);
        Assert.IsTrue(tower.SpecialtyUpgraded);
    }
}
