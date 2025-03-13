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
    
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Notify listeners of initial health
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if collided with an enemy
        EnemyHealthManager enemy = collision.gameObject.GetComponent<EnemyHealthManager>();
        
        if (enemy != null)
        {
            // Take damage based on enemy's damage value
            TakeDamage(enemy.damageValue);
            
            // Destroy the enemy when it hits the base
            enemy.Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealthManager enemy = collision.gameObject.GetComponent<EnemyHealthManager>();
        
        if (enemy != null)
        {
            // Take damage based on enemy's damage value
            TakeDamage(enemy.damageValue);
            
            // Destroy the enemy when it hits the base
            enemy.Die();
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
        // Add game over logic here
        Debug.Log("Base destroyed! Game Over!");
        
        // You might want to trigger a game over screen, etc.
        // This is a placeholder for your game-specific logic
    }

    // Public method to get current health (for UI, etc.)
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Public method to get max health (for UI, etc.)
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
