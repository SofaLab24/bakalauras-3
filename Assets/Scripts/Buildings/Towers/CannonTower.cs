using UnityEngine;

public class CannonTower : BaseTower
{
    [SerializeField] protected float explosionRadius = 2f;
    [SerializeField] private GameObject explosionPrefab;

    public override void Initialize(BuildingSettings settings)
    {
        base.Initialize(settings);
        explosionRadius = settings.towerExplosionRadius;
        explosionPrefab = settings.towerExplosionPrefab;
    }

    public override void DealDamage(Vector3 targetPosition, Transform target)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(targetPosition, explosionRadius, enemyLayer);
        ExplosionEffect(targetPosition);
        foreach (Collider2D collider in hitColliders)
        {
            EnemyHealthManager enemyHealth = collider.GetComponent<EnemyHealthManager>();
            if (enemyHealth != null)
            {
                float distance = Vector2.Distance(targetPosition, collider.transform.position);
                float damageMultiplier = 1 - (distance / explosionRadius);
                int actualDamage = Mathf.RoundToInt(damage * damageMultiplier);
                enemyHealth.TakeDamage(actualDamage);
                DealPoisonDamage(enemyHealth);
            }
        }
    }

    private void ExplosionEffect(Vector3 targetPosition)
    {
        GameObject explosion = Instantiate(explosionPrefab, targetPosition, Quaternion.identity);
        Destroy(explosion, 2f);
        SFXManager.Instance.ExplosionSFX(GetComponent<AudioSource>());
    }

    protected override TowerUpgrade CreateSpecialtyUpgrade()
    {
        string name = poisonDamage > 0 ? "EXPL. RADIUS + POISON" : "EXPL. RADIUS";
        int cost = settings != null ? settings.buildingCost : 0;
        return new TowerUpgrade(name, cost, () =>
        {
            if (poisonDamage > 0)
                poisonDamage = Mathf.RoundToInt(poisonDamage * 1.5f);
            explosionRadius *= 1.5f;
        });
    }

    public override string GetStatsText()
    {
        return base.GetStatsText() + $"\n- Expl. Radius: {explosionRadius:F1}";
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentTarget.position, explosionRadius);
        }
    }
}
