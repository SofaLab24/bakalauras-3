using UnityEngine;

[CreateAssetMenu(menuName = "MetaUpgrades/Unlock Building", order = 5)]
public class UnlockBuildingEffect : MetaUpgradeEffect
{
    public override void Apply(BuildingSettings settings)
    {
        settings.isUnlocked = true;
    }
}
