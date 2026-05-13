using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class PlayerEconomyIntegrationTests
{
    private GameObject economyGo;
    private PlayerEconomyManager economy;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        economyGo = new GameObject("PlayerEconomyManager");
        economy = economyGo.AddComponent<PlayerEconomyManager>();

        // Yield one frame so Start runs and OnEnable subscriptions are live
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(economyGo);
        yield return null;
    }

    private EnemyHealthManager CreateEnemy(out GameObject enemyGo)
    {
        var sliderGo = new GameObject("Slider");
        var slider = sliderGo.AddComponent<Slider>();

        enemyGo = new GameObject("Enemy");
        sliderGo.transform.SetParent(enemyGo.transform); // destroyed with parent

        var health = enemyGo.AddComponent<EnemyHealthManager>();
        typeof(EnemyHealthManager)
            .GetField("healthBar", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(health, slider);
        return health;
    }

    // --- HandleEnemyDeath (gold reward) ---

    [UnityTest]
    public IEnumerator EnemyKilledByTower_IncreasesBalanceByMoneyValue()
    {
        // moneyMultiplier defaults to 10, so SetMoneyValue(1) → moneyValue = 10
        var enemyHealth = CreateEnemy(out var enemyGo);
        enemyHealth.SetMoneyValue(1);

        enemyHealth.TakeDamage(enemyHealth.currentHealth);

        yield return null; // Let Destroy execute

        Assert.AreEqual(10, economy.playerMoney,
            "Economy should receive the enemy's moneyValue when killed by a tower.");
    }

    [UnityTest]
    public IEnumerator EnemyReachedEnd_DoesNotIncreaseBalance()
    {
        var enemyHealth = CreateEnemy(out var enemyGo);
        enemyHealth.SetMoneyValue(1);

        enemyHealth.ReachEnd();

        yield return null;

        Assert.AreEqual(0, economy.playerMoney,
            "Economy should not award gold when an enemy reaches the end.");
    }
}
