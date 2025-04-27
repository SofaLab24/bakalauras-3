using UnityEngine;
using System;
using System.Collections.Generic;

public class ProgressionManager : MonoBehaviour, IGameDataPersistence
{
    private string selectedBuildingName;
    private int selectedBuildingIndex;
    private MainMenuManager mainMenuManager;

    public List<(string towerName, bool damageUpgraded, bool rangeUpgraded, bool fireRateUpgraded, bool fireTypeUnlocked)> towerUpgradeStatus;
    public int metaCoins;
    public int waveHighscore;

    public static ProgressionManager Instance { get; private set; }

    void OnEnable()
    {
        BaseManager.OnBaseDestroyed += HandleRunEnd;
    }
    void OnDisable()
    {
        BaseManager.OnBaseDestroyed -= HandleRunEnd;
    }
    void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Found more than one Building Upgrades Manager in the scene. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        mainMenuManager.UpdateMetaCoins(metaCoins);
    }
    public void SetSelectedBuilding(BuildingSettings building)
    {
        selectedBuildingName = building.towerName;
        selectedBuildingIndex = towerUpgradeStatus.FindIndex(upgrade => upgrade.towerName == selectedBuildingName);
    }
    public bool IsUpgradePurchased(UpgradeType upgradeType)
    {
        switch(upgradeType)
        {
            case UpgradeType.Damage:
                return towerUpgradeStatus[selectedBuildingIndex].damageUpgraded;
            case UpgradeType.Range:
                return towerUpgradeStatus[selectedBuildingIndex].rangeUpgraded;
            case UpgradeType.FireRate:
                return towerUpgradeStatus[selectedBuildingIndex].fireRateUpgraded;
            case UpgradeType.FireType:
                return towerUpgradeStatus[selectedBuildingIndex].fireTypeUnlocked;
        }
        return false;
    }
    public bool UpgradeBuilding(UpgradeType upgradeType)
    {
        int cost = GetUpgradeCost(upgradeType);
        if(IsUpgradePurchased(upgradeType) || metaCoins < cost)
        {
            return false;
        }
        BuildingPresetsHandler.Instance.UpgradeBuilding(selectedBuildingName, upgradeType);
        var upgradeStatus = towerUpgradeStatus[selectedBuildingIndex];
        switch(upgradeType)
        {
            case UpgradeType.Damage:
                upgradeStatus.damageUpgraded = true;
                break;
            case UpgradeType.Range:
                upgradeStatus.rangeUpgraded = true;
                break;
            case UpgradeType.FireRate:
                upgradeStatus.fireRateUpgraded = true;
                break;
            case UpgradeType.FireType:
                upgradeStatus.fireTypeUnlocked = true;
                break;
        }
        towerUpgradeStatus[selectedBuildingIndex] = upgradeStatus;
        metaCoins -= cost;
        mainMenuManager.UpdateMetaCoins(metaCoins);
        return true;
    }
    public int GetUpgradeCost(UpgradeType upgradeType, string towerName = "")
    {
        if(towerName == "") towerName = selectedBuildingName;
        if (upgradeType == UpgradeType.FireType)
        {
            return BuildingPresetsHandler.Instance.GetBuildingPreset(towerName).buildingCost;
        }
        return BuildingPresetsHandler.Instance.GetBuildingPreset(towerName).buildingCost / 2;
    }
    private void HandleRunEnd(int waveNumber)
    {
        if(waveNumber > waveHighscore)
        {
            waveHighscore = waveNumber;
        }
        metaCoins += waveNumber;
    }
    public void LoadData(GameData data)
    {
        this.metaCoins = data.metaCoins;
        mainMenuManager.UpdateMetaCoins(metaCoins);
        if(data.towerUpgradeStatus.Count <= 0)
        {
            List<BuildingSettings> buildingPresets = BuildingPresetsHandler.Instance.GetAllBuildingPresets();
            towerUpgradeStatus = new List<(string towerName, bool damageUpgraded, bool rangeUpgraded, bool fireRateUpgraded, bool fireTypeUnlocked)>();
            foreach (BuildingSettings building in buildingPresets)
            {
                towerUpgradeStatus.Add((building.towerName, false, false, false, false));
            }
        }
        else
        {
            towerUpgradeStatus = data.towerUpgradeStatus;
        }
    }

    public void SaveData(ref GameData data)
    {
        data.metaCoins = this.metaCoins;
        data.towerUpgradeStatus = this.towerUpgradeStatus;
        data.waveHighscore = this.waveHighscore;
    }
}
public enum UpgradeType
{
    Damage,
    Range,
    FireRate,
    FireType
}
