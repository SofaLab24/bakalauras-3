using UnityEngine;

[CreateAssetMenu(menuName = "MetaUpgrades/Unlock Poison Type", order = 4)]
public class UnlockPoisonTypeEffect : MetaUpgradeEffect
{
    public override void Apply(BuildingSettings settings)
    {
        BuildingPresetsHandler.Instance.UnlockPoisonType(settings.towerName);
    }
}
