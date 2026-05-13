using UnityEngine;

[CreateAssetMenu(menuName = "MetaUpgrades/Multiply Damage", order = 1)]
public class MultiplyDamageEffect : MetaUpgradeEffect
{
    public float multiplier = 2f;

    public override void Apply(BuildingSettings settings)
    {
        settings.towerDamage = Mathf.RoundToInt(settings.towerDamage * multiplier);
    }
}
