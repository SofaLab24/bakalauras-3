using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float zoomSpeed = 2f;
    [SerializeField]
    private float minZoom = 5f;
    [SerializeField]
    private float maxZoom = 15f;

    [Header("Screen shake settings")]
    [SerializeField]
    private float shakeDuration = 0.5f;
    [SerializeField]
    private float shakeMagnitude = 0.1f;
    private float shakeLeftTime = 0f;
    private Vector3 originalPosition;

    private Camera cam;
    void OnEnable()
    {
        BaseManager.OnBaseDamaged += ShakeCamera;
    }

    void OnDisable()
    {
        BaseManager.OnBaseDamaged -= ShakeCamera;
    }
    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        if (shakeLeftTime > 0)
        {
            transform.localPosition = originalPosition + UnityEngine.Random.insideUnitSphere * shakeMagnitude;
            shakeLeftTime -= Time.deltaTime;
        }
    }
    private void ShakeCamera()
    {
        originalPosition = transform.position;
        shakeLeftTime = shakeDuration;
    }
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, vertical, 0f);
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newSize = cam.orthographicSize - scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }
} 