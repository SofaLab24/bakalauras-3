using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseManager : MonoBehaviour, IRunDataPersistence
{
    public static event Action<int> OnBaseHealthChange;
    public static event Action OnBaseDamaged;
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
        waveManager = FindObjectOfType<WaveManager>();
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
        OnBaseHealthChange?.Invoke(currentHealth);
        OnBaseDamaged?.Invoke();

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

    public void LoadData(RunData data)
    {
        if (data.currentHealth <= 0)
        {
            this.currentHealth = maxHealth;
        }
        else
        {
            this.currentHealth = data.currentHealth;
        }
        OnBaseHealthChange?.Invoke(currentHealth);
    }

    public void SaveData(ref RunData data)
    {
        data.currentHealth = this.currentHealth;
    }
}
