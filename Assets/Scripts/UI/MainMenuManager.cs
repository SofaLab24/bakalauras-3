using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private Label metaCoinsAmount;

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
        upgradesButton.RegisterCallback<ClickEvent>(OnUpgradesButtonClick);

        metaCoinsAmount = root.Q<Label>("MetaCoinsAmount");
    }
    public void UpdateMetaCoins(int amount)
    {
        Debug.Log("Updating meta coins: " + amount);
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

    private void OnUpgradesButtonClick(ClickEvent evt)
    {
        menuWrapper.Clear();
        VisualElement upgradesRoot = upgradesMenuTemplate.CloneTree().Q<VisualElement>("UpgradesRoot");
        upgradesWrapper = upgradesRoot.Q<VisualElement>("UpgradesWrapper");
        foreach (BuildingSettings tower in baseTowers)
        {
            VisualElement towerIconButton = towerIconButtonTemplate.CloneTree().Q<VisualElement>("BuildingIcon");
            towerIconButton.style.backgroundImage = new StyleBackground(tower.buildingIcon);
            upgradesWrapper.Add(towerIconButton);
            towerIconButton.RegisterCallback<ClickEvent>(OnTowerIconButtonClick);
        }
        menuWrapper.Add(upgradesRoot);

        upgradesBackButton = upgradesRoot.Q<VisualElement>("BackButton");
        upgradesBackButton.RegisterCallback<ClickEvent>(OnUpgradesBackButtonClick);
    }

    private void OnTowerIconButtonClick(ClickEvent evt)
    {
        upgradesWrapper.Clear();
        upgradesBackButton.UnregisterCallback<ClickEvent>(OnUpgradesBackButtonClick);
        upgradesBackButton.RegisterCallback<ClickEvent>(OnUpgradesButtonClick);
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
