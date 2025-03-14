using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public int damageValue = 10;

    // Method for when enemy dies by any cause
    private void Die(DeathReason reason)
    {
        // TODO: trigger particles
        OnEnemyDeath?.Invoke(this, reason);

        Destroy(gameObject);
    }

    // Called when enemy is killed by a tower
    public void Die()
    {
        Die(DeathReason.KilledByTower);
    }

    // Called when enemy reaches the end
    public void ReachEnd()
    {
        Die(DeathReason.ReachedEnd);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) { Die(DeathReason.KilledByTower); }
    }
}
