using UnityEngine;
using UnityEngine.Tilemaps;
using System;

[CreateAssetMenu(menuName = "Buildings/New Building", order = 1)]
public class BuildingSettings : ScriptableObject
{
    public bool isUnlocked;
    public int buildingCost;
    public Texture2D buildingIcon;
    public GameObject buildingPrefab;
    
    [Header("Building Type")]
    public bool isTower;
    
    [Header("Tower Settings")]
    public TowerType towerType;
    public float towerRange = 5f;
    public float towerShootingDelay = 0.5f;
    public float towerProjectileSpeed = 10f;
    public AnimationCurve projectileSpeedCurve;
    public int towerDamage = 20;
    public int towerBurnDamage = 0;
    public float towerExplosionRadius = 2f;
    public LayerMask enemyLayer;
    public GameObject towerProjectilePrefab;
    [Header("Resource Building Settings")]
    public string resourceType;
    public int resourceMultiplier;
}

public enum TowerType
{
    Arrow,
    Cannon,
    Wizard
}