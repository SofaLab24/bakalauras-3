using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    [SerializeField] private string saveFileName;
    private FileDataHandler dataHandler;
    private GameData gameData;
    // need to serialize/deserialize these differently
    private List<BuildingSettings> buildingPresets;
    private List<IDataPersistence> dataPersistenceObjects;
    private List<ISettingsPersistence> settingsPersistenceObjects;

    public static DataPersistenceManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, saveFileName);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        this.settingsPersistenceObjects = FindAllSettingsPersistenceObjects();
        LoadGame();
    }
    void OnApplicationQuit()
    {
        SaveGame();
    }

    public void NewGame()
    {
        gameData = new GameData();
    }
    public void LoadGame()
    {
        gameData = dataHandler.Load();
        buildingPresets = dataHandler.LoadSettings();
        Debug.Log("Loaded " + buildingPresets.Count + " building presets");
        if (gameData == null)
        {
            Debug.Log("No saves found. Creating new game...");
            NewGame();
        }
        // load settings
        foreach (ISettingsPersistence settingsPersistenceObj in settingsPersistenceObjects)
        {
            settingsPersistenceObj.LoadSettings(buildingPresets);
        }
        // load data (towers reference settings)
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }

    }
    public void SaveGame()
    {
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }
        List<BuildingSettings> buildingPresetsToSave = new List<BuildingSettings>();
        foreach (ISettingsPersistence settingsPersistenceObj in settingsPersistenceObjects)
        {
            buildingPresetsToSave = settingsPersistenceObj.SaveSettings();
        }
        dataHandler.Save(gameData);
        dataHandler.SaveSettings(buildingPresetsToSave);
    }
    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
    private List<ISettingsPersistence> FindAllSettingsPersistenceObjects()
    {
        IEnumerable<ISettingsPersistence> settingsPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<ISettingsPersistence>();
        return new List<ISettingsPersistence>(settingsPersistenceObjects);
    }
}
