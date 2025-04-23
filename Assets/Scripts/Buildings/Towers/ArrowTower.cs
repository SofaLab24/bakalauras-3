using UnityEngine;

public class ArrowTower : BaseTower
{
    public override void DealDamage(Vector3 targetPosition, Transform target)
    {   
        if (target != null)
        {
            EnemyHealthManager enemyHealth = target.GetComponent<EnemyHealthManager>();
            enemyHealth.TakeDamage(damage);
        }

    }
} 