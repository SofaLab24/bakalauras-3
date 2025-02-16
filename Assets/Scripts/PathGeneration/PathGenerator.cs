using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class PathGenerator : MonoBehaviour
{
    private class Path
    {
        public List<PathTile> pathTiles;
        public List<GameObject> enemyWalkPoints;
        public bool hasEnd;
        public Vector2Int lastFilledTile;
        public Vector2Int targetTile;

        public Path(List<PathTile> pathTiles, List<GameObject> enemyWalkPoints, Vector2Int lastFilledTile, Vector2Int targetTile)
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
        }
        public void SetEnd()
        {
            hasEnd = true;
        }
        public Path GetCopy()
        {
            return new Path(pathTiles, enemyWalkPoints, lastFilledTile, targetTile);
        }
    }

    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    TileBase road;
    [SerializeField]
    TileBase towerSpot;
    [SerializeField]
    TileBase portal;

    public int tileSize = 3;

    private Dictionary<Vector2Int, PathTile> allTiles;
    private List<Path> paths;

    public void Start()
    {
        SetupTileGeneration();
    }
    public void SetupTileGeneration()
    {
        allTiles = new Dictionary<Vector2Int, PathTile>();
        paths = new List<Path>();
        List<PathTile> initPath = new List<PathTile>();
        // TODO: implement walkpoint logic
        List<GameObject> enemyWalkPoints = new List<GameObject>();
        paths.Add(new Path(initPath, enemyWalkPoints, Vector2Int.zero, Vector2Int.zero));

        for (int i = 0; i < 2; i++)
        {
            allTiles[new Vector2Int(0, i)] = new PathTile(new Vector2Int(0, i), tileSize);
            allTiles[new Vector2Int(0, i)].GeneratePath(Vector2Int.down, Vector2Int.up);
            paths[0].AddNewTile(allTiles[new Vector2Int(0, i)]);
            PaintPathTile(allTiles[new Vector2Int(0, i)]);
        }
        PathTile lastPath = new PathTile(new Vector2Int(0, 2), tileSize);
        allTiles[new Vector2Int(0, 2)] = lastPath;
        Vector2Int targetTile = lastPath.GeneratePath(Vector2Int.down, Vector2Int.up);
        allTiles[targetTile] = new PathTile(targetTile, tileSize);
        PaintPathTile(lastPath);
        paths[0].SetLastFilledTile(lastPath.GetCoordinates());
        paths[0].SetTargetTile(targetTile);
    }
    public void PaintPathTile(PathTile tile)
    {
        for (int i = 0; i < tile.tilesToFill.GetLength(0); i++)
        {
            for (int j = 0; j < tile.tilesToFill.GetLength(1); j++)
            {
                Vector2Int tilemapCoordinate = tile.GetTilemapCoordinates(i, j);
                switch (tile.tilesToFill[i, j])
                {
                    case 0:
                        tilemap.SetTile((Vector3Int)tilemapCoordinate, towerSpot);
                        break;
                    case 1:
                        tilemap.SetTile((Vector3Int)tilemapCoordinate, road);
                        break;
                    case 2:
                        tilemap.SetTile((Vector3Int)tilemapCoordinate, portal);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    /// <summary>
    /// Generates next paths for all the current paths
    /// </summary>
    /// <param name="splitChance"> 0.0 - 1.0 chance of paths to have a split tile </param>
    public void GenerateNextPaths(float splitChance)
    {
        for (int i = paths.Count - 1; i >= 0; i--)
        {
            if (paths[i].hasEnd) continue;
            Vector2Int targetCoordinates = paths[i].targetTile;
            List<Vector2Int> possiblePaths = new List<Vector2Int>();
            // Left
            if (!allTiles.ContainsKey(new Vector2Int(targetCoordinates.x - 1, targetCoordinates.y)))
            {
                possiblePaths.Add(new Vector2Int(targetCoordinates.x - 1, targetCoordinates.y));
            }
            // Right
            if (!allTiles.ContainsKey(new Vector2Int(targetCoordinates.x + 1, targetCoordinates.y)))
            {
                possiblePaths.Add(new Vector2Int(targetCoordinates.x + 1, targetCoordinates.y));
            }
            // Bottom
            if (!allTiles.ContainsKey(new Vector2Int(targetCoordinates.x, targetCoordinates.y - 1)))
            {
                possiblePaths.Add(new Vector2Int(targetCoordinates.x, targetCoordinates.y - 1));
            }
            // Top
            if (!allTiles.ContainsKey(new Vector2Int(targetCoordinates.x, targetCoordinates.y + 1)))
            {
                possiblePaths.Add(new Vector2Int(targetCoordinates.x, targetCoordinates.y + 1));
            }

            if (possiblePaths.Count == 0) // if it's the end
            {
                allTiles[targetCoordinates].GeneratePath(paths[i].lastFilledTile - targetCoordinates, Vector2Int.zero);
                paths[i].SetEnd();
                PaintPathTile(allTiles[targetCoordinates]);
                continue;
            }

            // Does the path split
            int exitCount = 1;
            if (splitChance > 0 && possiblePaths.Count > 1)
            {
                if (Random.Range(0, 1.0f) <= splitChance) exitCount++;
            }

            // Create exits
            Vector2Int[] exits = new Vector2Int[exitCount];
            for (int j = 0; j < exitCount; j++)
            {
                exits[j] = (possiblePaths[Random.Range(0, possiblePaths.Count)]);
            }
            // only single path
            if (exitCount == 1)
            {
                Vector2Int nextTraget = allTiles[targetCoordinates].GeneratePath(paths[i].lastFilledTile - targetCoordinates, exits[0] - targetCoordinates);
                PaintPathTile(allTiles[targetCoordinates]);
                paths[i].SetLastFilledTile(targetCoordinates);
                allTiles[nextTraget] = new PathTile(nextTraget, tileSize);
                paths[i].SetTargetTile(nextTraget);
            }
            // split path
            else
            {
                int newPathId = paths.Count;
                (Vector2Int nextTraget1, Vector2Int nextTarget2) = allTiles[targetCoordinates]
                    .GenerateSplitPath(paths[i].lastFilledTile - targetCoordinates, exits[0] - targetCoordinates, exits[1] - targetCoordinates);
                PaintPathTile(allTiles[targetCoordinates]);
                paths[i].SetLastFilledTile(targetCoordinates);
                paths.Add(paths[i].GetCopy());
                allTiles[nextTraget1] = new PathTile(nextTraget1, tileSize);
                allTiles[nextTarget2] = new PathTile(nextTarget2, tileSize);
                paths[i].SetTargetTile(nextTraget1);
                paths[newPathId].SetTargetTile(nextTarget2);
            }
        }
    }
}
