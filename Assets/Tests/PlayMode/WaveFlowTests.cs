using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class WaveFlowTests
{
    private GameObject waveManagerGo;
    private WaveManager waveManager;
    private GameObject dpmGo; // DataPersistenceManager — required by WaveManager.Start

    private bool waveCompletedFired;
    private int lastCompletedWave;

    private void OnWaveCompleted(int waveNumber)
    {
        waveCompletedFired = true;
        lastCompletedWave = waveNumber;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // WaveManager.Start calls DataPersistenceManager.Instance.LoadRun().
        // FileDataHandler.LoadObjectData returns null when the file does not exist,
        // so LoadRun gracefully falls back to NewRun(). No test data is written.
        if (DataPersistenceManager.Instance == null)
        {
            dpmGo = new GameObject("DataPersistenceManager");
            dpmGo.AddComponent<DataPersistenceManager>();
        }
        else
        {
            dpmGo = null; // already exists; don't own it
        }

        waveManagerGo = new GameObject("WaveManager");
        waveManager = waveManagerGo.AddComponent<WaveManager>();

        // Wait for Start to run. WaveManager.LoadData fires OnWaveCompleted internally
        // (wave 0) — we subscribe AFTER this to avoid catching that spurious event.
        yield return null;

        waveCompletedFired = false;
        lastCompletedWave = -1;
        WaveManager.OnWaveCompleted += OnWaveCompleted;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        WaveManager.OnWaveCompleted -= OnWaveCompleted;

        Object.Destroy(waveManagerGo);

        if (dpmGo != null)
        {
            Object.Destroy(dpmGo);
            // Reset the singleton so the next test can create a fresh instance.
            typeof(DataPersistenceManager)
                .GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .GetSetMethod(nonPublic: true)
                .Invoke(null, new object[] { null });
        }

        yield return null; // Let Destroy calls execute
    }

    // Fires EnemyHealthManager.OnEnemyDeath by calling Die() on a minimal enemy.
    // Die() is synchronous so the event is delivered before this method returns.
    private void FireDeathEvent(EnemyHealthManager.DeathReason reason = EnemyHealthManager.DeathReason.KilledByTower)
    {
        var go = new GameObject("TempEnemy");

        var sliderGo = new GameObject("Slider");
        sliderGo.transform.SetParent(go.transform); // child → destroyed with parent
        var slider = sliderGo.AddComponent<Slider>();

        var health = go.AddComponent<EnemyHealthManager>();
        typeof(EnemyHealthManager)
            .GetField("healthBar", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(health, slider);

        // Fires OnEnemyDeath synchronously, then schedules Destroy(go)
        health.Die(reason);
    }

    // --- Counter ---

    [UnityTest]
    public IEnumerator EnemyDeath_DecrementsEnemiesLeftToDie()
    {
        waveManager.enemiesLeftToDie = 3;

        FireDeathEvent(); // HandleEnemyCounter is called synchronously

        yield return null;

        Assert.AreEqual(2, waveManager.enemiesLeftToDie);
    }

    [UnityTest]
    public IEnumerator WaveCompleted_EventDoesNotFire_WhenEnemiesRemain()
    {
        waveManager.enemiesLeftToDie = 2;

        FireDeathEvent(); // decrements to 1, not yet zero

        yield return null;

        Assert.IsFalse(waveCompletedFired,
            "OnWaveCompleted should not fire while enemies are still alive.");
    }

    // --- Wave completion ---

    [UnityTest]
    public IEnumerator WaveCompleted_EventFires_WhenLastEnemyDies()
    {
        waveManager.enemiesLeftToDie = 1;
        waveManager.waveNumber = 5;

        FireDeathEvent(); // decrements to 0 → fires OnWaveCompleted(5)

        yield return null;

        Assert.IsTrue(waveCompletedFired,
            "OnWaveCompleted should fire when the last enemy dies.");
        Assert.AreEqual(5, lastCompletedWave);
    }

    // --- RegisterExtraEnemies ---

    [UnityTest]
    public IEnumerator RegisterExtraEnemies_IncreasesCounter()
    {
        waveManager.enemiesLeftToDie = 1;

        waveManager.RegisterExtraEnemies(5);

        yield return null;

        Assert.AreEqual(6, waveManager.enemiesLeftToDie);
    }
}
