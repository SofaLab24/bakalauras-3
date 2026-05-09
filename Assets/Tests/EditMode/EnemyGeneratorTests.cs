using NUnit.Framework;
using UnityEngine;

public class EnemyGeneratorTests
{
    private GameObject go;
    private EnemyGenerator generator;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        generator = go.AddComponent<EnemyGenerator>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void GenerateEnemy_DamageAlwaysEqualsStatPoints()
    {
        for (int i = 0; i < 50; i++)
        {
            var (damage, _, _) = generator.GenerateEnemy(10);
            Assert.AreEqual(10, damage, "Damage must always equal the stat points input.");
        }
    }

    [Test]
    public void GenerateEnemy_HealthIsAtLeastOneHealthMultiplier()
    {
        // healthPoints = Random.Range(1, statPoints+1), minimum is 1 * healthMultiplier (10)
        for (int i = 0; i < 50; i++)
        {
            var (_, health, _) = generator.GenerateEnemy(5);
            Assert.GreaterOrEqual(health, 10, "Health must be at least 10 (1 healthPoint × healthMultiplier).");
        }
    }

    [Test]
    public void GenerateEnemy_HealthDoesNotExceedStatPoints_Times_Multiplier()
    {
        const int statPoints = 5;
        for (int i = 0; i < 50; i++)
        {
            var (_, health, _) = generator.GenerateEnemy(statPoints);
            Assert.LessOrEqual(health, statPoints * 10,
                "Health must not exceed statPoints × healthMultiplier.");
        }
    }

    [Test]
    public void GenerateEnemy_MoveSpeedIsAtLeastThree()
    {
        // leftStatPoints after health allocation is at least 1, so speed >= 1 + 2 = 3
        for (int i = 0; i < 50; i++)
        {
            var (_, _, moveSpeed) = generator.GenerateEnemy(5);
            Assert.GreaterOrEqual(moveSpeed, 3f, "MoveSpeed must be at least 3.");
        }
    }

    [Test]
    public void GenerateEnemy_MoveSpeedCappedAtFifteen()
    {
        // The cap to 15 fires when leftStatPoints >= 15.
        // When leftStatPoints == 14 the formula gives 14+2 = 16, so the real ceiling is 16.
        for (int i = 0; i < 50; i++)
        {
            var (_, _, moveSpeed) = generator.GenerateEnemy(100);
            Assert.LessOrEqual(moveSpeed, 16f, "MoveSpeed must not exceed 16 (cap is 15 only when leftStatPoints >= 15).");
        }
    }

    [Test]
    public void GenerateEnemy_SingleStatPoint_IsDeterministic()
    {
        // statPoints=1 → leftStatPoints=2 → healthPoints=Range(1,2)=1 always
        // leftStatPoints after -= 1 → leftStatPoints=1 → moveSpeed = 1+2 = 3
        var (damage, health, moveSpeed) = generator.GenerateEnemy(1);
        Assert.AreEqual(1, damage);
        Assert.AreEqual(10, health);
        Assert.AreEqual(3f, moveSpeed, 0.001f);
    }

    [Test]
    public void GenerateEnemy_LargeStatPoints_MovespeedReachesMax()
    {
        // With enough points (>=14 left after health allocation), speed hits 15
        bool reachedMax = false;
        for (int i = 0; i < 200; i++)
        {
            var (_, _, moveSpeed) = generator.GenerateEnemy(50);
            if (Mathf.Approximately(moveSpeed, 15f)) { reachedMax = true; break; }
        }
        Assert.IsTrue(reachedMax, "MoveSpeed should reach the 15 cap with enough stat points.");
    }
}
