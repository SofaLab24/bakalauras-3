using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Rigidbody2D body;
    List<Vector2> targets;
    EnemyHealthManager healthManager;
    SpriteRenderer spriteRenderer;

    [SerializeField] float maxHealthColorScale = 200f;
    [SerializeField] float maxMoveSpeedColorScale = 15f;

    public float moveSpeed = 5f;
    public float distanceOffset = 0.09f;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        healthManager = GetComponent<EnemyHealthManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        Move();
    }
    public void SetHealth(int health)
    {
        healthManager.currentHealth = health;
        float red;
        if (health < maxHealthColorScale)
        {
            red = health / maxHealthColorScale;
        }
        else red = 1f;
        Color currentColor = spriteRenderer.color;
        spriteRenderer.color = new Color(red, currentColor.g, currentColor.b, currentColor.a);
    }
    public void SetMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
        float blue;
        if (moveSpeed < maxMoveSpeedColorScale)
        {
            blue = moveSpeed / maxMoveSpeedColorScale;
        }
        else blue = 1f;
        Color currentColor = spriteRenderer.color;
        spriteRenderer.color = new Color(currentColor.r, currentColor.g, blue, currentColor.a);
    }
    private void Move()
    {
        Vector2 target = targets[^1];
        float distance = CalculateDistance(target);
        if (distance > distanceOffset)
        {
            Vector2Int direction = CalculateDirection(target);
            
            body.velocity = new Vector2(direction.x, direction.y) * moveSpeed * Time.fixedDeltaTime * 40;
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
    public void Initialize(int damage, List<Vector2> targets, float moveSpeed, int health)
    {
        this.healthManager.damageValue = damage;
        this.targets = new List<Vector2>(targets);
        SetMoveSpeed(moveSpeed);
        SetHealth(health);
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
