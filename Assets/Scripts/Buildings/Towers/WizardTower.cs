using UnityEngine;

public class WizardTower : BaseTower
{
    public float towerExplosionRadius;
    override public void Initialize(BuildingSettings settings)
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
            // TODO: only visual explosion
        }

    }
    // Aims at enemy with most health
    protected override void FindNewTarget()
    {
        currentTarget = null;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
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
    }
} 