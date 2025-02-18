using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField]
    EnemyController enemyPrefab;

    PathGenerator pathGenerator;
    private void Start()
    {
        pathGenerator = GetComponent<PathGenerator>();
    }
    public void StartNextWave(float splitChance)
    {
        List<Path> currentPaths = pathGenerator.GenerateNextPaths(splitChance);
        foreach (Path path in currentPaths)
        {
            List<Vector2> enemyTargets = path.enemyWalkPoints;
            Vector3 initPosition = new Vector3(enemyTargets[^1].x, enemyTargets[^1].y);
            EnemyController enemy = Instantiate(enemyPrefab, initPosition, Quaternion.identity);
            enemy.SetTargets(enemyTargets);
        }
    }
}
