using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseManager : MonoBehaviour
{
    public static event Action<int> OnHealthChanged;
    public static event Action<int> OnBaseDestroyed;

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    private WaveManager waveManager;

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
        waveManager = FindObjectOfType<WaveManager>();
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
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0)
        {
            // -1 because the player died during current wave
            OnBaseDestroyed?.Invoke(waveManager.waveNumber - 1);
            HandleBaseDestruction();
        }
    }

    private void HandleBaseDestruction()
    {
        Debug.Log("Base destroyed! Game Over!");
        DataPersistenceManager.Instance.SaveGame();
        DataPersistenceManager.Instance.ResetRun();
        SceneManager.LoadScene(0);
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
