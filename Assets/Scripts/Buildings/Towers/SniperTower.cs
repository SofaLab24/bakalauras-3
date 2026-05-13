using UnityEngine;

public class SniperTower : BaseTower
{
    public float towerExplosionRadius;

    public override void Initialize(BuildingSettings settings)
    {
        base.Initialize(settings);
        towerExplosionRadius = settings.towerExplosionRadius;
    }

    public override void DealDamage(Vector3 targetPosition, Transform target)
    {
        if (target != null)
        {
            EnemyHealthManager enemyHealth = target.GetComponent<EnemyHealthManager>();
            enemyHealth.TakeDamage(damage);
            DealPoisonDamage(enemyHealth);
        }
    }

    protected override void FindNewTarget()
    {
        currentTarget = null;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        if (hitColliders.Length == 0) return;

        float maxHealth = hitColliders[0].GetComponent<EnemyHealthManager>().currentHealth;
        Collider2D targetEnemy = hitColliders[0];

        foreach (Collider2D collider in hitColliders)
        {
            EnemyHealthManager enemyHealth = collider.GetComponent<EnemyHealthManager>();
            if (enemyHealth != null && enemyHealth.currentHealth > 0 && enemyHealth.currentHealth > maxHealth)
            {
                maxHealth = enemyHealth.currentHealth;
                targetEnemy = collider;
            }
        }

        currentTarget = targetEnemy.transform;
        LookAtTarget();
    }

    protected override TowerUpgrade CreateSpecialtyUpgrade()
    {
        string name = poisonDamage > 0 ? "ATK RANGE + POISON" : "ATK RANGE";
        int cost = settings != null ? settings.buildingCost : 0;
        return new TowerUpgrade(name, cost, () =>
        {
            if (poisonDamage > 0)
                poisonDamage = Mathf.RoundToInt(poisonDamage * 1.5f);
            range *= 1.5f;
        });
    }
}
