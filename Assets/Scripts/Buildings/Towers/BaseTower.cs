using UnityEngine;
using System.Collections;
using System;

public abstract class BaseTower : MonoBehaviour
{
    [SerializeField] protected string towerName;
    [SerializeField] protected float range;
    [SerializeField] protected float shootingSpeed;
    [SerializeField] protected float projectileSpeed;
    [SerializeField] protected int damage;
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected AnimationCurve projectileSpeedCurve;
    [SerializeField] protected Transform towerHead;
    [SerializeField] protected Animator animator;
    
    protected Coroutine shootingCoroutine;
    protected Transform currentTarget;
    protected bool rangeIndicatorActive;
    protected GameObject rangeIndicator;
    protected int poisonDamage;
    protected BuildingSettings settings;

    public string TowerName => towerName;
    public float Range => range;
    public bool DamageUpgraded { get; private set; }
    public bool SpecialtyUpgraded { get; private set; }
    public int DamageCost = 100;
    public int SpecialtyCost => settings != null ? settings.buildingCost : 0;

    protected virtual void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public virtual void Initialize(BuildingSettings settings)
    {
        this.settings = settings;
        this.towerName = settings.towerName;
        this.range = settings.towerRange;
        this.shootingSpeed = settings.towerShootingDelay;
        this.projectileSpeed = settings.towerProjectileSpeed;
        this.damage = settings.towerDamage;
        this.enemyLayer = settings.enemyLayer;
        this.projectilePrefab = settings.towerProjectilePrefab;
        this.projectileSpeedCurve = settings.projectileSpeedCurve;
        this.poisonDamage = settings.towerPoisonDamage;
    }

    protected virtual void OnEnable()
    {
        StartShooting();
        BuildingManager.TriggerRangeIndicator += ToggleRangeIndicator;
        rangeIndicatorActive = false;
    }

    protected virtual void OnDisable()
    {
        StopShooting();
        BuildingManager.TriggerRangeIndicator -= ToggleRangeIndicator;
    }
    public virtual void ToggleRangeIndicator(bool setToFalse = false)
    {
        if (setToFalse && rangeIndicatorActive)
        {
            rangeIndicatorActive = false;
            Destroy(rangeIndicator);
            return;
        }
        else if (setToFalse) return;

        if (!rangeIndicatorActive && !setToFalse)
        {
            rangeIndicator = RangeDrawer.DrawCircle(transform, range, Color.white);
        }
        else
        {
            Destroy(rangeIndicator);
        }
        rangeIndicatorActive = !rangeIndicatorActive;
    }

    protected virtual void StartShooting()
    {
        if (shootingCoroutine == null)
        {
            shootingCoroutine = StartCoroutine(ShootingRoutine());
        }
    }

    protected virtual void StopShooting()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    protected virtual IEnumerator ShootingRoutine()
    {
        while (true)
        {
            if (!IsTargetValid())
            {
                FindNewTarget();
            }

            if (currentTarget != null)
            {
                ShootAtTarget();
            }

            yield return new WaitForSeconds(shootingSpeed);
        }
    }

    protected virtual bool IsTargetValid()
    {
        if (currentTarget == null)
            return false;

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > range)
            return false;

        EnemyHealthManager enemyHealth = currentTarget.GetComponent<EnemyHealthManager>();
        if (enemyHealth == null || enemyHealth.currentHealth <= 0)
            return false;

        return true;
    }

    protected virtual void FindNewTarget()
    {
        currentTarget = null;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        
        foreach (Collider2D collider in hitColliders)
        {
            EnemyHealthManager enemyHealth = collider.GetComponent<EnemyHealthManager>();
            if (enemyHealth != null && enemyHealth.currentHealth > 0)
            {
                currentTarget = collider.transform;
                LookAtTarget();
                break;
            }
        }
    }
    protected virtual void LookAtTarget()
    {
        if (towerHead != null)
        {
            towerHead.up = currentTarget.position - towerHead.position;
        }
    }

    // shoots projectile at target
    protected virtual void ShootAtTarget()
    {
        EnemyHealthManager enemyHealth = currentTarget.GetComponent<EnemyHealthManager>();
        if (enemyHealth != null)
        {
            animator.SetTrigger("Shoot");
            Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<Projectile>();
            projectile.Initialize(currentTarget, projectileSpeed, projectileSpeedCurve, this);
            SFXManager.instance.ShootSFX(GetComponent<AudioSource>());
        }
    }
    // called by projectile when it hits the target
    public abstract void DealDamage(Vector3 targetPosition, Transform target);
    public virtual void DealPoisonDamage(EnemyHealthManager enemyHealth)
    {
        if (enemyHealth != null)
        {
            enemyHealth.SetPoisonDamage(poisonDamage);
        }
    }

    public virtual void LoadUpgrades(bool damageUpgraded, bool specialtyUpgraded)
    {
        if (damageUpgraded)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            DamageUpgraded = true;
        }
        if (specialtyUpgraded)
        {
            ApplySpecialtyUpgrade();
            SpecialtyUpgraded = true;
        }
    }

    public bool TryUpgradeDamage(PlayerEconomyManager economy)
    {
        if (DamageUpgraded) return false;
        if (!economy.SpendMoney(DamageCost)) return false;
        damage = Mathf.RoundToInt(damage * 1.5f);
        DamageUpgraded = true;
        return true;
    }

    public bool TryUpgradeSpecialty(PlayerEconomyManager economy)
    {
        if (SpecialtyUpgraded) return false;
        if (!economy.SpendMoney(SpecialtyCost)) return false;
        ApplySpecialtyUpgrade();
        SpecialtyUpgraded = true;
        return true;
    }

    protected virtual void ApplySpecialtyUpgrade()
    {
        if (poisonDamage > 0)
            poisonDamage = Mathf.RoundToInt(poisonDamage * 1.5f);
    }

    public virtual string GetSpecialtyName()
    {
        return poisonDamage > 0 ? "SPECIALTY + POISON" : "SPECIALTY";
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
} 