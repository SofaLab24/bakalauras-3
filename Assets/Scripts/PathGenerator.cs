using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathGenerator : MonoBehaviour
{
    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    TileBase road;
    [SerializeField]
    TileBase towerSpot;
    [SerializeField]
    TileBase portal;

    public int tileSize = 3;

    private Dictionary<(int, int), PathTile> pathTiles;


    public void Start()
    {
        pathTiles = new Dictionary<(int, int), PathTile>();
        for (int i = 0; i < 4; i++)
        {
            pathTiles[(0, i)] = new PathTile(new Vector2Int(0, i), tileSize);
            pathTiles[(0, i)].GeneratePath(Vector2Int.down, Vector2Int.up);
            PaintPathTile(pathTiles[(0, i)]);

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
    public void GenerateNextPath()
    {

    }

}
