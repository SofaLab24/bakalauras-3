using UnityEngine;

public class ArrowTower : BaseTower
{
    public override void DealDamage(Vector3 targetPosition, Transform target)
    {   
        if (target != null)
        {
            EnemyHealthManager enemyHealth = target.GetComponent<EnemyHealthManager>();
            enemyHealth.TakeDamage(damage);
            DealPoisonDamage(enemyHealth);
        }
    }

    protected override void ApplySpecialtyUpgrade()
    {
        base.ApplySpecialtyUpgrade();
        shootingSpeed *= 0.67f;
    }

    public override string GetSpecialtyName()
    {
        return poisonDamage > 0 ? "ATK SPEED + POISON" : "ATK SPEED";
    }
} 