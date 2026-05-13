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

    protected override TowerUpgrade CreateSpecialtyUpgrade()
    {
        string name = poisonDamage > 0 ? "ATK SPEED + POISON" : "ATK SPEED";
        int cost = settings != null ? settings.buildingCost : 0;
        return new TowerUpgrade(name, cost, () =>
        {
            if (poisonDamage > 0)
                poisonDamage = Mathf.RoundToInt(poisonDamage * 1.5f);
            shootingSpeed *= 0.67f;
        });
    }
}
