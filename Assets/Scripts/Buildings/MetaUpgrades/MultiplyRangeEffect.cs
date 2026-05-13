using UnityEngine;

[CreateAssetMenu(menuName = "MetaUpgrades/Multiply Range", order = 2)]
public class MultiplyRangeEffect : MetaUpgradeEffect
{
    public float multiplier = 2f;

    public override void Apply(BuildingSettings settings)
    {
        settings.towerRange *= multiplier;
    }
}
