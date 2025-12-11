using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 8f;
    public float height = 4f;

    public float mouseSensitivity = 150f;
    public float rotationSmoothness = 10f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    float yaw;    // horizontal mouse rotation
    float pitch;  // vertical mouse rotation

    Vector3 smoothVelocity;

    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Read mouse input
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Convert rotation to world rotation
        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);

        // Calculate position based on rotation
        Vector3 desiredPosition = target.position + desiredRotation * new Vector3(0, height, -distance);

        // Smooth transition between positions
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            rotationSmoothness * Time.deltaTime
        );

        // Look at player
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
