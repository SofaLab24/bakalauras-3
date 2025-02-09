using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathGenerator : MonoBehaviour
{
    private struct Path
    {
        public Dictionary<Vector2Int, PathTile> pathTiles;
        public List<GameObject> enemyWalkPoints;
        public bool hasEnd;
        public Vector2Int lastFilledTile;
        public Vector2Int targetTile;

        public Path(Dictionary<Vector2Int, PathTile> pathTiles, List<GameObject> enemyWalkPoints, Vector2Int lastFilledTile, Vector2Int targetTile)
        {
            this.pathTiles = pathTiles;
            this.enemyWalkPoints = enemyWalkPoints;
            this.lastFilledTile = lastFilledTile;
            this.targetTile = targetTile;
            hasEnd = false;
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
        allTiles = new Dictionary<Vector2Int, PathTile>();
        for (int i = 0; i < 4; i++)
        {
            allTiles[new Vector2Int(0, i)] = new PathTile(new Vector2Int(0, i), tileSize);
            allTiles[new Vector2Int(0, i)].GeneratePath(Vector2Int.down, Vector2Int.up);
            PaintPathTile(allTiles[new Vector2Int(0, i)]);
        }
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
    public void GenerateNextPaths()
    {
        foreach (Path path in paths)
        {

        }
    }

}
