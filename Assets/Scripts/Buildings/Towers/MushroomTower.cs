using UnityEngine;
using System.Collections;

public class MushroomTower : BaseTower
{
    [SerializeField] private float slowPercent = 30f;
    [SerializeField] private float slowDuration = 2f;

    private const float MaxSlowDuration = 6f;
    private const float MaxSlowPercent = 80f;

    public override void Initialize(BuildingSettings settings)
    {
        base.Initialize(settings);
        slowPercent = settings.towerSlowPercent;
        slowDuration = settings.towerSlowDuration;
    }

    protected override void RegisterUpgrades()
    {
        int cost = settings != null ? settings.buildingCost : 0;
        upgrades.Add(new TowerUpgrade("SLOW DURATION", 100,
            () => slowDuration = Mathf.Min(slowDuration * 1.5f, MaxSlowDuration)));
        upgrades.Add(new TowerUpgrade("SLOW %", cost,
            () => slowPercent = Mathf.Min(slowPercent * 1.5f, MaxSlowPercent)));
    }

    protected override void OnEnable()
    {
        WaveManager.OnWaveStarted += OnWaveStarted;
        WaveManager.OnWaveCompleted += OnWaveCompleted;
        BuildingManager.TriggerRangeIndicator += ToggleRangeIndicator;
        rangeIndicatorActive = false;
    }

    protected override void OnDisable()
    {
        WaveManager.OnWaveStarted -= OnWaveStarted;
        WaveManager.OnWaveCompleted -= OnWaveCompleted;
        StopShooting();
        BuildingManager.TriggerRangeIndicator -= ToggleRangeIndicator;
    }

    private void OnWaveStarted(int waveNumber) => StartShooting();
    private void OnWaveCompleted(int waveNumber) => StopShooting();

    protected override IEnumerator ShootingRoutine()
    {
        while (true)
        {
            BurstSlow();
            yield return new WaitForSeconds(shootingSpeed);
        }
    }

    private void BurstSlow()
    {
        animator.SetTrigger("Shoot");

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        foreach (Collider2D col in hitColliders)
        {
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.ApplySlow(slowPercent, slowDuration);
        }

        SFXManager.Instance.ShootSFX(GetComponent<AudioSource>());
    }

    public override void DealDamage(Vector3 targetPosition, Transform target) { }

    public override string GetStatsText()
    {
        float burstsPerSec = shootingSpeed > 0f ? 1f / shootingSpeed : 0f;
        return $"- Slow: {slowPercent:F0}%\n- Duration: {slowDuration:F1}s\n- Burst Rate: {burstsPerSec:F1}/s\n- Range: {range:F0}";
    }
}
