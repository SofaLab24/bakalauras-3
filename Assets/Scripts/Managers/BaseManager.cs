using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    public static event Action<int> OnHealthChanged;
    public static event Action OnBaseDestroyed;

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    void OnEnable()
    {
        EnemyHealthManager.OnEnemyDeath += HandleEnemyAttack;
    }
    void OnDisable()
    {
        EnemyHealthManager.OnEnemyDeath -= HandleEnemyAttack;
    }
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // Notify listeners of initial health after they are set up
    void LateStart()
    {
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void HandleEnemyAttack(EnemyHealthManager enemy, EnemyHealthManager.DeathReason reason)
    {
        if (reason == EnemyHealthManager.DeathReason.ReachedEnd)
        {
            TakeDamage(enemy.damageValue);
        }
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Clamp health to minimum of 0
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Notify listeners that health has changed
        OnHealthChanged?.Invoke(currentHealth);
        
        // Check if the base is destroyed
        if (currentHealth <= 0)
        {
            OnBaseDestroyed?.Invoke();
            HandleBaseDestruction();
        }
    }

    private void HandleBaseDestruction()
    {
        Debug.Log("Base destroyed! Game Over!");
        // TODO: Reset save of game. Don't reset meta progression
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
