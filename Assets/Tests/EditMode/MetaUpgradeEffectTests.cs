using NUnit.Framework;
using UnityEngine;

public class MetaUpgradeEffectTests
{
    private BuildingSettings settings;

    [SetUp]
    public void Setup()
    {
        settings = ScriptableObject.CreateInstance<BuildingSettings>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(settings);
    }

    // --- MultiplyDamageEffect ---

    [Test]
    public void MultiplyDamageEffect_Apply_MultipliesTowerDamage()
    {
        var effect = ScriptableObject.CreateInstance<MultiplyDamageEffect>();
        effect.multiplier = 2f;
        settings.towerDamage = 20;

        effect.Apply(settings);

        Assert.AreEqual(40, settings.towerDamage);
        Object.DestroyImmediate(effect);
    }

    [Test]
    public void MultiplyDamageEffect_Apply_RoundsResultToNearestInt()
    {
        var effect = ScriptableObject.CreateInstance<MultiplyDamageEffect>();
        effect.multiplier = 1.5f;
        settings.towerDamage = 10;

        effect.Apply(settings);

        Assert.AreEqual(15, settings.towerDamage);
        Object.DestroyImmediate(effect);
    }

    [Test]
    public void MultiplyDamageEffect_Apply_AppliedTwice_CompoundsMultiplier()
    {
        var effect = ScriptableObject.CreateInstance<MultiplyDamageEffect>();
        effect.multiplier = 2f;
        settings.towerDamage = 10;

        effect.Apply(settings);
        effect.Apply(settings);

        Assert.AreEqual(40, settings.towerDamage);
        Object.DestroyImmediate(effect);
    }

    // --- MultiplyRangeEffect ---

    [Test]
    public void MultiplyRangeEffect_Apply_MultipliesTowerRange()
    {
        var effect = ScriptableObject.CreateInstance<MultiplyRangeEffect>();
        effect.multiplier = 3f;
        settings.towerRange = 5f;

        effect.Apply(settings);

        Assert.AreEqual(15f, settings.towerRange, 0.001f);
        Object.DestroyImmediate(effect);
    }

    [Test]
    public void MultiplyRangeEffect_Apply_WithMultiplierLessThanOne_ReducesRange()
    {
        var effect = ScriptableObject.CreateInstance<MultiplyRangeEffect>();
        effect.multiplier = 0.5f;
        settings.towerRange = 10f;

        effect.Apply(settings);

        Assert.AreEqual(5f, settings.towerRange, 0.001f);
        Object.DestroyImmediate(effect);
    }

    // --- MultiplyFireRateEffect ---

    [Test]
    public void MultiplyFireRateEffect_Apply_DividesShootingDelayByMultiplier()
    {
        var effect = ScriptableObject.CreateInstance<MultiplyFireRateEffect>();
        effect.multiplier = 2f;
        settings.towerShootingDelay = 1f;

        effect.Apply(settings);

        Assert.AreEqual(0.5f, settings.towerShootingDelay, 0.001f);
        Object.DestroyImmediate(effect);
    }

    [Test]
    public void MultiplyFireRateEffect_Apply_MultiplierGreaterThanOne_ResultIsSmaller()
    {
        var effect = ScriptableObject.CreateInstance<MultiplyFireRateEffect>();
        effect.multiplier = 4f;
        settings.towerShootingDelay = 2f;

        effect.Apply(settings);

        Assert.Less(settings.towerShootingDelay, 2f);
        Object.DestroyImmediate(effect);
    }

    // --- UnlockBuildingEffect ---

    [Test]
    public void UnlockBuildingEffect_Apply_SetsIsUnlockedTrue()
    {
        var effect = ScriptableObject.CreateInstance<UnlockBuildingEffect>();
        settings.isUnlocked = false;

        effect.Apply(settings);

        Assert.IsTrue(settings.isUnlocked);
        Object.DestroyImmediate(effect);
    }

    [Test]
    public void UnlockBuildingEffect_Apply_AlreadyUnlocked_RemainsUnlocked()
    {
        var effect = ScriptableObject.CreateInstance<UnlockBuildingEffect>();
        settings.isUnlocked = true;

        effect.Apply(settings);

        Assert.IsTrue(settings.isUnlocked);
        Object.DestroyImmediate(effect);
    }
}
