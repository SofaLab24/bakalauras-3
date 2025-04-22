using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Rigidbody2D body;
    List<Vector2> targets;
    EnemyHealthManager healthManager;

    public float moveSpeed = 5f;
    public float distanceOffset = 0.05f;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        healthManager = GetComponent<EnemyHealthManager>();
    }

    private void FixedUpdate()
    {
        Move();
    }
    public void SetHealth(int health)
    {
        healthManager.health = health;
    }
    public void SetMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }
    private void Move()
    {
        Vector2 target = targets[^1];
        float distance = CalculateDistance(target);
        if (distance > distanceOffset)
        {
            Vector2Int direction = CalculateDirection(target);
            body.velocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed);
        }
        else // reached the current target
        {
            if (targets.Count == 1) { 
                // Enemy has reached the end of the path
                Debug.Log("Enemy reached end of path");
                healthManager.ReachEnd();
                return; //just in case
            }
            body.velocity = Vector2.zero;
            transform.position = new Vector3(target.x, target.y);
            targets.RemoveAt(targets.Count - 1);
        }
    }

    public void SetTargets(List<Vector2> targets)
    {
        this.targets = new List<Vector2>(targets);
    }
    public Vector2Int CalculateDirection(Vector2 target)
    {
        return Vector2Int.RoundToInt((new Vector2(target.x - transform.position.x, target.y - transform.position.y)).normalized);
    }
    public float CalculateDistance(Vector2 target)
    {
        return Vector3.Distance(transform.position, new Vector3(target.x, target.y));
    }
}
