using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements; // Add UIElements namespace

public class OverlayManager : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager; // Reference to WaveManager
    [SerializeField] private EconomyManager economyManager; // Reference to EconomyManager

    private UIDocument uiDocument; // Reference to UI Document
    [SerializeField] private float splitChance = 0.3f; // Default split chance value

    private VisualElement nextWaveButton;
    private Label moneyText;

    private void OnEnable()
    {
        // Subscribe to the money changed event
        EconomyManager.OnMoneyChanged += UpdateMoneyDisplay;
    }

    private void OnDisable()
    {
        // Unsubscribe from the money changed event
        EconomyManager.OnMoneyChanged -= UpdateMoneyDisplay;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument != null)
        {
            // Get the root element of the UI
            VisualElement root = uiDocument.rootVisualElement;
            
            // Find the NextWaveButton
            nextWaveButton = root.Q<VisualElement>("NextWaveButton");
            
            if (nextWaveButton != null)
            {
                // Register the click event handler
                nextWaveButton.RegisterCallback<ClickEvent>(OnNextWaveButtonClicked);
            }
            else
            {
                Debug.LogError("NextWaveButton not found in UI");
            }

            // Find the MoneyText label
            moneyText = root.Q<Label>("MoneyText");
            if (moneyText == null)
            {
                Debug.LogError("MoneyText not found in UI");
            }
        }
        else
        {
            Debug.LogError("UIDocument component is missing");
        }

        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
            if (waveManager == null)
            {
                Debug.LogError("WaveManager not found in scene");
            }
        }

        if (economyManager == null)
        {
            economyManager = FindObjectOfType<EconomyManager>();
            if (economyManager == null)
            {
                Debug.LogError("EconomyManager not found in scene");
            }
        }

        // Initialize money display with current value
        UpdateMoneyDisplay(economyManager != null ? economyManager.playerMoney : 0);
    }

    // Method to handle the NextWaveButton click
    private void OnNextWaveButtonClicked(ClickEvent evt)
    {
        if (waveManager != null)
        {
            waveManager.StartNextWave(splitChance);
        }
        else
        {
            Debug.LogError("WaveManager reference is missing");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Money display is now updated via events, no need to poll every frame
    }

    // Method to update the money display with proper formatting
    // Now accepts current money value from the event
    private void UpdateMoneyDisplay(int currentMoney)
    {
        if (moneyText != null)
        {
            // Format the money value with spaces for thousands
            string formattedMoney = FormatMoneyWithSpaces(currentMoney);
            moneyText.text = "x " + formattedMoney;
        }
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
