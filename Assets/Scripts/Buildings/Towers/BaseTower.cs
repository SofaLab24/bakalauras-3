using UnityEngine;
using System.Collections;
using System;

public abstract class BaseTower : MonoBehaviour
{
    [SerializeField] protected float range;
    [SerializeField] protected float shootingSpeed;
    [SerializeField] protected int damage;
    [SerializeField] protected LayerMask enemyLayer;
    
    protected Coroutine shootingCoroutine;
    protected Transform currentTarget;
    protected bool rangeIndicatorActive;
    protected GameObject rangeIndicator;

    public float Range => range;
    
    public void Initialize(BuildingSettings settings)
    {
        this.range = settings.towerRange;
        this.shootingSpeed = settings.towerShootingSpeed;
        this.damage = settings.towerDamage;
        this.enemyLayer = settings.enemyLayer;
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
        if (enemyHealth == null || enemyHealth.health <= 0)
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
            if (enemyHealth != null && enemyHealth.health > 0)
            {
                currentTarget = collider.transform;
                break;
            }
        }
    }

    protected abstract void ShootAtTarget();

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
} 