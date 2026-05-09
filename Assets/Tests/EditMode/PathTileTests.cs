using NUnit.Framework;
using UnityEngine;

public class PathTileTests
{
    [Test]
    public void GetCoordinates_ReturnsCorrectCoordinates()
    {
        var coords = new Vector2Int(3, 5);
        var tile = new PathTile(coords, 3);
        Assert.AreEqual(coords, tile.GetCoordinates());
    }

    [Test]
    public void GetTilemapCoordinates_WithTileOffsets_CalculatesCorrectly()
    {
        // coords (2,1), size 3: result = (2*3 + tileX, 1*3 + tileY)
        var tile = new PathTile(new Vector2Int(2, 1), 3);
        var result = tile.GetTilemapCoordinates(1, 2);
        Assert.AreEqual(new Vector2Int(7, 5), result);
    }

    [Test]
    public void GetTilemapCoordinates_AtOrigin_ReturnsOffset()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        Assert.AreEqual(new Vector2Int(1, 2), tile.GetTilemapCoordinates(1, 2));
    }

    [Test]
    public void NewTile_IsNotFilled()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        Assert.IsFalse(tile.isFilled);
    }

    [Test]
    public void NewTile_IsNotEnd()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        Assert.IsFalse(tile.isEnd);
    }

    [Test]
    public void NewTile_AllTilesToFillAreZero()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                Assert.AreEqual(0, tile.tilesToFill[i, j]);
    }

    [Test]
    public void GeneratePath_SetsFilledTrue()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        tile.GeneratePath(new Vector2Int(-1, 0), new Vector2Int(1, 0));
        Assert.IsTrue(tile.isFilled);
    }

    [Test]
    public void GeneratePath_WithZeroExit_SetsIsEnd()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        tile.GeneratePath(new Vector2Int(-1, 0), Vector2Int.zero);
        Assert.IsTrue(tile.isEnd);
    }

    [Test]
    public void GeneratePath_WithZeroExit_ReturnsZero()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        var next = tile.GeneratePath(new Vector2Int(-1, 0), Vector2Int.zero);
        Assert.AreEqual(Vector2Int.zero, next);
    }

    [Test]
    public void GeneratePath_WithZeroExit_MarksCenterAsPortal()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        tile.GeneratePath(new Vector2Int(-1, 0), Vector2Int.zero);
        // center of size-3 tile is (1,1); portal = value 2
        Assert.AreEqual(2, tile.tilesToFill[1, 1]);
    }

    [Test]
    public void GeneratePath_NonZeroExit_DoesNotSetIsEnd()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        tile.GeneratePath(new Vector2Int(-1, 0), new Vector2Int(1, 0));
        Assert.IsFalse(tile.isEnd);
    }

    [Test]
    public void GeneratePath_ReturnsNextTileCoordinates()
    {
        var coords = new Vector2Int(2, 3);
        var tile = new PathTile(coords, 3);
        var exit = new Vector2Int(1, 0);
        var next = tile.GeneratePath(new Vector2Int(-1, 0), exit);
        Assert.AreEqual(new Vector2Int(coords.x + exit.x, coords.y + exit.y), next);
    }

    [Test]
    public void GeneratePath_LeftEntrance_FillsLeftHalfRoadTiles()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        // entrance from left (x=-1): fills tilesToFill[0, center] = 1
        tile.GeneratePath(new Vector2Int(-1, 0), new Vector2Int(1, 0));
        Assert.AreEqual(1, tile.tilesToFill[0, 1]);
    }

    [Test]
    public void GeneratePath_RightExit_FillsCenterTileAsRoad()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        // exit to right (x=1): fills tilesToFill[2, 1] = 1 and center
        tile.GeneratePath(new Vector2Int(-1, 0), new Vector2Int(1, 0));
        Assert.AreEqual(1, tile.tilesToFill[2, 1]);
    }

    [Test]
    public void GetWorldOfCenter_ReturnsCorrectPosition()
    {
        // coords (1,2), size 3: center = (1*3 + 1.5f, 2*3 + 1.5f) = (4.5, 7.5)
        var tile = new PathTile(new Vector2Int(1, 2), 3);
        var center = tile.GetWorldOfCenter();
        Assert.AreEqual(new Vector2(4.5f, 7.5f), center);
    }

    [Test]
    public void GetWorldOfCenter_AtOrigin_ReturnsHalfSize()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        var center = tile.GetWorldOfCenter();
        Assert.AreEqual(new Vector2(1.5f, 1.5f), center);
    }

    [Test]
    public void GetWorldOfCenter_WithTilemapOffset_AddsOffset()
    {
        var tile = new PathTile(new Vector2Int(0, 0), 3);
        var center = tile.GetWorldOfCenter(new Vector2(10f, 5f));
        Assert.AreEqual(new Vector2(11.5f, 6.5f), center);
    }

    [Test]
    public void GenerateSplitPath_SetsFilledTrue()
    {
        var tile = new PathTile(new Vector2Int(1, 1), 3);
        tile.GenerateSplitPath(new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1));
        Assert.IsTrue(tile.isFilled);
    }

    [Test]
    public void GenerateSplitPath_ReturnsTwoNextCoordinates()
    {
        var coords = new Vector2Int(1, 1);
        var tile = new PathTile(coords, 3);
        var exit1 = new Vector2Int(1, 0);
        var exit2 = new Vector2Int(0, 1);
        var (next1, next2) = tile.GenerateSplitPath(new Vector2Int(-1, 0), exit1, exit2);
        Assert.AreEqual(new Vector2Int(coords.x + exit1.x, coords.y + exit1.y), next1);
        Assert.AreEqual(new Vector2Int(coords.x + exit2.x, coords.y + exit2.y), next2);
    }
}
