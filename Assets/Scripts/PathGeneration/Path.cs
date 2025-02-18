using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public List<PathTile> pathTiles;
    public List<Vector2> enemyWalkPoints;
    public bool hasEnd;
    public Vector2Int lastFilledTile;
    public Vector2Int targetTile;

    public Path(List<PathTile> pathTiles, List<Vector2> enemyWalkPoints, Vector2Int lastFilledTile, Vector2Int targetTile)
    {
        this.pathTiles = pathTiles;
        this.enemyWalkPoints = enemyWalkPoints;
        this.lastFilledTile = lastFilledTile;
        this.targetTile = targetTile;
        hasEnd = false;
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
    public Path GetCopy()
    {
        return new Path(new List<PathTile>(pathTiles), new List<Vector2>(enemyWalkPoints), lastFilledTile, targetTile);
    }
}
