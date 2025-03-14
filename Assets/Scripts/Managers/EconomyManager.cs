using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    [SerializeField]
    private int _playerMoney = 0;
    
    public static event Action<int> OnMoneyChanged;
    public static event Action<int, bool> OnPurchaseAttempted; // Amount, success

    public int playerMoney
    {
        get { return _playerMoney; }
        private set
        {
            if (_playerMoney != value)
            {
                _playerMoney = value;
                OnMoneyChanged?.Invoke(_playerMoney);
            }
        }
    }

    void OnEnable()
    {
        EnemyHealthManager.OnEnemyDeath += HandleEnemyDeath;
    }

    void OnDisable()
    {
        EnemyHealthManager.OnEnemyDeath -= HandleEnemyDeath;
    }

    void HandleEnemyDeath(EnemyHealthManager enemy, EnemyHealthManager.DeathReason reason)
    {
        if (reason == EnemyHealthManager.DeathReason.KilledByTower)
        {
            AddMoney(enemy);
        }
    }

    private void AddMoney(EnemyHealthManager enemy)
    {
        playerMoney += enemy.moneyValue;
    }

    public bool CanAfford(int amount)
    {
        return _playerMoney >= amount;
    }

    public bool SpendMoney(int amount)
    {
        bool success = false;
        
        if (CanAfford(amount))
        {
            playerMoney -= amount;
            success = true;
        }
        
        // Invoke event about the purchase attempt
        OnPurchaseAttempted?.Invoke(amount, success);
        
        return success;
    }
}
