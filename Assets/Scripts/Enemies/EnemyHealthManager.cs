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
    public int health = 100;
    private int maxHealth;
    public int damageValue = 10;
    [SerializeField] private Slider healthBar;

    void Start()
    {
        maxHealth = health;
        healthBar.maxValue = maxHealth;
        healthBar.value = health;
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

    public void TakeDamage(int damage)
    {
        health -= damage;
        healthBar.value = health;
        if (health <= 0) { Die(DeathReason.KilledByTower); }
    }
}
