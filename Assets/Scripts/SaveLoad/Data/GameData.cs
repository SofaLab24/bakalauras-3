using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int waveHighscore;
    public int currentWave;
    public SerializableMapData mapData;

    // Default values on new game
    public GameData()
    {
        waveHighscore = 0;
        currentWave = 0;
        mapData = new SerializableMapData();
    }
}
