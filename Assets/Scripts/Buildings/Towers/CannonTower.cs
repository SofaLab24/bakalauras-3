using UnityEngine;

public class CannonTower : BaseTower
{
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private GameObject explosionPrefab;
    public override void Initialize(BuildingSettings settings)
    {
        base.Initialize(settings);
        explosionRadius = settings.towerExplosionRadius;
        explosionPrefab = settings.towerExplosionPrefab;
    }
    public override void DealDamage(Vector3 targetPosition, Transform target)
    {
        // Find all enemies in explosion radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(targetPosition, explosionRadius, enemyLayer);
        ExplosionEffect(targetPosition);
        foreach (Collider2D collider in hitColliders)
        {
            EnemyHealthManager enemyHealth = collider.GetComponent<EnemyHealthManager>();
            if (enemyHealth != null)
            {
                // Damage decreases with distance from explosion center
                float distance = Vector2.Distance(targetPosition, collider.transform.position);
                float damageMultiplier = 1 - (distance / explosionRadius);
                int actualDamage = Mathf.RoundToInt(damage * damageMultiplier);
                
                enemyHealth.TakeDamage(actualDamage);
            }
        }
    }
    private void ExplosionEffect(Vector3 targetPosition)
    {
        // Create explosion effect
        GameObject explosion = Instantiate(explosionPrefab, targetPosition, Quaternion.identity);
        Destroy(explosion, 2f);
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