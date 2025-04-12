using UnityEngine;

public class CannonTower : BaseTower
{
    [SerializeField] private float explosionRadius = 2f;

    protected override void ShootAtTarget()
    {
        // Find all enemies in explosion radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(currentTarget.position, explosionRadius, enemyLayer);
        
        foreach (Collider2D collider in hitColliders)
        {
            EnemyHealthManager enemyHealth = collider.GetComponent<EnemyHealthManager>();
            if (enemyHealth != null)
            {
                // Damage decreases with distance from explosion center
                float distance = Vector2.Distance(currentTarget.position, collider.transform.position);
                float damageMultiplier = 1 - (distance / explosionRadius);
                int actualDamage = Mathf.RoundToInt(damage * damageMultiplier);
                
                enemyHealth.TakeDamage(actualDamage);
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw explosion radius
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentTarget.position, explosionRadius);
        }
    }
} 