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

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
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