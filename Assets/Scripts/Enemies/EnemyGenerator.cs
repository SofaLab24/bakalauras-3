using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] int healthMultiplier = 10;
    [SerializeField] float moveSpeedMultiplier = 1.0f;


    public (int damage,int health, float moveSpeed) GenerateEnemy(int statPoints)
    {
        int leftStatPoints = statPoints + 1;

        // If only 1 stat point, then it's 10 health and 3f move speed
        int healthPoints = Random.Range(1, leftStatPoints);
        leftStatPoints -= healthPoints;
        float moveSpeed;
        if (leftStatPoints < 15f)
        {
            moveSpeed = leftStatPoints + 2f;
        }
        else moveSpeed = 15f; // max move speed

        Debug.Log("Generated enemy from " + statPoints + " stat points. Health: " + healthPoints * healthMultiplier 
        + " Move speed: " + moveSpeed * moveSpeedMultiplier);
        return (statPoints, healthPoints * healthMultiplier, moveSpeed * moveSpeedMultiplier);
    }

}
