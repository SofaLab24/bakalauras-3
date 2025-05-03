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
    [SerializeField] string fireTypeUpgradePrefix = "Flaming";

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
            BuildingSettings combinedPreset = CombineBuildingPresets(defaultBuildingPresets.Find(p => p.towerName == savedPreset.towerName), savedPreset);
            this.buildingPresets.Add(combinedPreset);
        }
    }
    private BuildingSettings CombineBuildingPresets(BuildingSettings defaultPreset, BuildingSettings savedPreset)
    {
        BuildingSettings combinedPreset = savedPreset;

        // update not serialized fields
        combinedPreset.towerName = defaultPreset.towerName;
        combinedPreset.buildingPrefab = defaultPreset.buildingPrefab;
        combinedPreset.buildingIcon = defaultPreset.buildingIcon;
        combinedPreset.projectileSpeedCurve = defaultPreset.projectileSpeedCurve;
        combinedPreset.towerProjectilePrefab = defaultPreset.towerProjectilePrefab;
        combinedPreset.towerExplosionPrefab = defaultPreset.towerExplosionPrefab;

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
    public void UpgradeBuilding(string buildingName, UpgradeType upgradeType)
    {
        switch(upgradeType)
        {
            case UpgradeType.Damage:
                buildingPresets.Find(preset => preset.towerName == buildingName).towerDamage *= 2;
                break;
            case UpgradeType.Range:
                buildingPresets.Find(preset => preset.towerName == buildingName).towerRange *= 2;
                break;
            case UpgradeType.FireRate:
                buildingPresets.Find(preset => preset.towerName == buildingName).towerShootingDelay /= 2;
                break;
            case UpgradeType.FireType:
                UnlockFireType(buildingName);
                break;
        }
        DataPersistenceManager.Instance.SaveGame();
    }
    public void UnlockFireType(string buildingName)
    {
        string fullTowerName = fireTypeUpgradePrefix + buildingName;
        buildingPresets.Find(preset => preset.towerName == fullTowerName).isUnlocked = true;
    }
}
