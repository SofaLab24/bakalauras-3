using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public int playerMoney = 0;

    void OnEnable()
    {
        EnemyHealthManager.OnEnemyDeath += AddMoney;
    }

    void OnDisable()
    {
        EnemyHealthManager.OnEnemyDeath -= AddMoney;
    }

    void AddMoney(EnemyHealthManager enemy)
    {
        playerMoney += enemy.moneyValue;
    }
}
