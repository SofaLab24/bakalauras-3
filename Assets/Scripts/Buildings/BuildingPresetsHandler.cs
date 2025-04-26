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
            Debug.LogError("Found more than one Building Presets Handler in the scene");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    [SerializeField] private List<BuildingSettings> defaultBuildingPresets;
    private List<BuildingSettings> buildingPresets;

    public void LoadSettings(List<BuildingSettings> savedPresets)
    {
        if (savedPresets.Count == 0)
        {
            Debug.Log("No saved building presets found, using default ones");
            this.buildingPresets = defaultBuildingPresets;
            return;
        }
        Debug.Log("Loaded " + savedPresets.Count + " saved building presets");
        this.buildingPresets = new List<BuildingSettings>();
        foreach (BuildingSettings savedPreset in savedPresets)
        {
            BuildingSettings combinedPreset = CombineBuildingPresets(defaultBuildingPresets.Find(p => p.name == savedPreset.name), savedPreset);
            Debug.Log("Loaded " + combinedPreset.name);
            this.buildingPresets.Add(combinedPreset);
        }
    }
    private BuildingSettings CombineBuildingPresets(BuildingSettings defaultPreset, BuildingSettings savedPreset)
    {
        BuildingSettings combinedPreset = savedPreset;

        // update not serialized fields
        combinedPreset.name = defaultPreset.name;
        combinedPreset.buildingPrefab = defaultPreset.buildingPrefab;
        combinedPreset.buildingIcon = defaultPreset.buildingIcon;
        combinedPreset.projectileSpeedCurve = defaultPreset.projectileSpeedCurve;
        combinedPreset.towerProjectilePrefab = defaultPreset.towerProjectilePrefab;

        return combinedPreset;
    }

    public List<BuildingSettings> SaveSettings()
    {
        return buildingPresets;
    }

    public BuildingSettings GetBuildingPreset(string buildingName)
    {
        return buildingPresets.Find(preset => preset.name == buildingName);
    }
    public List<BuildingSettings> GetAllBuildingPresets()
    {
        return defaultBuildingPresets;
    }
    // TODO: update buildingPreset
}
