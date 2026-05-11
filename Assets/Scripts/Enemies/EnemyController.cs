using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    protected List<Vector2> targets;
    protected EnemyHealthManager healthManager;
    protected Rigidbody2D body;
    SpriteRenderer spriteRenderer;

    [SerializeField] float moveSpeedVariance = 0.1f;

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
    }
    public void SetMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed + Random.Range(-moveSpeedVariance, moveSpeedVariance);
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
                healthManager.ReachEnd();
                return; //just in case
            }
            body.velocity = Vector2.zero;
            transform.position = new Vector3(target.x, target.y);
            targets.RemoveAt(targets.Count - 1);
            UpdateFacingDirection(targets[^1]);
        }
    }
    public virtual void Initialize(int damage, List<Vector2> targets, float moveSpeed, int health)
    {
        this.healthManager.damageValue = damage;
        this.healthManager.SetMoneyValue(damage);
        this.targets = new List<Vector2>(targets);
        SetMoveSpeed(moveSpeed);
        SetHealth(health);
        UpdateFacingDirection(this.targets[^1]);
    }

    private void UpdateFacingDirection(Vector2 target)
    {
        Vector2Int direction = CalculateDirection(target);
        if (direction.x < 0) spriteRenderer.flipX = true;
        else if (direction.x > 0) spriteRenderer.flipX = false;
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
