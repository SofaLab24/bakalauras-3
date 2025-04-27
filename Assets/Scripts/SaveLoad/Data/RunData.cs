using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class RunData
{
    public int currentWave;
    public SerializableMapData mapData;

    // Default values on new game
    public RunData()
    {
        // current run data
        currentWave = 0;
        mapData = new SerializableMapData();
    }
}
