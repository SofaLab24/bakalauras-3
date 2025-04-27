using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Newtonsoft.Json;

[CreateAssetMenu(menuName = "Buildings/New Building", order = 1)]
[System.Serializable]
public class BuildingSettings : ScriptableObject
{
    [JsonProperty][SerializeField]
    public string towerName;
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

    public BuildingSettings CloneInstance()
    {
        BuildingSettings clone = ScriptableObject.CreateInstance<BuildingSettings>();
        clone.towerName = towerName;
        clone.isUnlocked = isUnlocked;
        clone.buildingCost = buildingCost;
        clone.buildingIcon = buildingIcon;
        clone.buildingPrefab = buildingPrefab;
        clone.isTower = isTower;
        clone.towerType = towerType;
        clone.towerRange = towerRange;
        clone.towerShootingDelay = towerShootingDelay;
        clone.towerProjectileSpeed = towerProjectileSpeed;
        clone.projectileSpeedCurve = projectileSpeedCurve;
        clone.towerDamage = towerDamage;
        clone.towerBurnDamage = towerBurnDamage;
        clone.towerExplosionRadius = towerExplosionRadius;
        clone.enemyLayer = enemyLayer;
        clone.towerProjectilePrefab = towerProjectilePrefab;
        clone.resourceType = resourceType;
        clone.resourceMultiplier = resourceMultiplier;
        return clone;
    }
}

[System.Serializable]
public enum TowerType
{
    Arrow,
    Cannon,
    Wizard
}