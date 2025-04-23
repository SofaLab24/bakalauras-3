using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthManager : MonoBehaviour
{
    // Adding a death reason enum to differentiate death types
    public enum DeathReason
    {
        KilledByTower,
        ReachedEnd
    }

    // Modified event to include death reason
    public static event Action<EnemyHealthManager, DeathReason> OnEnemyDeath;
    
    public int moneyValue = 10;
    public int currentHealth = 100;
    private int maxHealth;
    public int damageValue = 10;
    [SerializeField] private Slider healthBar;
    [SerializeField] private float burnTickRate = 1f;

    private int burnDamage = 0;
    private float burnTickTimer = 0f;
    void Start()
    {
        maxHealth = currentHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }
    void Update()
    {
        if (burnDamage > 0)
        {
            burnTickTimer += Time.deltaTime;
            if (burnTickTimer >= burnTickRate)
            {
                TakeDamage(burnDamage);
                burnTickTimer = 0f;
            }
        }
    }
    public void Die(DeathReason reason)
    {
        // TODO: trigger particles
        OnEnemyDeath?.Invoke(this, reason);

        Destroy(gameObject);
    }

    public void ReachEnd()
    {
        Die(DeathReason.ReachedEnd);
    }

    public void SetBurnDamage(int damage)
    {
        burnDamage = damage;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;
        if (currentHealth <= 0) { Die(DeathReason.KilledByTower); }
    }
}
