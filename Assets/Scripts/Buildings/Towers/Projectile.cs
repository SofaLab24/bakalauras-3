using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float distanceOffset = 0.9f;
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private BaseTower parentTower;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isInitialized = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isInitialized)
        {
            if (target != null)
            {
                targetPosition = target.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, (1f + CalculateNextSpeed()) * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPosition) < distanceOffset)
                {
                    parentTower.DealDamage(transform.position, target);
                    Destroy(gameObject);
                }
            }
            else // move towards latest position of the target
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, (1f + CalculateNextSpeed()) * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPosition) < distanceOffset)
                {
                    parentTower.DealDamage(transform.position, target);
                    Destroy(gameObject);
                }
            }
        }

    }

    private float CalculateNextSpeed()
    {
        Vector3 trajectoryRange = targetPosition - startPosition;
        float totalDistance = trajectoryRange.magnitude;
        float currentDistance = Vector3.Distance(transform.position, targetPosition);
        float nextStep = speedCurve.Evaluate(1 - (currentDistance / totalDistance)) * moveSpeed;
        return nextStep;
    }

    public void Initialize(Transform target, float moveSpeed, AnimationCurve speedCurve, BaseTower parentTower)
    {
        this.target = target;
        this.moveSpeed = moveSpeed;
        this.speedCurve = speedCurve;
        this.parentTower = parentTower;
        Debug.Log("Parent tower: " + parentTower.gameObject.name);
        isInitialized = true;
    }
    public Vector3 GetMoveDirection()
    {
        return (targetPosition - transform.position).normalized;
    }
}
