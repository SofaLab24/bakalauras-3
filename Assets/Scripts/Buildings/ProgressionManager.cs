using UnityEngine;
using System.Collections.Generic;

public class ProgressionManager : MonoBehaviour, IGameDataPersistence
{
    private string selectedBuildingName;
    private MainMenuManager mainMenuManager;

    public List<TowerUpgradeStatus> towerUpgradeStatus;
    public int metaCoins;
    public int waveHighscore;

    [Header("Debug")]
    public bool enableDebugCoinCheat = false;

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

    void Update()
    {
        if (enableDebugCoinCheat && Input.GetKeyDown(KeyCode.J))
        {
            metaCoins += 20;
            mainMenuManager.UpdateMetaCoins(metaCoins);
            Debug.Log($"[Debug] Added 20 meta coins. Total: {metaCoins}");
        }
    }

    public void SetSelectedBuilding(BuildingSettings building)
    {
        selectedBuildingName = building.towerName;
    }

    private TowerUpgradeStatus GetSelectedStatus()
    {
        return towerUpgradeStatus.Find(s => s.towerName == selectedBuildingName);
    }

    public bool IsUpgradePurchased(MetaUpgradeType upgradeType)
    {
        return GetSelectedStatus()?.IsUpgradePurchased(upgradeType) ?? false;
    }

    public bool UpgradeBuilding(MetaUpgradeType upgradeType)
    {
        int cost = GetUpgradeCost(upgradeType);
        if (IsUpgradePurchased(upgradeType) || metaCoins < cost)
        {
            return false;
        }
        BuildingPresetsHandler.Instance.UpgradeBuilding(selectedBuildingName, upgradeType);
        GetSelectedStatus().Purchase(upgradeType);
        metaCoins -= cost;
        mainMenuManager.UpdateMetaCoins(metaCoins);
        DataPersistenceManager.Instance.SaveGame();
        return true;
    }

    public int GetUpgradeCost(MetaUpgradeType upgradeType, string towerName = "")
    {
        if (towerName == "") towerName = selectedBuildingName;
        BuildingSettings preset = BuildingPresetsHandler.Instance.GetBuildingPreset(towerName);
        MetaUpgradeDefinition def = preset.metaUpgradeDefinitions.Find(d => d.upgradeType == upgradeType);
        return def?.effect != null ? def.effect.cost : preset.buildingCost / 2;
    }

    private void HandleRunEnd(int waveNumber)
    {
        if (waveNumber > waveHighscore)
        {
            waveHighscore = waveNumber;
        }
        metaCoins += waveNumber;
    }

    public void LoadData(GameData data)
    {
        this.metaCoins = data.metaCoins;
        this.waveHighscore = data.waveHighscore;
        mainMenuManager.UpdateMetaCoins(metaCoins);
        if (data.towerUpgradeStatus.Count <= 0)
        {
            List<BuildingSettings> buildingPresets = BuildingPresetsHandler.Instance.GetAllBuildingPresets();
            towerUpgradeStatus = new List<TowerUpgradeStatus>();
            foreach (BuildingSettings building in buildingPresets)
            {
                towerUpgradeStatus.Add(new TowerUpgradeStatus { towerName = building.towerName });
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

public enum MetaUpgradeType
{
    Damage,
    Range,
    FireRate,
    PoisonType,
    SlowPercent,
    SlowDuration,
    UnlockTower
}

[System.Serializable]
public class TowerUpgradeStatus
{
    public string towerName;
    public List<MetaUpgradeType> purchasedUpgrades = new List<MetaUpgradeType>();

    public bool IsUpgradePurchased(MetaUpgradeType type) => purchasedUpgrades.Contains(type);
    public void Purchase(MetaUpgradeType type) => purchasedUpgrades.Add(type);
}
