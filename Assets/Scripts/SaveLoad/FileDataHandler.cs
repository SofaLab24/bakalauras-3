using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public T LoadObjectData<T>(string prefix = "")
    {
        string fullPath = Path.Combine(dataDirPath, prefix + dataFileName);
        T loadedData = default(T);
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                loadedData = JsonConvert.DeserializeObject<T>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when loading data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }
    public void Save<T>(T data, string prefix = "")
    {
        string fullPath = Path.Combine(dataDirPath, prefix + dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataJson = JsonConvert.SerializeObject(data);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataJson);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when saving data to file: " + fullPath + "\n" + e);
        }
    }
    public List<BuildingSettings> LoadSettings(string prefix = "")
    {
        string fullPath = Path.Combine(dataDirPath, prefix + dataFileName);
        List<BuildingSettings> buildingSettings = new List<BuildingSettings>();
        
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                
                // First deserialize to a temporary list of dictionaries
                List<Dictionary<string, object>> tempData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(dataToLoad);
                Debug.Log("Loaded " + tempData.Count + " building presets (list of dictionaries)");

                foreach (var itemData in tempData)
                {
                    // Create a ScriptableObject instance
                    BuildingSettings newSettings = ScriptableObject.CreateInstance<BuildingSettings>();
                    
                    // Use JsonConvert to convert the dictionary to a JSON string
                    string itemJson = JsonConvert.SerializeObject(itemData);
                    Debug.Log(itemJson);
                    // Populate the ScriptableObject with the JSON data
                    JsonConvert.PopulateObject(itemJson, newSettings);
                    
                    buildingSettings.Add(newSettings);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occurred when loading settings from file: " + fullPath + "\n" + e);
            }
        }
        else
        {
            Debug.Log("Settings file not found: " + fullPath);
        }
        Debug.Log("Loaded " + buildingSettings.Count + " building presets");
        return buildingSettings;
    }
    public void SaveSettings(List<BuildingSettings> buildingPresets, string prefix = "")
    {
        string fullPath = Path.Combine(dataDirPath, prefix + dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataJson = JsonConvert.SerializeObject(buildingPresets);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataJson);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when saving data to file: " + fullPath + "\n" + e);
        }
    }
    public void DeleteRunData(string prefix)
    {
        string fullPath = Path.Combine(dataDirPath, prefix + dataFileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
