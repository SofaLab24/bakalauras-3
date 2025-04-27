using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    [SerializeField] private string saveFileName;
    [SerializeField] private string runDataFilePrefix;
    [SerializeField] private string gameDataFilePrefix;
    [SerializeField] private string settingsFilePrefix;
    private FileDataHandler dataHandler;
    private RunData runData;
    private GameData gameData;
    // need to serialize/deserialize these differently
    private List<BuildingSettings> buildingPresets;
    private List<IRunDataPersistence> runDataPersistenceObjects;
    private List<IGameDataPersistence> gameDataPersistenceObjects;
    private List<ISettingsPersistence> settingsPersistenceObjects;

    public static DataPersistenceManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, saveFileName);
        this.runDataPersistenceObjects = FindAllRunDataPersistenceObjects();
        this.gameDataPersistenceObjects = FindAllGameDataPersistenceObjects();
        this.settingsPersistenceObjects = FindAllSettingsPersistenceObjects();
    }
    void OnApplicationQuit()
    {
        SaveGame();
    }

    public void NewGame()
    {
        runData = new RunData();
        gameData = new GameData();
        buildingPresets = new List<BuildingSettings>();
    }
    public void NewRun()
    {
        runData = new RunData();
    }
    public void ResetRun()
    {
        runData = new RunData();
        dataHandler.DeleteRunData(runDataFilePrefix);
    }
    public void LoadGame()
    {
        gameData = dataHandler.LoadObjectData<GameData>(gameDataFilePrefix);
        buildingPresets = dataHandler.LoadSettings(settingsFilePrefix);
        if (gameData == null)
        {
            Debug.Log("No game saves found. Creating new game...");
            NewGame();
        }
        // load settings
        foreach (ISettingsPersistence settingsPersistenceObj in settingsPersistenceObjects)
        {
            settingsPersistenceObj.LoadSettings(buildingPresets);
        }
        // load game data
        foreach (IGameDataPersistence gameDataPersistenceObj in gameDataPersistenceObjects)
        {
            gameDataPersistenceObj.LoadData(gameData);
        }
    }
    public void LoadRun()
    {
        runData = dataHandler.LoadObjectData<RunData>(runDataFilePrefix);
        if (runData == null)
        {
            Debug.Log("No run data found. Creating new run...");
            NewRun();
        }
        this.runDataPersistenceObjects = FindAllRunDataPersistenceObjects();
        foreach (IRunDataPersistence dataPersistenceObj in runDataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(runData);
        }
    }
    public void SaveRun()
    {
        foreach (IRunDataPersistence dataPersistenceObj in runDataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref runData);
        }
        
        dataHandler.Save(runData, runDataFilePrefix);
    }
    public void SaveGame()
    {
        List<BuildingSettings> buildingPresetsToSave = new List<BuildingSettings>();
        foreach (ISettingsPersistence settingsPersistenceObj in settingsPersistenceObjects)
        {
            buildingPresetsToSave = settingsPersistenceObj.SaveSettings();
        }
        foreach (IGameDataPersistence gameDataPersistenceObj in gameDataPersistenceObjects)
        {
            gameDataPersistenceObj.SaveData(ref gameData);
        }
        dataHandler.Save(gameData, gameDataFilePrefix);
        dataHandler.Save(buildingPresetsToSave, settingsFilePrefix);
    }
    private List<IRunDataPersistence> FindAllRunDataPersistenceObjects()
    {
        IEnumerable<IRunDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IRunDataPersistence>();
        return new List<IRunDataPersistence>(dataPersistenceObjects);
    }
    private List<IGameDataPersistence> FindAllGameDataPersistenceObjects()
    {
        IEnumerable<IGameDataPersistence> gameDataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IGameDataPersistence>();
        return new List<IGameDataPersistence>(gameDataPersistenceObjects);
    }
    private List<ISettingsPersistence> FindAllSettingsPersistenceObjects()
    {
        IEnumerable<ISettingsPersistence> settingsPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<ISettingsPersistence>();
        return new List<ISettingsPersistence>(settingsPersistenceObjects);
    }
}
