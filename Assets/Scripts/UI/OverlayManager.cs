using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private VisualElement buildingsWrapper;

    private VisualElement nextWaveButton;
    private Label moneyText;
    private VisualElement healthBarFill;
    private Label healthText;

    private void OnEnable()
    {
        // Subscribe to the money changed event
        EconomyManager.OnMoneyChanged += UpdateMoneyDisplay;
        
        // Subscribe to the health changed event
        BaseManager.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from the money changed event
        EconomyManager.OnMoneyChanged -= UpdateMoneyDisplay;
        
        // Unsubscribe from the health changed event
        BaseManager.OnHealthChanged -= HandleHealthChanged;
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
        waveManager.StartNextWave(splitChance);
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
