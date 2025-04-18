using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    public List<BuildingSettings> availableBuildings;
    [SerializeField]
    private Tilemap tilemap;
    
    private EconomyManager economyManager;
    private PathGenerator pathGenerator;
    private Camera mainCamera;

    public static event Action<Vector3> OnBuildingPlaced;
    // TODO: trigger faulty sound effect and highlight missing money
    public static event Action<bool> TriggerRangeIndicator;
    public static event Action OnInsufficientFunds;

    private BuildingSettings selectedBuilding;

    private void OnEnable()
    {
        EconomyManager.OnPurchaseAttempted += HandlePurchaseAttempt;
    }

    private void OnDisable()
    {
        EconomyManager.OnPurchaseAttempted -= HandlePurchaseAttempt;
    }

    private void Start()
    {
        economyManager = GetComponent<EconomyManager>();
        pathGenerator = GetComponent<PathGenerator>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleBuildingPlacement();
        }
    }
    public void SelectBuilding(BuildingSettings buildingSettings)
    {
        selectedBuilding = buildingSettings;
    }

    private void HandleBuildingPlacement()
    {
        TriggerRangeIndicator?.Invoke(true);

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);

        // Check if the clicked position is a building spot
        TileBase clickedTile = tilemap.GetTile(cellPosition);
        if (clickedTile != null && IsBuildingSpot(cellPosition))
        {
            // Check if there's already a building at this position
            Vector3 worldPosition = tilemap.GetCellCenterWorld(cellPosition);
            Transform building = IsBuildingAtPosition(worldPosition);
            if (building != null)
            {
                if (building.TryGetComponent<BaseTower>(out BaseTower tower))
                {
                    tower.ToggleRangeIndicator();
                    return;
                }
                return;
            }

            if (TryPlaceBuilding(selectedBuilding))
            {
                PlaceBuilding(cellPosition, selectedBuilding);
            }
        }
    }

    private bool TryPlaceBuilding(BuildingSettings buildingSettings)
    {
        if (buildingSettings == null)
        {
            Debug.LogError("No building settings selected");
            return false;
        }

        if (economyManager.SpendMoney(buildingSettings.buildingCost))
        {
            return true;
        }
        else
        {
            OnInsufficientFunds?.Invoke();
            return false;
        }
    }

    private Transform IsBuildingAtPosition(Vector3 worldPosition)
    {
        // Increased radius to 0.4f for better detection
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPosition, 0.4f);
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Tower"))
            {
                return collider.transform; // Building already exists here
            }
        }
        
        return null;
    }

    private void HandlePurchaseAttempt(int amount, bool success)
    {
        // We can use this to respond to purchase attempts if needed
        // For example, play a sound when purchase fails
        if (!success)
        {
            OnInsufficientFunds?.Invoke();
        }
    }

    private bool IsBuildingSpot(Vector3Int position)
    {
        // Convert tilemap position to PathTile coordinates
        Vector2Int pathTileCoords = new Vector2Int(position.x / pathGenerator.tileSize, position.y / pathGenerator.tileSize);
        Vector2Int localCoords = new Vector2Int(position.x % pathGenerator.tileSize, position.y % pathGenerator.tileSize);
        if (localCoords.x < 0) localCoords.x += pathGenerator.tileSize;
        if (localCoords.y < 0) localCoords.y += pathGenerator.tileSize;

        // Get the PathTile and check its tilesToFill array
        if (pathGenerator.allTiles.TryGetValue(pathTileCoords, out PathTile tile))
        {
            return tile.tilesToFill[localCoords.x, localCoords.y] == 0;
        }
        return false;
    }

    private void PlaceBuilding(Vector3Int cellPosition, BuildingSettings buildingSettings)
    {
        Vector3 buildingPosition = tilemap.GetCellCenterWorld(cellPosition);
        
        GameObject building = Instantiate(buildingSettings.buildingPrefab, buildingPosition, Quaternion.identity);
        if (buildingSettings.isTower)
        {
            building.GetComponent<BaseTower>().Initialize(buildingSettings);
        }
        OnBuildingPlaced?.Invoke(buildingPosition);
    }
} 