using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SerializableMapDataTests
{
    // --- Default constructor ---

    [Test]
    public void DefaultConstructor_SerializablePathsIsNull()
    {
        var data = new SerializableMapData();
        Assert.IsNull(data.serializablePaths);
    }

    [Test]
    public void DefaultConstructor_PlacedBuildingsIsEmptyList()
    {
        var data = new SerializableMapData();
        Assert.IsNotNull(data.placedBuildings);
        Assert.AreEqual(0, data.placedBuildings.Count);
    }

    // --- GetPaths before SetPaths ---

    [Test]
    public void GetPaths_ReturnsNull_WhenNeverSet()
    {
        var data = new SerializableMapData();
        Assert.IsNull(data.GetPaths());
    }

    // --- SetPaths / GetPaths round-trips ---

    [Test]
    public void SetPaths_EmptyList_GetPathsReturnsEmptyList()
    {
        var data = new SerializableMapData();
        data.SetPaths(new List<WavePath>());
        var result = data.GetPaths();
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void SetPaths_GetPaths_PreservesPathCount()
    {
        var data = new SerializableMapData();
        data.SetPaths(new List<WavePath>
        {
            MakePath(new Vector2Int(0, 0), new Vector2Int(1, 0), false),
            MakePath(new Vector2Int(1, 0), new Vector2Int(2, 0), true),
        });
        Assert.AreEqual(2, data.GetPaths().Count);
    }

    [Test]
    public void SetPaths_GetPaths_PreservesHasEnd_True()
    {
        var data = new SerializableMapData();
        data.SetPaths(new List<WavePath> { MakePath(Vector2Int.zero, Vector2Int.one, true) });
        Assert.IsTrue(data.GetPaths()[0].hasEnd);
    }

    [Test]
    public void SetPaths_GetPaths_PreservesHasEnd_False()
    {
        var data = new SerializableMapData();
        data.SetPaths(new List<WavePath> { MakePath(Vector2Int.zero, Vector2Int.one, false) });
        Assert.IsFalse(data.GetPaths()[0].hasEnd);
    }

    [Test]
    public void SetPaths_GetPaths_PreservesLastFilledTile()
    {
        var expected = new Vector2Int(3, 7);
        var data = new SerializableMapData();
        data.SetPaths(new List<WavePath> { MakePath(expected, new Vector2Int(4, 7), false) });
        Assert.AreEqual(expected, data.GetPaths()[0].lastFilledTile);
    }

    [Test]
    public void SetPaths_GetPaths_PreservesTargetTile()
    {
        var expected = new Vector2Int(5, 2);
        var data = new SerializableMapData();
        data.SetPaths(new List<WavePath> { MakePath(Vector2Int.zero, expected, false) });
        Assert.AreEqual(expected, data.GetPaths()[0].targetTile);
    }

    [Test]
    public void SetPaths_GetPaths_PreservesEnemyWalkPointCount()
    {
        var walkPoints = new List<Vector2> { new Vector2(1.5f, 2.5f), new Vector2(4.5f, 2.5f) };
        var path = new WavePath(new List<PathTile>(), walkPoints, Vector2Int.zero, Vector2Int.one, false);
        var data = new SerializableMapData();
        data.SetPaths(new List<WavePath> { path });
        Assert.AreEqual(2, data.GetPaths()[0].enemyWalkPoints.Count);
    }

    [Test]
    public void SetPaths_GetPaths_PreservesEnemyWalkPointValues()
    {
        var expected = new Vector2(1.5f, 2.5f);
        var path = new WavePath(new List<PathTile>(), new List<Vector2> { expected }, Vector2Int.zero, Vector2Int.one, false);
        var data = new SerializableMapData();
        data.SetPaths(new List<WavePath> { path });
        var result = data.GetPaths()[0].enemyWalkPoints[0];
        Assert.AreEqual(expected.x, result.x, 0.001f);
        Assert.AreEqual(expected.y, result.y, 0.001f);
    }

    [Test]
    public void SetPaths_Twice_OverwritesPreviousPaths()
    {
        var data = new SerializableMapData();
        data.SetPaths(new List<WavePath> { MakePath(Vector2Int.zero, Vector2Int.one, false) });
        data.SetPaths(new List<WavePath>());
        Assert.AreEqual(0, data.GetPaths().Count);
    }

    // --- PlacedBuildingData ---

    [Test]
    public void PlacedBuildingData_StoresPosition()
    {
        var b = new PlacedBuildingData { x = 1.5f, y = 3.0f };
        Assert.AreEqual(1.5f, b.x);
        Assert.AreEqual(3.0f, b.y);
    }

    [Test]
    public void PlacedBuildingData_StoresBuildingName()
    {
        var b = new PlacedBuildingData { buildingName = "ArrowTower" };
        Assert.AreEqual("ArrowTower", b.buildingName);
    }

    [Test]
    public void PlacedBuildingData_StoresUpgradeFlags()
    {
        var b = new PlacedBuildingData { damageUpgraded = true, specialtyUpgraded = false };
        Assert.IsTrue(b.damageUpgraded);
        Assert.IsFalse(b.specialtyUpgraded);
    }

    // --- Helper ---

    private static WavePath MakePath(Vector2Int lastFilledTile, Vector2Int targetTile, bool hasEnd)
    {
        return new WavePath(new List<PathTile>(), new List<Vector2>(), lastFilledTile, targetTile, hasEnd);
    }
}
