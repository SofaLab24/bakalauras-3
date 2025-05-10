using System.Collections.Generic;
using UnityEngine;
using static SerializableMapData;

public class WavePath
{
    public List<PathTile> pathTiles;
    public List<Vector2> enemyWalkPoints;
    public bool hasEnd;
    public Vector2Int lastFilledTile;
    public Vector2Int targetTile;

    public WavePath(List<PathTile> pathTiles, List<Vector2> enemyWalkPoints, Vector2Int lastFilledTile, Vector2Int targetTile, bool hasEnd = false)
    {
        this.pathTiles = pathTiles;
        this.enemyWalkPoints = enemyWalkPoints;
        this.lastFilledTile = lastFilledTile;
        this.targetTile = targetTile;
        this.hasEnd = hasEnd;
    }
    public WavePath(SerializableWavePath serializableWavePath)
    {
        pathTiles = serializableWavePath.pathTiles;
        hasEnd = serializableWavePath.hasEnd;
        lastFilledTile = serializableWavePath.lastFilledTile;
        targetTile = serializableWavePath.targetTile;
        
        enemyWalkPoints = new List<Vector2>();
        foreach (var point in serializableWavePath.enemyWalkPoints)
        {
            enemyWalkPoints.Add(new Vector2(point.x, point.y));
        }
    }

    public void SetLastFilledTile(Vector2Int coordinates)
    {
        lastFilledTile = coordinates;
    }
    public void SetTargetTile(Vector2Int coordinates)
    {
        targetTile = coordinates;
    }
    public void AddNewTile(PathTile tile)
    {
        pathTiles.Add(tile);
        enemyWalkPoints.Add(tile.GetWorldOfCenter());
    }
    public void SetEnd()
    {
        hasEnd = true;
    }
    public WavePath GetCopy()
    {
        return new WavePath(new List<PathTile>(pathTiles), new List<Vector2>(enemyWalkPoints), lastFilledTile, targetTile);
    }
}
