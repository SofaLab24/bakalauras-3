using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements; // Add UIElements namespace

public class OverlayManager : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager; // Reference to WaveManager
    [SerializeField] private EconomyManager economyManager; // Reference to EconomyManager
    [SerializeField] private BaseManager baseManager; // Reference to BaseManager

    [SerializeField] private UIDocument uiDocument; // Reference to UI Document
    [SerializeField] private float splitChance = 0.3f; // Default split chance value

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
        
        // Initialize money display with current value
        UpdateMoneyDisplay(economyManager != null ? economyManager.playerMoney : 0);
        
        // Initialize health bar with current value if BaseManager is available
        UpdateHealthBar(baseManager.GetCurrentHealth(), baseManager.GetMaxHealth());
    }

    // Method to handle the NextWaveButton click
    private void OnNextWaveButtonClicked(ClickEvent evt)
    {
        waveManager.StartNextWave(splitChance);
    }

    private void UpdateMoneyDisplay(int currentMoney)
    {
        if (moneyText != null)
        {
            // Format the money value with spaces for thousands
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
