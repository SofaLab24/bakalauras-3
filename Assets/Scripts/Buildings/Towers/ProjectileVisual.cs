using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileVisual : MonoBehaviour
{
    [SerializeField] private Transform projectileVisual;
    private Projectile projectile;

    void Start()
    {
        projectile = GetComponent<Projectile>();
    }

    void Update()
    {
        UpdateRotation();
    }
    private void UpdateRotation()
    {
        Vector3 moveDirection = projectile.GetMoveDirection();
        projectileVisual.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg);
    }
}
