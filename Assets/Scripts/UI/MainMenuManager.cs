using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private Label metaCoinsAmount;

    [SerializeField] Texture2D upgradePurchasedIcon;
    [SerializeField] VisualTreeAsset upgradesMenuTemplate;
    private VisualElement upgradesWrapper;
    private VisualElement upgradesBackButton;
    private VisualElement upgradesButton;
    private VisualElement mainMenuButtons;
    private VisualElement menuWrapper;
    [SerializeField] VisualTreeAsset towerUpgradesMenuTemplate;
    [SerializeField] VisualTreeAsset towerIconButtonTemplate;
    [SerializeField] List<BuildingSettings> baseTowers;
    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        mainMenuButtons = root.Q<VisualElement>("MainMenuButtons");
        menuWrapper = root.Q<VisualElement>("MenuWrapper");

        VisualElement startButton = root.Q<VisualElement>("StartButton");
        startButton.RegisterCallback<ClickEvent>(OnStartButtonClick);
        
        VisualElement exitButton = root.Q<VisualElement>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(OnExitButtonClick);

        upgradesButton = root.Q<VisualElement>("UpgradesButton");
        upgradesButton.RegisterCallback<ClickEvent>(OnUpgradesMenuClick);

        metaCoinsAmount = root.Q<Label>("MetaCoinsAmount");
    }
    void Start()
    {
        DataPersistenceManager.Instance.LoadGame();
        UpdateMetaCoins(ProgressionManager.Instance.metaCoins);
    }
    public void UpdateMetaCoins(int amount)
    {
        metaCoinsAmount.text = "x " + amount;
    }
    private void OnStartButtonClick(ClickEvent evt)
    {
        StartGame();
    }
    
    private void OnExitButtonClick(ClickEvent evt)
    {
        QuitGame();
    }

    private void OnUpgradesMenuClick(ClickEvent evt)
    {
        menuWrapper.Clear();
        VisualElement upgradesRoot = upgradesMenuTemplate.CloneTree().Q<VisualElement>("UpgradesRoot");
        upgradesWrapper = upgradesRoot.Q<VisualElement>("UpgradesWrapper");
        foreach (BuildingSettings tower in baseTowers)
        {
            VisualElement towerIconButton = towerIconButtonTemplate.CloneTree().Q<VisualElement>("BuildingIcon");
            towerIconButton.style.backgroundImage = new StyleBackground(tower.buildingIcon);
            upgradesWrapper.Add(towerIconButton);
            towerIconButton.RegisterCallback<ClickEvent>(evt => OnSpecificTowerUpgradesClick(evt, tower));
        }
        menuWrapper.Add(upgradesRoot);

        upgradesBackButton = upgradesRoot.Q<VisualElement>("BackButton");
        upgradesBackButton.UnregisterCallback<ClickEvent>(OnUpgradesMenuClick);
        upgradesBackButton.RegisterCallback<ClickEvent>(OnUpgradesBackButtonClick);
    }

    private void OnSpecificTowerUpgradesClick(ClickEvent evt, BuildingSettings tower)
    {
        upgradesWrapper.Clear();
        ProgressionManager.Instance.SetSelectedBuilding(tower);

        VisualElement upgrades = towerUpgradesMenuTemplate.CloneTree();

        VisualElement damageUpgrade = upgrades.Q<VisualElement>("DamageButton");
        if(ProgressionManager.Instance.IsUpgradePurchased(UpgradeType.Damage))
        {
            SetupPurchasedUpgrade(damageUpgrade);
        }
        else
        {
            damageUpgrade.RegisterCallback<ClickEvent>(evt => OnUpgradeButtonClick(evt, UpgradeType.Damage, damageUpgrade));
            upgrades.Q<VisualElement>("DamageUpgrade").Q<Label>("Cost").text = ""+ProgressionManager.Instance.GetUpgradeCost(UpgradeType.Damage, tower.towerName);
        }
        upgradesWrapper.Add(upgrades.Q<VisualElement>("DamageUpgrade"));


        VisualElement rangeUpgrade = upgrades.Q<VisualElement>("RangeButton");
        if(ProgressionManager.Instance.IsUpgradePurchased(UpgradeType.Range))
        {
            SetupPurchasedUpgrade(rangeUpgrade);
        }
        else
        {
            rangeUpgrade.RegisterCallback<ClickEvent>(evt => OnUpgradeButtonClick(evt, UpgradeType.Range, rangeUpgrade));
            upgrades.Q<VisualElement>("RangeUpgrade").Q<Label>("Cost").text = ""+ProgressionManager.Instance.GetUpgradeCost(UpgradeType.Range, tower.towerName);
        }
        upgradesWrapper.Add(upgrades.Q<VisualElement>("RangeUpgrade"));

        VisualElement fireRateUpgrade = upgrades.Q<VisualElement>("FireRateButton");
        if(ProgressionManager.Instance.IsUpgradePurchased(UpgradeType.FireRate))
        {
            SetupPurchasedUpgrade(fireRateUpgrade);
        }
        else
        {
            fireRateUpgrade.RegisterCallback<ClickEvent>(evt => OnUpgradeButtonClick(evt, UpgradeType.FireRate, fireRateUpgrade));
            upgrades.Q<VisualElement>("FireRateUpgrade").Q<Label>("Cost").text = ""+ProgressionManager.Instance.GetUpgradeCost(UpgradeType.FireRate, tower.towerName);
        }
        upgradesWrapper.Add(upgrades.Q<VisualElement>("FireRateUpgrade"));

        VisualElement fireTypeUpgrade = upgrades.Q<VisualElement>("FireButton");
        if(ProgressionManager.Instance.IsUpgradePurchased(UpgradeType.FireType))
        {
            SetupPurchasedUpgrade(fireTypeUpgrade);
        }
        else
        {
            fireTypeUpgrade.RegisterCallback<ClickEvent>(evt => OnUpgradeButtonClick(evt, UpgradeType.FireType, fireTypeUpgrade));
            upgrades.Q<VisualElement>("FireUpgrade").Q<Label>("Cost").text = ""+ProgressionManager.Instance.GetUpgradeCost(UpgradeType.FireType, tower.towerName);
        }
        upgradesWrapper.Add(upgrades.Q<VisualElement>("FireUpgrade"));

        upgradesBackButton.UnregisterCallback<ClickEvent>(OnUpgradesBackButtonClick);
        upgradesBackButton.RegisterCallback<ClickEvent>(OnUpgradesMenuClick);
    }
    private void OnUpgradeButtonClick(ClickEvent evt, UpgradeType upgradeType, VisualElement clickedButton)
    {
        if(ProgressionManager.Instance.UpgradeBuilding(upgradeType))
        {
            SetupPurchasedUpgrade(clickedButton);
        }
    }
    private void SetupPurchasedUpgrade(VisualElement upgradeButton)
    {
        upgradeButton.parent.Q<Label>("Cost").text = "Purchased";
        upgradeButton.style.backgroundImage = new StyleBackground(upgradePurchasedIcon);
        upgradeButton.pickingMode = PickingMode.Ignore;
    }

    private void OnUpgradesBackButtonClick(ClickEvent evt)
    {
        menuWrapper.Clear();
        menuWrapper.Add(mainMenuButtons);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
