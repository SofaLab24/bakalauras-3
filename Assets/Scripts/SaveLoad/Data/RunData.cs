using System.Collections.Generic;

[System.Serializable]
public class EnemyStats
{
    public string id;
    public int health;
    public float moveSpeed;
    public int damage;
}

[System.Serializable]
public class RunData
{
    public int currentWave;
    public int currentMoney;
    public int currentHealth;
    public SerializableMapData mapData;

    public List<EnemyStats> enemyTypeStats = new();
    public List<string> savedEnemyPoolIds = new();
    public int savedEnemiesToGenerate;

    // Default values on new game
    public RunData()
    {
        currentWave = 0;
        currentMoney = 400;
        currentHealth = 0;
        mapData = new SerializableMapData();
        enemyTypeStats = new List<EnemyStats>();
        savedEnemyPoolIds = new List<string>();
        savedEnemiesToGenerate = 0;
    }
}
