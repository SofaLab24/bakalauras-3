using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerManager : MonoBehaviour
{
    [SerializeField]
    private GameObject towerPrefab;
    [SerializeField]
    private Tilemap tilemap;
    [SerializeField]
    private int towerCost = 100;

    private EconomyManager economyManager;
    private PathGenerator pathGenerator;
    private Camera mainCamera;

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
            HandleTowerPlacement();
        }
    }

    private void HandleTowerPlacement()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);

        // Check if the clicked position is a tower spot
        TileBase clickedTile = tilemap.GetTile(cellPosition);
        if (clickedTile != null && IsTowerSpot(cellPosition))
        {
            // Check if we can afford the tower
            if (economyManager.playerMoney >= towerCost)
            {
                Vector3 worldPosition = tilemap.GetCellCenterWorld(cellPosition);
                // Increased radius to 0.4f for better detection
                Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPosition, 0.4f);
                
                foreach (Collider2D collider in colliders)
                {
                    if (collider.CompareTag("Tower"))
                    {
                        return; // Tower already exists here
                    }
                }
                
                PlaceTower(cellPosition);
            }
        }
    }

    private bool IsTowerSpot(Vector3Int position)
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

    private void PlaceTower(Vector3Int cellPosition)
    {
        Vector3 towerPosition = tilemap.GetCellCenterWorld(cellPosition);
        GameObject tower = Instantiate(towerPrefab, towerPosition, Quaternion.identity);
        economyManager.playerMoney -= towerCost;
    }
} 