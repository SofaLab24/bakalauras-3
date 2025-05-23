using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour, IRunDataPersistence
{
    [SerializeField] private Tilemap tilemap;
    
    private PlayerEconomyManager economyManager;
    private PathGenerator pathGenerator;
    private Camera mainCamera;

    public static event Action<Vector3> OnBuildingPlaced;
    public static event Action<bool> TriggerRangeIndicator;
    public static event Action OnInsufficientFunds;
    private bool canPlaceBuilding = true;

    private BuildingSettings selectedBuilding;
    private List<(float x, float y, string buildingName)> placedBuildings;

    private void OnEnable()
    {
        PlayerEconomyManager.OnPurchaseAttempted += HandlePurchaseAttempt;
        OverlayManager.OnEscMenu += HandleEscMenu;
    }

    private void OnDisable()
    {
        PlayerEconomyManager.OnPurchaseAttempted -= HandlePurchaseAttempt;
        OverlayManager.OnEscMenu -= HandleEscMenu;
    }

    private void Start()
    {
        economyManager = GetComponent<PlayerEconomyManager>();
        pathGenerator = GetComponent<PathGenerator>();
        mainCamera = Camera.main;

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleBuildingPlacement();
        }
    }
    public void HandleEscMenu(bool isOpen)
    {
        if (isOpen)
        {
            canPlaceBuilding = false;
        }
        else
        {
            canPlaceBuilding = true;
        }
    }
    public void SelectBuilding(BuildingSettings buildingSettings)
    {
        selectedBuilding = buildingSettings;
    }

    private void HandleBuildingPlacement()
    {
        if (!canPlaceBuilding) return;
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
        if (!success)
        {
            OnInsufficientFunds?.Invoke();
        }
    }

    private bool IsBuildingSpot(Vector3Int position)
    {
        // Convert tilemap position to PathTile coordinates
        Vector2Int pathTileCoords = new Vector2Int(
            Mathf.FloorToInt((float)position.x / pathGenerator.tileSize),
            Mathf.FloorToInt((float)position.y / pathGenerator.tileSize)
        );
        Vector2Int localCoords = new Vector2Int(position.x % pathGenerator.tileSize, position.y % pathGenerator.tileSize);
        if (localCoords.x < 0) localCoords.x += pathGenerator.tileSize;
        if (localCoords.y < 0) localCoords.y += pathGenerator.tileSize;
        if (pathGenerator.allTiles.TryGetValue(pathTileCoords, out PathTile tile))
        {
            return tile.tilesToFill[localCoords.x, localCoords.y] == 0;
        }
        return false;
    }

    private void PlaceBuilding(Vector3Int cellPosition, BuildingSettings buildingSettings)
    {
        Vector3 buildingPosition = tilemap.GetCellCenterWorld(cellPosition);
        GameObject building = InstantiateBuilding(buildingPosition, buildingSettings);
        placedBuildings.Add((buildingPosition.x, buildingPosition.y, buildingSettings.towerName));

        OnBuildingPlaced?.Invoke(buildingPosition);
    }
    private GameObject InstantiateBuilding(Vector3 position, BuildingSettings buildingSettings)
    {
        GameObject building = Instantiate(buildingSettings.buildingPrefab, position, Quaternion.identity);
        if (buildingSettings.isTower)
        {
            building.GetComponent<BaseTower>().Initialize(buildingSettings);
        }
        return building;
    }

    public void LoadData(RunData data)
    {
        placedBuildings = data.mapData.placedBuildings;
        foreach (var building in placedBuildings)
        {
            InstantiateBuilding(new Vector3(building.x, building.y, 0), BuildingPresetsHandler.Instance.GetBuildingPreset(building.buildingName));
        }
    }

    public void SaveData(ref RunData data)
    {
        data.mapData.placedBuildings = placedBuildings;
    }

} 