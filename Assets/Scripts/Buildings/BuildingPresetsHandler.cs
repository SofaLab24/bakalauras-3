using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPresetsHandler : MonoBehaviour, ISettingsPersistence
{
    public static BuildingPresetsHandler Instance { get; private set; }
    void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Found more than one Building Presets Handler in the scene");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    [SerializeField] private List<BuildingSettings> defaultBuildingPresets;
    private List<BuildingSettings> buildingPresets;
    [SerializeField] string poisonTypeUpgradePrefix = "Poison";


    public void LoadSettings(List<BuildingSettings> savedPresets)
    {
        this.buildingPresets = new List<BuildingSettings>();
        if (savedPresets.Count == 0)
        {
            foreach (BuildingSettings preset in defaultBuildingPresets)
            {
                this.buildingPresets.Add(preset.CloneInstance());
            }
            return;
        }
        foreach (BuildingSettings savedPreset in savedPresets)
        {
            BuildingSettings defaultPreset = defaultBuildingPresets.Find(p => p.towerName == savedPreset.towerName);
            if (defaultPreset == null) continue;
            this.buildingPresets.Add(CombineBuildingPresets(defaultPreset, savedPreset));
        }
        // Add any towers that exist in defaults but are missing from the save (e.g. newly added towers)
        foreach (BuildingSettings defaultPreset in defaultBuildingPresets)
        {
            if (this.buildingPresets.Find(p => p.towerName == defaultPreset.towerName) == null)
                this.buildingPresets.Add(defaultPreset.CloneInstance());
        }
    }
    private BuildingSettings CombineBuildingPresets(BuildingSettings defaultPreset, BuildingSettings savedPreset)
    {
        BuildingSettings combinedPreset = savedPreset;

        // restore fields that are not serialized to disk
        combinedPreset.towerName = defaultPreset.towerName;
        combinedPreset.buildingPrefab = defaultPreset.buildingPrefab;
        combinedPreset.buildingIcon = defaultPreset.buildingIcon;
        combinedPreset.projectileSpeedCurve = defaultPreset.projectileSpeedCurve;
        combinedPreset.towerProjectilePrefab = defaultPreset.towerProjectilePrefab;
        combinedPreset.towerExplosionPrefab = defaultPreset.towerExplosionPrefab;
        combinedPreset.metaUpgradeDefinitions = new List<MetaUpgradeDefinition>(defaultPreset.metaUpgradeDefinitions);
        combinedPreset.enemyLayer = defaultPreset.enemyLayer;

        return combinedPreset;
    }

    public List<BuildingSettings> SaveSettings()
    {
        return buildingPresets;
    }

    public BuildingSettings GetBuildingPreset(string buildingName)
    {
        return buildingPresets.Find(preset => preset.towerName == buildingName);
    }
    public List<BuildingSettings> GetAllBuildingPresets()
    {
        return buildingPresets;
    }
    public void UpgradeBuilding(string buildingName, MetaUpgradeType upgradeType)
    {
        BuildingSettings preset = GetBuildingPreset(buildingName);
        preset.metaUpgradeDefinitions.Find(d => d.upgradeType == upgradeType)?.effect.Apply(preset);
        DataPersistenceManager.Instance.SaveGame();
    }
    public void UnlockPoisonType(string buildingName)
    {
        string fullTowerName = poisonTypeUpgradePrefix + buildingName;
        buildingPresets.Find(preset => preset.towerName == fullTowerName).isUnlocked = true;
    }
}
