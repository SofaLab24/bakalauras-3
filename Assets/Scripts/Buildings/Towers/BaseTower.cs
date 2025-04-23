using UnityEngine;
using System.Collections;
using System;

public abstract class BaseTower : MonoBehaviour
{
    [SerializeField] protected float range;
    [SerializeField] protected float shootingSpeed;
    [SerializeField] protected float projectileSpeed;
    [SerializeField] protected int damage;
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected AnimationCurve projectileSpeedCurve;
    
    protected Coroutine shootingCoroutine;
    protected Transform currentTarget;
    protected bool rangeIndicatorActive;
    protected GameObject rangeIndicator;

    public float Range => range;
    
    public virtual void Initialize(BuildingSettings settings)
    {
        this.range = settings.towerRange;
        this.shootingSpeed = settings.towerShootingDelay;
        this.projectileSpeed = settings.towerProjectileSpeed;
        this.damage = settings.towerDamage;
        this.enemyLayer = settings.enemyLayer;
        this.projectilePrefab = settings.towerProjectilePrefab;
        this.projectileSpeedCurve = settings.projectileSpeedCurve;
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
                break;
            }
        }
    }

    // shoots projectile at target
    protected virtual void ShootAtTarget()
    {
        EnemyHealthManager enemyHealth = currentTarget.GetComponent<EnemyHealthManager>();
        if (enemyHealth != null)
        {
            Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<Projectile>();
            projectile.Initialize(currentTarget, projectileSpeed, projectileSpeedCurve, this);
        }
    }
    // called by projectile when it hits the target
    public abstract void DealDamage(Vector3 targetPosition, Transform target);

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
} 