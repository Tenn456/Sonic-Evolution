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

    float yaw;    // horizontal rotation
    float pitch;  // vertical rotation

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
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Read controller right stick
        float stickX = Input.GetAxis("RightStickX");
        float stickY = Input.GetAxis("RightStickY");

        // Combine mouse and controller input
        yaw += (mouseX + stickX) * mouseSensitivity * Time.deltaTime;
        pitch -= (mouseY + -stickY) * mouseSensitivity * Time.deltaTime;

        // Clamp vertical look
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
