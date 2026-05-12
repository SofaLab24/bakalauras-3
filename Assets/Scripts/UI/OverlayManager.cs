using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class OverlayManager : MonoBehaviour
{
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private PlayerEconomyManager economyManager;
    [SerializeField] private BaseManager baseManager;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float splitChance = 0.3f;
    [Header("UI Templates")]
    [SerializeField] private VisualTreeAsset buildingIconTemplate;
    [SerializeField] private VisualTreeAsset escMenuTemplate;
    [SerializeField] private VisualTreeAsset towerUpgradeMenuTemplate;
    [SerializeField] private VisualTreeAsset settingsMenuTemplate;
    [Header("Building Buttons")]
    [SerializeField] private Color unselectedColor;
    [SerializeField] private Color selectedColor;

    public static event Action<bool> OnEscMenu;
    private bool isEscMenuOpen = false;
    private bool isTowerUpgradeMenuOpen = false;

    private VisualElement buildingsWrapper;
    private VisualElement escMenuWrapper;
    private VisualElement escMenuButton;
    private VisualElement nextWaveButton;
    private VisualElement gameSpeedButton;
    private Label gameSpeedLabel;
    private VisualElement towerUpgradeWrapper;
    private Label moneyText;
    private VisualElement healthBarFill;
    private Label healthText;
    private Label currentWaveLabel;
    private Label previewTextBox;
    private Label previewTitle;
    private Dictionary<string, int> waveEnemyCounts;
    private BaseTower selectedTower;
    private bool isDoubleSpeed = false;

    private void OnEnable()
    {
        PlayerEconomyManager.OnMoneyChanged += UpdateMoneyDisplay;
        BaseManager.OnBaseHealthChange += HandleHealthChanged;
        WaveManager.OnWaveCompleted += OnWaveCompleted;
        WaveManager.OnWaveRestored += OnWaveRestored;
        WaveManager.OnWaveStarted += HandleWaveStartedPreview;
        WaveManager.OnWavePoolGenerated += HandleWavePoolGenerated;
        EnemyHealthManager.OnEnemyDeath += HandleEnemyDeathPreview;
        BuildingManager.OnTowerClicked += OpenTowerUpgradeMenu;
        BuildingManager.OnBuildingDeselected += ClearSelectedBuildingIcon;
    }

    private void OnDisable()
    {
        PlayerEconomyManager.OnMoneyChanged -= UpdateMoneyDisplay;
        BaseManager.OnBaseHealthChange -= HandleHealthChanged;
        WaveManager.OnWaveCompleted -= OnWaveCompleted;
        WaveManager.OnWaveRestored -= OnWaveRestored;
        WaveManager.OnWaveStarted -= HandleWaveStartedPreview;
        WaveManager.OnWavePoolGenerated -= HandleWavePoolGenerated;
        EnemyHealthManager.OnEnemyDeath -= HandleEnemyDeathPreview;
        BuildingManager.OnTowerClicked -= OpenTowerUpgradeMenu;
        BuildingManager.OnBuildingDeselected -= ClearSelectedBuildingIcon;
    }

    // Start is called before the first frame update
    void Start()
    {
        VisualElement root = uiDocument.rootVisualElement;
        nextWaveButton = root.Q<VisualElement>("NextWaveButton");
        nextWaveButton.RegisterCallback<ClickEvent>(OnNextWaveButtonClicked);

        moneyText = root.Q<Label>("MoneyText");
        healthBarFill = root.Q<VisualElement>("HealthBarFill");
        healthText = root.Q<Label>("HealthText");
        
        // Initialize money and health
        UpdateMoneyDisplay(economyManager != null ? economyManager.playerMoney : 0);
        UpdateHealthBar(baseManager.GetCurrentHealth(), baseManager.GetMaxHealth());

        // Initialize buildings icons
        buildingsWrapper = root.Q<VisualElement>("BuildingsWrapper");
        InitializeBuildingsIcons();

        escMenuWrapper = root.Q<VisualElement>("EscMenuWrapper");
        towerUpgradeWrapper = root.Q<VisualElement>("TowerUpgradeWrapper");
        escMenuButton = root.Q<VisualElement>("EscButton");
        escMenuButton.RegisterCallback<ClickEvent>(OnEscMenuButtonClicked);

        gameSpeedButton = root.Q<VisualElement>("GameSpeedButton");
        gameSpeedLabel = gameSpeedButton.Q<Label>("Text");
        gameSpeedLabel.text = "1X";
        gameSpeedButton.RegisterCallback<ClickEvent>(OnGameSpeedButtonClicked);

        currentWaveLabel = root.Q<Label>("CurrentWave");

        previewTextBox = root.Q<Label>("PreviewTextBox");
        previewTitle   = root.Q<Label>("PreviewTitle");

        waveManager = FindObjectOfType<WaveManager>();
        UpdateCurrentWave(waveManager.waveNumber);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isTowerUpgradeMenuOpen)
            {
                CloseTowerUpgradeMenu();
            }
            else if (isEscMenuOpen)
            {
                CloseEscMenu();
            }
            else
            {
                OpenEscMenu();
                OnEscMenu?.Invoke(true);
            }
        }
    }
    public void UpdateCurrentWave(int waveNumber)
    {
        currentWaveLabel.text = "Current wave: " + waveNumber;
    }
    public void OpenEscMenu()
    {
        if (isTowerUpgradeMenuOpen)
        {
            CloseTowerUpgradeMenu();
        }
        escMenuWrapper.Clear();
        escMenuTemplate.CloneTree(escMenuWrapper);
        isEscMenuOpen = true;
        Time.timeScale = 0f;
        EscMenuButtonSetup();
    }
    public void CloseEscMenu()
    {
        escMenuWrapper.Clear();
        OnEscMenu?.Invoke(false);
        isEscMenuOpen = false;
        Time.timeScale = isDoubleSpeed ? 2f : 1f;
    }

    private void EscMenuButtonSetup()
    {
        VisualElement resumeButton = escMenuWrapper.Q<VisualElement>("ResumeButton");
        resumeButton.RegisterCallback<ClickEvent>(OnResumeClicked);

        VisualElement settingsButton = escMenuWrapper.Q<VisualElement>("SettingsButton");
        settingsButton.RegisterCallback<ClickEvent>(OnSettingsClicked);

        VisualElement mainMenuButton = escMenuWrapper.Q<VisualElement>("MainMenuButton");
        mainMenuButton.RegisterCallback<ClickEvent>(OnMainMenuClicked);
    }
    private void OnEscMenuButtonClicked(ClickEvent evt)
    {
        OnEscMenu?.Invoke(true);
        OpenEscMenu();
    }

    private void OnGameSpeedButtonClicked(ClickEvent evt)
    {
        isDoubleSpeed = !isDoubleSpeed;
        Time.timeScale = isDoubleSpeed ? 2f : 1f;
        gameSpeedLabel.text = isDoubleSpeed ? "2X" : "1X";
    }
    private void OnResumeClicked(ClickEvent evt)
    {
        CloseEscMenu();
    }
    private void OnSettingsClicked(ClickEvent evt)
    {
        escMenuWrapper.Clear();
        settingsMenuTemplate.CloneTree(escMenuWrapper);

        SliderInt musicSlider = escMenuWrapper.Q<SliderInt>("MusicSlider");
        musicSlider.value = SFXManager.Instance.musicVolume;
        musicSlider.RegisterCallback<ChangeEvent<int>>(OnMusicSliderChanged);

        SliderInt sfxSlider = escMenuWrapper.Q<SliderInt>("SFXSlider");
        sfxSlider.value = SFXManager.Instance.sfxVolume;
        sfxSlider.RegisterCallback<ChangeEvent<int>>(OnSFXSliderChanged);

        VisualElement backButton = escMenuWrapper.Q<VisualElement>("BackButton");
        backButton.RegisterCallback<ClickEvent>(OnSettingsBackButtonClicked);
    }

    private void OnMusicSliderChanged(ChangeEvent<int> evt)
    {
        SFXManager.Instance.SetMusicVolume(evt.newValue);
    }

    private void OnSFXSliderChanged(ChangeEvent<int> evt)
    {
        SFXManager.Instance.sfxVolume = evt.newValue;
    }

    private void OnSettingsBackButtonClicked(ClickEvent evt)
    {
        OpenEscMenu();
    }
    private void OnMainMenuClicked(ClickEvent evt)
    {
        Time.timeScale = 1f;
        DataPersistenceManager.Instance.SaveRun();
        DataPersistenceManager.Instance.SaveGame();
        SceneManager.LoadScene(0);
    }
    private void InitializeBuildingsIcons()
    {
        buildingsWrapper.Clear();
        foreach (var building in BuildingPresetsHandler.Instance.GetAllBuildingPresets())
        {
            if (!building.isUnlocked) continue;
            VisualElement buttonParent = buildingIconTemplate.CloneTree().Q<VisualElement>("TowerSelectButton");
            VisualElement buildingIcon = buttonParent.Q<VisualElement>("BuildingIcon");
            buildingIcon.style.backgroundImage = new StyleBackground(building.buildingIcon);
            buttonParent.Q<Label>("BuildingCost").text = building.buildingCost.ToString();
            buttonParent.RegisterCallback<ClickEvent>(evt => OnBuildingIconClicked(building, buildingIcon));
            buildingsWrapper.Add(buttonParent);
        }
    }

    private void OnNextWaveButtonClicked(ClickEvent evt)
    {
        if (!isEscMenuOpen)
        {
            waveManager.StartWave();
            nextWaveButton.visible = false;
        }
    }
    private void OnWaveCompleted(int waveNumber)
    {
        waveManager.PrepareNextWave(splitChance);
        nextWaveButton.visible = true;
        UpdateCurrentWave(waveManager.waveNumber);
    }

    private void OnWaveRestored(int waveNumber)
    {
        nextWaveButton.visible = true;
        UpdateCurrentWave(waveNumber);
    }
    private void OnBuildingIconClicked(BuildingSettings buildingSettings, VisualElement buildingButton)
    {
        buildingManager.SelectBuilding(buildingSettings);
        UpdateSelectedBuildingIcon(buildingButton);
    }
    private void UpdateSelectedBuildingIcon(VisualElement buildingButton)
    {
        // Remove border from the other building buttons
        foreach (VisualElement button in buildingsWrapper.Children())
        {
            button.style.borderBottomColor = unselectedColor;
            button.style.borderLeftColor = unselectedColor;
            button.style.borderRightColor = unselectedColor;
            button.style.borderTopColor = unselectedColor;
        }
        // Add border to the selected building button
        buildingButton.style.borderBottomColor = selectedColor;
        buildingButton.style.borderLeftColor = selectedColor;
        buildingButton.style.borderRightColor = selectedColor;
        buildingButton.style.borderTopColor = selectedColor;
    }
    private void ClearSelectedBuildingIcon()
    {
        if (buildingsWrapper == null) return;
        foreach (VisualElement button in buildingsWrapper.Children())
        {
            button.style.borderBottomColor = unselectedColor;
            button.style.borderLeftColor = unselectedColor;
            button.style.borderRightColor = unselectedColor;
            button.style.borderTopColor = unselectedColor;
        }
    }
    private void UpdateMoneyDisplay(int currentMoney)
    {
        if (moneyText != null)
        {
            string formattedMoney = FormatMoneyWithSpaces(currentMoney);
            moneyText.text = "x " + formattedMoney;
        }
    }
    
    // Handler for the health changed event
    private void HandleHealthChanged(int currentHealth)
    {
        UpdateHealthBar(currentHealth, baseManager.GetMaxHealth());
    }
    
    // Method to update the health bar fill and text based on current and max health
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        float healthPercentage = Mathf.Clamp01((float)currentHealth / maxHealth) * 100f;
        healthBarFill.style.width = new StyleLength(new Length(healthPercentage, LengthUnit.Percent));
        healthText.text = $"{currentHealth} / {maxHealth}";
    }

    private void OpenTowerUpgradeMenu(BaseTower tower)
    {
        if (isEscMenuOpen) return;

        selectedTower = tower;
        towerUpgradeWrapper.Clear();
        towerUpgradeMenuTemplate.CloneTree(towerUpgradeWrapper);
        isTowerUpgradeMenuOpen = true;
        OnEscMenu?.Invoke(true);

        Label towerNameLabel = towerUpgradeWrapper.Q<Label>("TowerName");
        if (towerNameLabel != null)
        {
            towerNameLabel.text = $"{tower.TowerName} Upgrades";
        }

        Label statsTextBox = towerUpgradeWrapper.Q<Label>("StatsTextBox");
        if (statsTextBox != null)
        {
            statsTextBox.text = tower.GetStatsText();
        }

        Action refreshStats = () =>
        {
            if (statsTextBox != null)
                statsTextBox.text = tower.GetStatsText();
        };

        new UpgradeSlot(towerUpgradeWrapper, "DamageLabel", "DamageButton")
            .WithName(() => "Damage")
            .WithCost(() => tower.DamageCost)
            .WithBoughtState(() => tower.DamageUpgraded)
            .OnPurchase(() => tower.TryUpgradeDamage(economyManager))
            .AfterPurchase(refreshStats)
            .Bind();

        new UpgradeSlot(towerUpgradeWrapper, "SpecialtyLabel", "SpecialtyButton")
            .WithName(tower.GetSpecialtyName)
            .WithCost(() => tower.SpecialtyCost)
            .WithBoughtState(() => tower.SpecialtyUpgraded)
            .OnPurchase(() => tower.TryUpgradeSpecialty(economyManager))
            .AfterPurchase(refreshStats)
            .Bind();

        VisualElement closeButton = towerUpgradeWrapper.Q<VisualElement>("CloseButton");
        closeButton.RegisterCallback<ClickEvent>(OnTowerUpgradeCloseClicked);
    }

    private void CloseTowerUpgradeMenu()
    {
        if (selectedTower != null)
        {
            selectedTower.ToggleRangeIndicator(true);
        }
        towerUpgradeWrapper.Clear();
        selectedTower = null;
        isTowerUpgradeMenuOpen = false;
        OnEscMenu?.Invoke(false);
    }

    private void OnTowerUpgradeCloseClicked(ClickEvent evt)
    {
        CloseTowerUpgradeMenu();
    }

    private void HandleWavePoolGenerated(Dictionary<string, int> composition)
    {
        waveEnemyCounts = new Dictionary<string, int>(composition);
        if (previewTitle != null) previewTitle.text = "Next Wave:";
        UpdatePreviewLabel();
    }

    private void HandleWaveStartedPreview(int _)
    {
        if (previewTitle != null) previewTitle.text = "Enemies Left:";
    }

    private void HandleEnemyDeathPreview(EnemyHealthManager enemy, EnemyHealthManager.DeathReason reason)
    {
        if (waveEnemyCounts == null) return;
        string typeId = enemy.GetComponent<EnemyController>()?.TypeId;
        if (string.IsNullOrEmpty(typeId) || !waveEnemyCounts.ContainsKey(typeId)) return;

        waveEnemyCounts[typeId]--;
        if (waveEnemyCounts[typeId] <= 0)
            waveEnemyCounts.Remove(typeId);

        UpdatePreviewLabel();
    }

    private void UpdatePreviewLabel()
    {
        if (previewTextBox == null) return;
        if (waveEnemyCounts == null || waveEnemyCounts.Count == 0)
        {
            previewTextBox.text = "—";
            return;
        }

        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<string, int> entry in waveEnemyCounts)
            sb.AppendLine($"{entry.Key}: {entry.Value}");

        previewTextBox.text = sb.ToString().TrimEnd();
    }

    private string FormatMoneyWithSpaces(int money)
    {
        string moneyStr = money.ToString();
        
        if (moneyStr.Length <= 3)
        {
            return moneyStr;
        }

        string result = "";
        int counter = 0;
        
        // Loop through the digits from right to left
        for (int i = moneyStr.Length - 1; i >= 0; i--)
        {
            result = moneyStr[i] + result;
            counter++;
            
            // Add a space after every third digit, but not at the beginning
            if (counter % 3 == 0 && i > 0)
            {
                result = " " + result;
            }
        }
        
        return result;
    }
}
