using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour, IRunDataPersistence
{
    [SerializeField] private Tilemap tilemap;
    public static BuildingManager Instance { get; private set; } = null;

    private PlayerEconomyManager economyManager;
    private PathGenerator pathGenerator;
    private Camera mainCamera;

    public static event Action<Vector3> OnBuildingPlaced;
    public static event Action<bool> TriggerRangeIndicator;
    public static event Action OnInsufficientFunds;
    public static event Action<BaseTower> OnTowerClicked;
    private bool canPlaceBuilding = true;

    private BuildingSettings selectedBuilding;
    private List<PlacedBuildingData> placedBuildings;
    private List<(GameObject obj, string buildingName)> placedBuildingObjects = new List<(GameObject, string)>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Found more than one Building Manager in the scene. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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
        if (selectedBuilding != null && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleBuildingPreview();
        }
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

    public Vector3Int GetHoveredTilePosition()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);
        return cellPosition;
    }

    private void HandleBuildingPreview()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);
        TileBase hoveredTile = tilemap.GetTile(cellPosition);

        if (hoveredTile != null && IsBuildingSpot(cellPosition))
        {
            // Check if there's already a building at this position
            Vector3 worldPosition = tilemap.GetCellCenterWorld(cellPosition);
            if (!IsBuildingAtPosition(worldPosition))
            {
                // show ghost building
                ShowBuildingPreview(worldPosition, cellPosition, selectedBuilding);
            }
        }
        
        
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
                    OnTowerClicked?.Invoke(tower);
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
    private void ShowBuildingPreview(Vector3 position, Vector3Int hoveredCellPosition, BuildingSettings buildingSettings)
    {
        // Check if there's already a building preview at this position
        if (IsBuildingPreviewAtPosition(position))
        {
            return;
        }
        GameObject buildingPreviewObject = InstantiateBuildingPreview(position, hoveredCellPosition, buildingSettings);
    }
    private bool IsBuildingPreviewAtPosition(Vector3 worldPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPosition, 0.4f);
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("BuildingPreview"))
            {
                return true;
            }
        }
        
        return false;
    }
    private GameObject InstantiateBuildingPreview(Vector3 position, Vector3Int cellPosition, BuildingSettings buildingSettings)
    {
        // Instantiate a sprite and a TowerPreviewHandler component
        GameObject buildingPreviewObject = new GameObject(buildingSettings.towerName + " Preview");
        buildingPreviewObject.tag = "BuildingPreview";
        SpriteRenderer spriteRenderer = buildingPreviewObject.AddComponent<SpriteRenderer>();
        Sprite towerSprite = Sprite.Create(buildingSettings.buildingIcon, new Rect(0, 0, buildingSettings.buildingIcon.width, buildingSettings.buildingIcon.height), new Vector2(0.5f, 0.5f), 32f);
        spriteRenderer.sprite = towerSprite;
        BoxCollider2D boxCollider = buildingPreviewObject.AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(0.5f, 0.5f);
        TowerPreviewHandler towerPreviewHandler = buildingPreviewObject.AddComponent<TowerPreviewHandler>();
        towerPreviewHandler.baseTile = cellPosition;

        buildingPreviewObject.transform.SetPositionAndRotation(position, Quaternion.identity);
        return buildingPreviewObject;
    }
    private void PlaceBuilding(Vector3Int cellPosition, BuildingSettings buildingSettings)
    {
        Vector3 buildingPosition = tilemap.GetCellCenterWorld(cellPosition);
        GameObject building = InstantiateBuilding(buildingPosition, buildingSettings);
        placedBuildingObjects.Add((building, buildingSettings.towerName));

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
        placedBuildingObjects = new List<(GameObject, string)>();
        foreach (var building in placedBuildings)
        {
            BuildingSettings settings = BuildingPresetsHandler.Instance.GetBuildingPreset(building.buildingName);
            GameObject obj = InstantiateBuilding(new Vector3(building.x, building.y, 0), settings);
            if (building.damageUpgraded || building.specialtyUpgraded)
            {
                BaseTower tower = obj.GetComponent<BaseTower>();
                if (tower != null)
                    tower.LoadUpgrades(building.damageUpgraded, building.specialtyUpgraded);
            }
            placedBuildingObjects.Add((obj, building.buildingName));
        }
    }

    public void SaveData(ref RunData data)
    {
        var savedBuildings = new List<PlacedBuildingData>();
        foreach (var (obj, buildingName) in placedBuildingObjects)
        {
            if (obj == null) continue;
            BaseTower tower = obj.GetComponent<BaseTower>();
            savedBuildings.Add(new PlacedBuildingData
            {
                x = obj.transform.position.x,
                y = obj.transform.position.y,
                buildingName = buildingName,
                damageUpgraded = tower != null && tower.DamageUpgraded,
                specialtyUpgraded = tower != null && tower.SpecialtyUpgraded
            });
        }
        data.mapData.placedBuildings = savedBuildings;
    }

} 