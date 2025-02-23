using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthManager : MonoBehaviour
{
    public static event Action<EnemyHealthManager> OnEnemyDeath;
    public int moneyValue = 10;

    public void Die()
    {
        // TODO: trigger particles
        OnEnemyDeath?.Invoke(this);

        Destroy(gameObject);
    }
}
