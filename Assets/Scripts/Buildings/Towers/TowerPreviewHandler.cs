using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerPreviewHandler : MonoBehaviour
{
    public Vector3Int baseTile;
    public BuildingManager buildingManager;

    void Start()
    {
        buildingManager = BuildingManager.Instance;
    }
    void Update()
    {
        Vector3Int hoveredTile = buildingManager.GetHoveredTilePosition();
        if (hoveredTile != baseTile)
        {
            DestroyPreview();
        }
    }
    public void DestroyPreview()
    {
        Destroy(this.gameObject);
    }
}
