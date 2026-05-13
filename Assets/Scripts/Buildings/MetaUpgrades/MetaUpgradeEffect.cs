using UnityEngine;

public abstract class MetaUpgradeEffect : ScriptableObject
{
    public int cost;
    public abstract void Apply(BuildingSettings settings);
}
