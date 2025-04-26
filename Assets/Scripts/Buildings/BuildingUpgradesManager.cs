using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class BuildingUpgradesManager : MonoBehaviour, IDataPersistence
{
    private BuildingSettings selectedBuilding;
    private int selectedBuildingIndex;
    private MainMenuManager mainMenuManager;

    public List<(string towerName, bool damageUpgraded, bool rangeUpgraded, bool fireRateUpgraded)> towerUpgradeStatus;
    public int metaCoins;

    void Start()
    {
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        mainMenuManager.UpdateMetaCoins(metaCoins);
    }
    public void SetSelectedBuilding(BuildingSettings building)
    {
        selectedBuilding = building;
        selectedBuildingIndex = towerUpgradeStatus.FindIndex(upgrade => upgrade.towerName == selectedBuilding.name);
    }
    public void UpgradeDamage()
    {
        int cost = GetUpgradeCost();
        if (metaCoins < cost)
        {
            return;
        }
        selectedBuilding.towerDamage = selectedBuilding.towerDamage * 2;
        var upgradeStatus = towerUpgradeStatus[selectedBuildingIndex];
        upgradeStatus.damageUpgraded = true;
        towerUpgradeStatus[selectedBuildingIndex] = upgradeStatus;
        metaCoins -= cost;
        mainMenuManager.UpdateMetaCoins(metaCoins);
    }
    public void UpgradeRange()
    {
        int cost = GetUpgradeCost();
        if (metaCoins < cost)
        {
            return;
        }
        selectedBuilding.towerRange = selectedBuilding.towerRange * 2;
        var upgradeStatus = towerUpgradeStatus[selectedBuildingIndex];
        upgradeStatus.rangeUpgraded = true;
        towerUpgradeStatus[selectedBuildingIndex] = upgradeStatus;
        metaCoins -= cost;
        mainMenuManager.UpdateMetaCoins(metaCoins);
    }
    public void UpgradeFireRate()
    {
        int cost = GetUpgradeCost();
        if (metaCoins < cost)
        {
            return;
        }
        selectedBuilding.towerShootingDelay = selectedBuilding.towerShootingDelay / 2;
        var upgradeStatus = towerUpgradeStatus[selectedBuildingIndex];
        upgradeStatus.fireRateUpgraded = true;
        towerUpgradeStatus[selectedBuildingIndex] = upgradeStatus;
        metaCoins -= cost;
        mainMenuManager.UpdateMetaCoins(metaCoins);
    }
    public void UnlockFireType()
    {
        int cost = GetUpgradeCost(true);
        if (metaCoins < cost)
        {
            return;
        }
        BuildingPresetsHandler.Instance.UnlockFireType(selectedBuilding.name);
        metaCoins -= cost;
        mainMenuManager.UpdateMetaCoins(metaCoins);
    }
    public int GetUpgradeCost(bool fireTypeUpgrade = false)
    {
        if (fireTypeUpgrade)
        {
            return selectedBuilding.buildingCost;
        }
        return selectedBuilding.buildingCost / 2;
    }
    public void LoadData(GameData data)
    {
        this.metaCoins = data.metaCoins;
        if(data.towerUpgradeStatus.Count <= 0)
        {
            List<BuildingSettings> buildingPresets = BuildingPresetsHandler.Instance.GetAllBuildingPresets();
            towerUpgradeStatus = new List<(string towerName, bool damageUpgraded, bool rangeUpgraded, bool fireRateUpgraded)>();
            foreach (BuildingSettings building in buildingPresets)
            {
                towerUpgradeStatus.Add((building.name, false, false, false));
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
    }
}
