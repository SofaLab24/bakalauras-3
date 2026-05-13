using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private Label metaCoinsAmount;
    private UIUtils uiUtils;
    [SerializeField] List<Texture2D> upgradeIconsFrames;
    [SerializeField] float animationDelayBetweenFrames = 0.33f;
    [SerializeField] Texture2D upgradePurchasedIcon;
    [SerializeField] VisualTreeAsset upgradesMenuTemplate;
    [SerializeField] VisualTreeAsset settingsMenuTemplate;
    private VisualElement upgradesWrapper;
    private VisualElement upgradesBackButton;
    private VisualElement upgradesButton;
    private VisualElement settingsButton;
    private VisualElement mainMenuButtons;
    private VisualElement menuWrapper;
    private Label highscore;
    [SerializeField] VisualTreeAsset towerIconButtonTemplate;
    [SerializeField] VisualTreeAsset mainMenuUpgradeSlotTemplate;
    [SerializeField] List<BuildingSettings> baseTowers;
    private void Awake()
    {
        uiUtils = GetComponent<UIUtils>();
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

        settingsButton = root.Q<VisualElement>("SettingsButton");
        settingsButton.RegisterCallback<ClickEvent>(OnSettingsButtonClick);

        metaCoinsAmount = root.Q<Label>("MetaCoinsAmount");
        highscore = root.Q<Label>("Highscore");
    }
    void Start()
    {
        DataPersistenceManager.Instance.LoadGame();
        UpdateMetaCoins(ProgressionManager.Instance.metaCoins);
        UpdateHighscore(ProgressionManager.Instance.waveHighscore);
    }
    public void UpdateMetaCoins(int amount)
    {
        metaCoinsAmount.text = "x " + amount;
    }
    public void UpdateHighscore(int amount)
    {
        highscore.text = "Highscore: " + amount;
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

        BuildingSettings runtimePreset = BuildingPresetsHandler.Instance.GetBuildingPreset(tower.towerName);
        bool isLocked = !runtimePreset.isUnlocked;

        foreach (MetaUpgradeDefinition def in tower.metaUpgradeDefinitions)
        {
            bool isUnlockDef = def.upgradeType == MetaUpgradeType.UnlockTower;
            if (isLocked && !isUnlockDef) continue;

            VisualElement slot = mainMenuUpgradeSlotTemplate.CloneTree().Q<VisualElement>("UpgradeSlot");
            slot.Q<Label>("UpgradeName").text = def.upgradeType.ToString();
            slot.Q<Label>("Cost").text = def.effect != null ? "" + def.effect.cost : "";
            VisualElement btn = slot.Q<VisualElement>("UpgradeButton");
            if (ProgressionManager.Instance.IsUpgradePurchased(def.upgradeType))
            {
                SetupPurchasedUpgrade(btn);
            }
            else
            {
                MetaUpgradeDefinition captured = def;
                string animKey = btn.name + def.upgradeType;
                btn.RegisterCallback<ClickEvent>(e =>
                {
                    if (ProgressionManager.Instance.UpgradeBuilding(captured.upgradeType))
                    {
                        SetupPurchasedUpgrade(btn, animKey);
                    }
                });
                uiUtils.AnimateIcon(animKey, btn, upgradeIconsFrames, animationDelayBetweenFrames);
            }
            upgradesWrapper.Add(slot);
        }

        upgradesBackButton.UnregisterCallback<ClickEvent>(OnUpgradesBackButtonClick);
        upgradesBackButton.RegisterCallback<ClickEvent>(OnUpgradesMenuClick);
    }

    private void SetupPurchasedUpgrade(VisualElement upgradeButton, string coroutineRef = null)
    {
        if (coroutineRef != null)
            uiUtils.StopCoroutineByReference(coroutineRef);
        upgradeButton.parent.Q<Label>("Cost").text = "Purchased";
        upgradeButton.style.backgroundImage = new StyleBackground(upgradePurchasedIcon);
        upgradeButton.pickingMode = PickingMode.Ignore;
    }

    private void OnUpgradesBackButtonClick(ClickEvent evt)
    {
        menuWrapper.Clear();
        menuWrapper.Add(mainMenuButtons);
    }

    private void OnSettingsButtonClick(ClickEvent evt)
    {
        menuWrapper.Clear();
        settingsMenuTemplate.CloneTree(menuWrapper);

        SliderInt musicSlider = menuWrapper.Q<SliderInt>("MusicSlider");
        musicSlider.value = SFXManager.Instance.musicVolume;
        musicSlider.RegisterCallback<ChangeEvent<int>>(OnMusicSliderChanged);

        SliderInt sfxSlider = menuWrapper.Q<SliderInt>("SFXSlider");
        sfxSlider.value = SFXManager.Instance.sfxVolume;
        sfxSlider.RegisterCallback<ChangeEvent<int>>(OnSFXSliderChanged);

        VisualElement backButton = menuWrapper.Q<VisualElement>("BackButton");
        backButton.RegisterCallback<ClickEvent>(OnSettingsBackButtonClick);
    }

    private void OnMusicSliderChanged(ChangeEvent<int> evt)
    {
        SFXManager.Instance.SetMusicVolume(evt.newValue);
    }

    private void OnSFXSliderChanged(ChangeEvent<int> evt)
    {
        SFXManager.Instance.sfxVolume = evt.newValue;
    }

    private void OnSettingsBackButtonClick(ClickEvent evt)
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
