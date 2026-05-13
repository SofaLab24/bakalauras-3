using UnityEngine;

[CreateAssetMenu(menuName = "MetaUpgrades/Multiply Fire Rate", order = 3)]
public class MultiplyFireRateEffect : MetaUpgradeEffect
{
    public float multiplier = 2f;

    public override void Apply(BuildingSettings settings)
    {
        settings.towerShootingDelay /= multiplier;
    }
}
