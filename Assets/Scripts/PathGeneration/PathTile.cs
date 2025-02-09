using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] // To be able to save
public class PathTile
{
    public bool isFilled = false;
    public bool isEnd = false;
    public int[,] tilesToFill; // 0 - tower placement tile, 1 - road tile, 2 - portal/spawn

    private Vector2Int coordinates; // should be equal to dictionary's indexes
    private int size; // should always be 3

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

    public Vector2Int GetTilemapCoordinates(int tileX, int tileY)
    {
        return new Vector2Int((coordinates.x * size) + tileX, (coordinates.y * size) + tileY);
    }
}
