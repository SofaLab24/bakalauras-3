using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        
        VisualElement startButton = root.Q<VisualElement>("StartButton");
        startButton.RegisterCallback<ClickEvent>(OnStartButtonClick);
        
        VisualElement exitButton = root.Q<VisualElement>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(OnExitButtonClick);
    }
    
    private void OnStartButtonClick(ClickEvent evt)
    {
        StartGame();
    }
    
    private void OnExitButtonClick(ClickEvent evt)
    {
        QuitGame();
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
