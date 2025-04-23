using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements; // Add UIElements namespace

public class OverlayManager : MonoBehaviour
{
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private BaseManager baseManager;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float splitChance = 0.3f;
    [SerializeField] private VisualTreeAsset buildingIconTemplate;
    [SerializeField] private VisualTreeAsset escMenuTemplate;

    public static event Action<bool> OnEscMenu;
    private bool isEscMenuOpen = false;

    private VisualElement buildingsWrapper;
    private VisualElement escMenuWrapper;
    private VisualElement escMenuButton;
    private VisualElement nextWaveButton;
    private Label moneyText;
    private VisualElement healthBarFill;
    private Label healthText;

    private void OnEnable()
    {
        EconomyManager.OnMoneyChanged += UpdateMoneyDisplay;
        BaseManager.OnHealthChanged += HandleHealthChanged;
        WaveManager.OnWaveCompleted += OnWaveCompleted;
    }

    private void OnDisable()
    {
        EconomyManager.OnMoneyChanged -= UpdateMoneyDisplay;
        BaseManager.OnHealthChanged -= HandleHealthChanged;
        WaveManager.OnWaveCompleted -= OnWaveCompleted;
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
        escMenuButton = root.Q<VisualElement>("EscButton");
        escMenuButton.RegisterCallback<ClickEvent>(OnEscMenuButtonClicked);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isEscMenuOpen)
            {
                CloseEscMenu();
            }
            else
            {
                OpenEscMenu();
            }
        }
    }
    public void OpenEscMenu()
    {
        escMenuWrapper.Clear();
        VisualElement escMenu = escMenuTemplate.CloneTree();
        escMenuWrapper.Add(escMenu);
        OnEscMenu?.Invoke(true);
        isEscMenuOpen = true;
        EscMenuButtonSetup();
    }
    public void CloseEscMenu()
    {
        escMenuWrapper.Clear();
        OnEscMenu?.Invoke(false);
        isEscMenuOpen = false;
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
        OpenEscMenu();
    }
    private void OnResumeClicked(ClickEvent evt)
    {
        CloseEscMenu();
    }
    private void OnSettingsClicked(ClickEvent evt)
    {
        // TODO: show settings UI
    }
    private void OnMainMenuClicked(ClickEvent evt)
    {
        SceneManager.LoadScene(0);
    }
    private void InitializeBuildingsIcons()
    {
        buildingsWrapper.Clear();
        VisualElement buildingIcon = buildingIconTemplate.CloneTree().Q<VisualElement>("BuildingIcon");
        foreach (var building in buildingManager.availableBuildings)
        {
            buildingIcon.style.backgroundImage = new StyleBackground(building.buildingIcon);
            buildingIcon.Q<Label>("BuildingCost").text = building.buildingCost.ToString();
            buildingIcon.RegisterCallback<ClickEvent, BuildingSettings>(OnBuildingIconClicked, building);
            buildingsWrapper.Add(buildingIcon);
            buildingIcon = buildingIconTemplate.CloneTree().Q<VisualElement>("BuildingIcon");
        }

    }

    private void OnNextWaveButtonClicked(ClickEvent evt)
    {
        if (!isEscMenuOpen)
        {
            waveManager.StartNextWave(splitChance);
            nextWaveButton.visible = false;
        }
    }
    private void OnWaveCompleted(int waveNumber)
    {
        nextWaveButton.visible = true;
        // TODO: show wave completed UI
    }
    private void OnBuildingIconClicked(ClickEvent evt, BuildingSettings buildingSettings)
    {
        buildingManager.SelectBuilding(buildingSettings);
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

    // Helper method to format money with spaces for thousands
    private string FormatMoneyWithSpaces(int money)
    {
        // Convert to string first
        string moneyStr = money.ToString();
        
        // If the number is less than 1000, no formatting needed
        if (moneyStr.Length <= 3)
        {
            return moneyStr;
        }

        // Add spaces for every thousand
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
