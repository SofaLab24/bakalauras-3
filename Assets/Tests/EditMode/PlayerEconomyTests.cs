using NUnit.Framework;
using UnityEngine;

public class PlayerEconomyTests
{
    private GameObject go;
    private PlayerEconomyManager economy;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        economy = go.AddComponent<PlayerEconomyManager>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    private void SetMoney(int amount)
    {
        var data = new RunData();
        data.currentMoney = amount;
        economy.LoadData(data);
    }

    // --- CanAfford ---

    [Test]
    public void CanAfford_ReturnsFalse_WhenBalanceBelowCost()
    {
        SetMoney(50);
        Assert.IsFalse(economy.CanAfford(100));
    }

    [Test]
    public void CanAfford_ReturnsTrue_WhenBalanceEqualsExactCost()
    {
        SetMoney(100);
        Assert.IsTrue(economy.CanAfford(100));
    }

    [Test]
    public void CanAfford_ReturnsTrue_WhenBalanceExceedsCost()
    {
        SetMoney(200);
        Assert.IsTrue(economy.CanAfford(100));
    }

    [Test]
    public void CanAfford_ReturnsFalse_WhenBalanceIsZero()
    {
        SetMoney(0);
        Assert.IsFalse(economy.CanAfford(1));
    }

    // --- SpendMoney ---

    [Test]
    public void SpendMoney_ReturnsFalse_WhenInsufficientFunds()
    {
        SetMoney(50);
        Assert.IsFalse(economy.SpendMoney(100));
    }

    [Test]
    public void SpendMoney_BalanceUnchanged_WhenFundsTooLow()
    {
        SetMoney(50);
        economy.SpendMoney(100);
        Assert.AreEqual(50, economy.playerMoney);
    }

    [Test]
    public void SpendMoney_ReturnsTrue_WhenSufficientFunds()
    {
        SetMoney(200);
        Assert.IsTrue(economy.SpendMoney(100));
    }

    [Test]
    public void SpendMoney_DeductsCostFromBalance()
    {
        SetMoney(200);
        economy.SpendMoney(100);
        Assert.AreEqual(100, economy.playerMoney);
    }

    [Test]
    public void SpendMoney_ExactBalance_LeavesZero()
    {
        SetMoney(100);
        economy.SpendMoney(100);
        Assert.AreEqual(0, economy.playerMoney);
    }

    [Test]
    public void SpendMoney_MultiplePurchases_AccumulatesDeductions()
    {
        SetMoney(300);
        economy.SpendMoney(100);
        economy.SpendMoney(50);
        Assert.AreEqual(150, economy.playerMoney);
    }

    [Test]
    public void SpendMoney_FailedPurchase_DoesNotAffectSubsequentSuccess()
    {
        SetMoney(100);
        economy.SpendMoney(200); // fails
        Assert.IsTrue(economy.SpendMoney(100));
        Assert.AreEqual(0, economy.playerMoney);
    }

    // --- LoadData ---

    [Test]
    public void LoadData_SetsPlayerMoneyCorrectly()
    {
        SetMoney(500);
        Assert.AreEqual(500, economy.playerMoney);
    }

    [Test]
    public void LoadData_OverwritesPreviousBalance()
    {
        SetMoney(500);
        SetMoney(100);
        Assert.AreEqual(100, economy.playerMoney);
    }

    // --- SaveData ---

    [Test]
    public void SaveData_PersistsCurrentMoney()
    {
        SetMoney(300);
        var data = new RunData();
        economy.SaveData(ref data);
        Assert.AreEqual(300, data.currentMoney);
    }

}
