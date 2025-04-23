using UnityEngine;

public class FlamingArrowTower : BaseTower
{
    public int towerBurnDamage = 0;
    public override void Initialize(BuildingSettings settings)
    {
        base.Initialize(settings);
        towerBurnDamage = settings.towerBurnDamage;
    }
    public override void DealDamage(Vector3 targetPosition, Transform target)
    {   
        if (target != null)
        {
            EnemyHealthManager enemyHealth = target.GetComponent<EnemyHealthManager>();
            enemyHealth.TakeDamage(damage);
            enemyHealth.SetBurnDamage(towerBurnDamage);
        }

    }
} 