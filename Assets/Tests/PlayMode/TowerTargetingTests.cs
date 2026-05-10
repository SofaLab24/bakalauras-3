using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class TowerTargetingTests
{
    // Minimal concrete tower: exposes currentTarget, suppresses animator/SFX/projectile calls.
    private class TestTower : BaseTower
    {
        public Transform GetCurrentTarget() => currentTarget;

        public override void DealDamage(Vector3 targetPosition, Transform target) { }

        protected override void ShootAtTarget() { /* no-op — avoids animator, projectile, and SFXManager NPEs */ }
    }

    private const int EnemyLayerIndex = 0; // Default layer; tests run in an empty scene

    private GameObject towerGo;
    private TestTower tower;
    private BuildingSettings settings;

    private readonly List<GameObject> toDestroy = new List<GameObject>();

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        settings = ScriptableObject.CreateInstance<BuildingSettings>();
        settings.towerName = "TestTower";
        settings.towerRange = 5f;
        settings.towerShootingDelay = 0.2f;
        settings.towerProjectileSpeed = 10f;
        settings.towerDamage = 10;
        settings.enemyLayer = 1 << EnemyLayerIndex;

        // Create the tower as inactive so we can call Initialize before the shooting loop starts
        towerGo = new GameObject("Tower");
        towerGo.SetActive(false);
        tower = towerGo.AddComponent<TestTower>();
        tower.Initialize(settings);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (towerGo != null) Object.Destroy(towerGo);
        foreach (var go in toDestroy)
            if (go != null) Object.Destroy(go);
        toDestroy.Clear();
        Object.Destroy(settings);
        yield return null;
    }

    // Creates a static target enemy: CircleCollider2D on the correct layer + EnemyHealthManager.
    private GameObject CreateEnemyAt(Vector3 position)
    {
        var go = new GameObject("Enemy");
        go.transform.position = position;
        go.layer = EnemyLayerIndex;

        go.AddComponent<CircleCollider2D>().radius = 0.5f;

        var sliderGo = new GameObject("Slider");
        sliderGo.transform.SetParent(go.transform); // child → destroyed together with parent
        var slider = sliderGo.AddComponent<Slider>();

        var health = go.AddComponent<EnemyHealthManager>();
        typeof(EnemyHealthManager)
            .GetField("healthBar", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(health, slider);

        toDestroy.Add(go);
        return go;
    }

    // --- Target acquisition ---

    [UnityTest]
    public IEnumerator Tower_FindsTarget_WhenEnemyWithinRange()
    {
        CreateEnemyAt(new Vector3(2f, 0f, 0f)); // within range 5

        // Activating starts the shooting loop; its first iteration immediately calls FindNewTarget
        towerGo.SetActive(true);

        // Wait for at least one full shooting cycle to guarantee a second FindNewTarget call
        yield return new WaitForSeconds(settings.towerShootingDelay + 0.1f);

        Assert.IsNotNull(tower.GetCurrentTarget(),
            "Tower should have found the enemy within range.");
    }

    [UnityTest]
    public IEnumerator Tower_DoesNotFindTarget_WhenEnemyOutsideRange()
    {
        CreateEnemyAt(new Vector3(10f, 0f, 0f)); // outside range 5

        towerGo.SetActive(true);

        yield return new WaitForSeconds(settings.towerShootingDelay + 0.1f);

        Assert.IsNull(tower.GetCurrentTarget(),
            "Tower should not find enemies outside its range.");
    }

    [UnityTest]
    public IEnumerator Tower_LosesTarget_WhenEnemyIsDestroyed()
    {
        var enemyGo = CreateEnemyAt(new Vector3(2f, 0f, 0f));

        towerGo.SetActive(true);
        yield return new WaitForSeconds(settings.towerShootingDelay + 0.1f);

        Assert.IsNotNull(tower.GetCurrentTarget(),
            "Precondition: tower should have acquired the enemy.");

        Object.Destroy(enemyGo);
        toDestroy.Remove(enemyGo);

        // After the next shooting cycle IsTargetValid returns false (destroyed GO == null in Unity),
        // so FindNewTarget is called and sets currentTarget to null.
        yield return new WaitForSeconds(settings.towerShootingDelay + 0.1f);

        Assert.IsNull(tower.GetCurrentTarget(),
            "Tower should lose its target once the enemy is destroyed.");
    }
}
