using UnityEngine;
using Unity.Cinemachine;

public class CameraRecenter : MonoBehaviour
{
    [Header("References")]
    public Transform sonic;
    public CinemachineCamera cinemachineCam;

    [Header("Input")]
    public KeyCode keyboardRecenterKey = KeyCode.R;
    public string recenterButton = "joystick button 9";

    [Header("Behavior")]
    public float recenterSpeed = 700f;

    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineInputAxisController axisController;

    private bool recentering;
    private float targetYaw; // Saved yaw target

    void Awake()
    {
        if (cinemachineCam != null)
        {
            orbitalFollow = cinemachineCam.GetComponent<CinemachineOrbitalFollow>();
            axisController = cinemachineCam.GetComponent<CinemachineInputAxisController>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (sonic == null || cinemachineCam == null || orbitalFollow == null)
            return;

        bool recenterPressed =
            Input.GetKeyDown(keyboardRecenterKey) ||
            Input.GetKeyDown(recenterButton);

        if (recenterPressed)
        {
            // Save the target ONLY when the button is pressed
            Vector3 desiredDir = -sonic.forward;
            desiredDir.y = 0f;

            if (desiredDir.sqrMagnitude < 0.001f)
                return;

            desiredDir.Normalize();

            targetYaw = Mathf.Atan2(desiredDir.x, desiredDir.z) * Mathf.Rad2Deg;

            recentering = true;

            if (axisController != null)
                axisController.enabled = false;
        }
    }

    void LateUpdate()
    {
        if (!recentering || sonic == null || cinemachineCam == null || orbitalFollow == null)
            return;

        // Get current camera direction
        Vector3 currentDir = cinemachineCam.transform.position - sonic.position;
        currentDir.y = 0f;

        if (currentDir.sqrMagnitude < 0.001f)
            return;

        currentDir.Normalize();

        float currentYaw = Mathf.Atan2(currentDir.x, currentDir.z) * Mathf.Rad2Deg;

        // Compare current camera yaw to the SAVED target yaw
        float delta = Mathf.DeltaAngle(currentYaw, targetYaw);

        float step = Mathf.Sign(delta) * Mathf.Min(
            Mathf.Abs(delta),
            recenterSpeed * Time.deltaTime
        );

        orbitalFollow.HorizontalAxis.Value += step;

        if (Mathf.Abs(delta) < 0.5f)
        {
            recentering = false;

            if (axisController != null)
                axisController.enabled = true;
        }
    }
}