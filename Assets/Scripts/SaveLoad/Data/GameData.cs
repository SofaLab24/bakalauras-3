using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class GameData
{
    public int waveHighscore;
    public int metaCoins;
    public List<(string towerName, bool damageUpgraded, bool rangeUpgraded, bool fireRateUpgraded, bool fireTypeUnlocked)> towerUpgradeStatus;

    // Default values on new game
    public GameData()
    {
        // all game data
        waveHighscore = 0;
        metaCoins = 0;
        towerUpgradeStatus = new List<(string towerName, bool damageUpgraded, bool rangeUpgraded, bool fireRateUpgraded, bool fireTypeUnlocked)>();
    }
}
