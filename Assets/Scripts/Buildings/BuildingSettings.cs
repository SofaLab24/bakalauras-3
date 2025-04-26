using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Newtonsoft.Json;

[CreateAssetMenu(menuName = "Buildings/New Building", order = 1)]
[System.Serializable]
public class BuildingSettings : ScriptableObject
{
    [JsonProperty][SerializeField]
    private string towerName;
    public bool isUnlocked;
    public int buildingCost;
    [JsonIgnore]
    public Texture2D buildingIcon;
    [JsonIgnore]
    public GameObject buildingPrefab;
    
    [Header("Building Type")]
    public bool isTower;
    
    [Header("Tower Settings")]
    public TowerType towerType;
    public float towerRange = 5f;
    public float towerShootingDelay = 0.5f;
    public float towerProjectileSpeed = 10f;
    [JsonIgnore]
    public AnimationCurve projectileSpeedCurve;
    public int towerDamage = 20;
    public int towerBurnDamage = 0;
    public float towerExplosionRadius = 2f;
    public LayerMask enemyLayer;
    [JsonIgnore]
    public GameObject towerProjectilePrefab;
    [Header("Resource Building Settings")]
    public string resourceType;
    public int resourceMultiplier;
}

[System.Serializable]
public enum TowerType
{
    Arrow,
    Cannon,
    Wizard
}