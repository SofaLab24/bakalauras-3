using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerShooting : MonoBehaviour
{
    [SerializeField]
    float Radius;
    [SerializeField]
    float ShootingSpeed;
    [SerializeField]
    int Damage;
    [SerializeField]
    LayerMask enemyLayer;

    private Coroutine shootingCoroutine;
    private Transform currentTarget; // Reference to the current target

    void OnEnable()
    {
        StartShooting();
    }

    void OnDisable()
    {
        StopShooting();
    }

    void StartShooting()
    {
        if (shootingCoroutine == null)
        {
            shootingCoroutine = StartCoroutine(ShootingRoutine());
        }
    }

    void StopShooting()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    IEnumerator ShootingRoutine()
    {
        while (true)
        {
            // Check if current target is still valid, otherwise find a new one
            if (!IsTargetValid())
            {
                FindNewTarget();
            }

            // Shoot at the current target if one exists
            if (currentTarget != null)
            {
                ShootAtTarget();
            }

            yield return new WaitForSeconds(ShootingSpeed);
        }
    }

    bool IsTargetValid()
    {
        // Check if target exists
        if (currentTarget == null)
            return false;

        // Check if target is still in range
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > Radius)
            return false;

        // Check if target still has health (is alive)
        EnemyHealthManager enemyHealth = currentTarget.GetComponent<EnemyHealthManager>();
        if (enemyHealth == null || enemyHealth.health <= 0)
            return false;

        return true;
    }

    void FindNewTarget()
    {
        currentTarget = null;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, Radius, enemyLayer);
        
        // Find the first valid enemy
        foreach (Collider2D collider in hitColliders)
        {
            EnemyHealthManager enemyHealth = collider.GetComponent<EnemyHealthManager>();
            if (enemyHealth != null && enemyHealth.health > 0)
            {
                currentTarget = collider.transform;
                break; // Found a target, exit the loop
            }
        }
    }

    void ShootAtTarget()
    {
        EnemyHealthManager enemyHealth = currentTarget.GetComponent<EnemyHealthManager>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(Damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
