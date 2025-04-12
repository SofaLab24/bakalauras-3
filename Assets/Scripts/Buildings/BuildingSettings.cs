using UnityEngine;
using UnityEngine.Tilemaps;
using System;

[CreateAssetMenu(menuName = "Buildings/New Building", order = 1)]
public class BuildingSettings : ScriptableObject
{
    public int buildingCost;
    public Texture2D buildingIcon;
    public GameObject buildingPrefab;
    
    [Header("Building Type")]
    public bool isTower;
    
    [Header("Tower Settings")]
    public TowerType towerType;
    public float towerRange = 5f;
    public float towerShootingSpeed = 0.5f;
    public int towerDamage = 20;
    public LayerMask enemyLayer;
    
    [Header("Resource Building Settings")]
    public string resourceType;
    public int resourceMultiplier;
}

public enum TowerType
{
    Basic,
    Arrow,
    Cannon
}