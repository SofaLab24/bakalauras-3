using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    protected List<TowerUpgrade> upgrades = new();

    private const int DamageCost = 100;

    public string TowerName => towerName;
    public float Range => range;
    public IReadOnlyList<TowerUpgrade> GetUpgrades() => upgrades;

    protected virtual void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public virtual void Initialize(BuildingSettings settings)
    {
        this.settings = settings;
        towerName = settings.towerName;
        range = settings.towerRange;
        shootingSpeed = settings.towerShootingDelay;
        projectileSpeed = settings.towerProjectileSpeed;
        damage = settings.towerDamage;
        enemyLayer = settings.enemyLayer;
        projectilePrefab = settings.towerProjectilePrefab;
        projectileSpeedCurve = settings.projectileSpeedCurve;
        poisonDamage = settings.towerPoisonDamage;
        RegisterUpgrades();
    }

    protected virtual void RegisterUpgrades()
    {
        upgrades.Add(CreateDamageUpgrade());
        upgrades.Add(CreateSpecialtyUpgrade());
    }

    protected virtual TowerUpgrade CreateDamageUpgrade()
    {
        return new TowerUpgrade("DAMAGE", DamageCost,
            () => damage = Mathf.RoundToInt(damage * 1.5f));
    }

    protected virtual TowerUpgrade CreateSpecialtyUpgrade()
    {
        string name = poisonDamage > 0 ? "SPECIALTY + POISON" : "SPECIALTY";
        int cost = settings != null ? settings.buildingCost : 0;
        return new TowerUpgrade(name, cost, () =>
        {
            if (poisonDamage > 0)
                poisonDamage = Mathf.RoundToInt(poisonDamage * 1.5f);
        });
    }

    public void LoadUpgrades(List<bool> purchasedStates)
    {
        for (int i = 0; i < purchasedStates.Count && i < upgrades.Count; i++)
        {
            if (purchasedStates[i])
                upgrades[i].ForceApply();
        }
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
            shootingCoroutine = StartCoroutine(ShootingRoutine());
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
                FindNewTarget();

            if (currentTarget != null)
                ShootAtTarget();

            yield return new WaitForSeconds(shootingSpeed);
        }
    }

    protected virtual bool IsTargetValid()
    {
        if (currentTarget == null) return false;

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > range) return false;

        EnemyHealthManager enemyHealth = currentTarget.GetComponent<EnemyHealthManager>();
        if (enemyHealth == null || enemyHealth.currentHealth <= 0) return false;

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
            towerHead.up = currentTarget.position - towerHead.position;
    }

    protected virtual void ShootAtTarget()
    {
        EnemyHealthManager enemyHealth = currentTarget.GetComponent<EnemyHealthManager>();
        if (enemyHealth != null)
        {
            animator.SetTrigger("Shoot");
            Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity)
                .GetComponent<Projectile>();
            projectile.Initialize(currentTarget, projectileSpeed, projectileSpeedCurve, this);
            SFXManager.Instance.ShootSFX(GetComponent<AudioSource>());
        }
    }

    public abstract void DealDamage(Vector3 targetPosition, Transform target);

    public virtual void DealPoisonDamage(EnemyHealthManager enemyHealth)
    {
        if (enemyHealth != null)
            enemyHealth.SetPoisonDamage(poisonDamage);
    }

    public virtual string GetStatsText()
    {
        float atkPerSec = shootingSpeed > 0f ? 1f / shootingSpeed : 0f;
        string stats = $"- Damage: {damage}\n- Atk Speed: {atkPerSec:F1} shots/s\n- Range: {range:F0}";
        if (poisonDamage > 0)
            stats += $"\n- Poison: {poisonDamage}";
        return stats;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
