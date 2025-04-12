using UnityEngine;

public class ArrowTower : BaseTower
{
    protected override void ShootAtTarget()
    {
        EnemyHealthManager enemyHealth = currentTarget.GetComponent<EnemyHealthManager>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
    }
} 