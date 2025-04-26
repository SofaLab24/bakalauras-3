using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class SerializableMapData
{
    public List<SerializableWavePath> serializablePaths;
    public List<(float x, float y, string buildingName)> placedBuildings;

    // Default values on new game
    public SerializableMapData()
    {
        serializablePaths = null;
        placedBuildings = new List<(float x, float y, string buildingName)>();
    }
    public void SetPaths(List<WavePath> newPaths)
    {
        serializablePaths = new List<SerializableWavePath>();
        foreach (var path in newPaths)
        {
            serializablePaths.Add(new SerializableWavePath(path));
        }
    }
    public List<WavePath> GetPaths()
    {
        if (serializablePaths == null) return null;
        List<WavePath> paths = new List<WavePath>();
        foreach (var path in serializablePaths)
        {
            paths.Add(new WavePath(path));
        }
        return paths;
    }

    [System.Serializable]
    public class SerializableWavePath
    {
        public bool hasEnd;
        public Vector2Int lastFilledTile;
        public Vector2Int targetTile;
        public List<(float x, float y)> enemyWalkPoints;
        public List<PathTile> pathTiles;

        public SerializableWavePath()
        {
            hasEnd = false;
            lastFilledTile = new Vector2Int(0, 0);
            targetTile = new Vector2Int(0, 0);
            enemyWalkPoints = new List<(float x, float y)>();
            pathTiles = new List<PathTile>();
        }
        public SerializableWavePath(WavePath wavePath)
        {
            hasEnd = wavePath.hasEnd;
            lastFilledTile = wavePath.lastFilledTile;
            targetTile = wavePath.targetTile;
            enemyWalkPoints = new List<(float x, float y)>();
            foreach (var point in wavePath.enemyWalkPoints)
            {
                enemyWalkPoints.Add((point.x, point.y));
            }
            pathTiles = wavePath.pathTiles;
        }
    }
}
