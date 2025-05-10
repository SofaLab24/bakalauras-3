using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class PathTile
{
    public bool isFilled = false;
    public bool isEnd = false;
    public int[,] tilesToFill; // 0 - tower placement tile, 1 - road tile, 2 - portal/spawn
    [JsonProperty]
    private Vector2Int coordinates; // should be equal to dictionary's indexes
    [JsonProperty]
    private int size; // should always be 3

    public override string ToString()
    {
       string str = "";
       str += "PathTile: " + coordinates + "\n";
       str += "Tiles to fill: \n";
       for (int i = 0; i < size; i++)
       {
        for (int j = 0; j < size; j++)
        {
            str += tilesToFill[i, j] + " ";
        }
        str += "\n";
       }
       return str;
    }

    public PathTile(Vector2Int coordinates, int size)
    {
        this.coordinates = coordinates;
        this.size = size;
        this.tilesToFill = new int[size, size]; // should default to 0
    }
    // Returns where the next tile should be
    private void GeneratePathToCenter(Vector2Int start, bool fillCenter = false)
    {
        int center = (size - 1) / 2;
        if (fillCenter)
        {
            tilesToFill[center, center] = 1;
        }

        // FOR X AXIS
        if (start.x == -1) // if entrance is on left side
        {
            for (int i = 0; i < center; i++)
            {
                tilesToFill[i, center] = 1; // set x to road until middle
            }
        }
        else if (start.x == 1) // if entrance is on right side
        {
            for (int i = size - 1; i > center; i--)
            {
                tilesToFill[i, center] = 1; // set x to road
            }
        }
        // FOR Y AXIS
        else if (start.y == -1) // if entrance is on bottom
        {
            for (int i = 0; i < center; i++)
            {
                tilesToFill[center, i] = 1;
            }
        }
        else // if entrance is on top
        {
            for (int i = size - 1; i > center; i--)
            {
                tilesToFill[center, i] = 1;
            }
        }
    }
    /// <summary>
    /// Generates which tiles to fill inside the path tile
    /// </summary>
    /// <param name="entrance"></param>
    /// <param name="exit"></param>
    /// <returns> Next target tile coordinates </returns>
    public Vector2Int GeneratePath(Vector2Int entrance, Vector2Int exit)
    {
        isFilled = true;
        GeneratePathToCenter(entrance);

        // Portal generation
        if (exit.x == 0 && exit.y == 0)
        {
            isEnd = true;
            int center = (size - 1) / 2;
            tilesToFill[center, center] = 2;

            return Vector2Int.zero;
        }
        GeneratePathToCenter(exit, true);

        return new Vector2Int(coordinates.x + exit.x, coordinates.y + exit.y);
    }
    public (Vector2Int, Vector2Int) GenerateSplitPath(Vector2Int entrance, Vector2Int exit1,  Vector2Int exit2)
    {
        isFilled = true;
        GeneratePathToCenter(entrance, true);
        GeneratePathToCenter(exit1);
        GeneratePathToCenter(exit2);

        return (new Vector2Int(coordinates.x + exit1.x, coordinates.y + exit1.y), new Vector2Int(coordinates.x + exit2.x, coordinates.y + exit2.y));
    }

    /// <summary>
    /// Gets coordinates of a specific tile in the tilemap
    /// </summary>
    /// <param name="tileX"></param>
    /// <param name="tileY"></param>
    /// <returns></returns>
    public Vector2Int GetTilemapCoordinates(int tileX, int tileY)
    {
        return new Vector2Int((coordinates.x * size) + tileX, (coordinates.y * size) + tileY);
    }
    public Vector2Int GetTilemapCoordinates()
    {
        return GetTilemapCoordinates(coordinates.x, coordinates.y);
    }
    public Vector2Int GetCoordinates()
    { return coordinates; }
    public Vector2 GetWorldOfCenter(Vector2 tilemapCoordinates = default)
    {
        Vector2 centerOfTilemap = new Vector2((coordinates.x * size) + (size / 2f), (coordinates.y * size) + (size / 2f));
        return new Vector2(centerOfTilemap.x + tilemapCoordinates.x, centerOfTilemap.y + tilemapCoordinates.y);
    }
}
