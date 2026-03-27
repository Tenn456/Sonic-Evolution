using UnityEngine;
using Unity.Cinemachine;

public class CameraRecenter : MonoBehaviour
{
    [Header("References")]
    public Transform sonic;                     
    public CinemachineCamera cinemachineCam;    

    [Header("Input")]
    public KeyCode keyboardRecenterKey = KeyCode.R;
    public string recenterButton = "joystick button 9";      // R3

    [Header("Behavior")]
    public float recenterSpeed = 700f;           // Speed (degrees/sec) the camera rotates when recentering

    private CinemachineOrbitalFollow orbitalFollow;          
    private CinemachineInputAxisController axisController;   
    private bool recentering;                               

    void Awake()
    {
        // Get the Orbital Follow component from the Cinemachine camera
        if (cinemachineCam != null)
        {
            orbitalFollow = cinemachineCam.GetComponent<CinemachineOrbitalFollow>();

            // Get the input controller
            axisController = cinemachineCam.GetComponent<CinemachineInputAxisController>();
        }
    }

    void Update()
    {
        // If references are missing, stop the script
        if (sonic == null || cinemachineCam == null || orbitalFollow == null)
            return;

        // Check if the player pressed the recenter button
        bool recenterPressed = Input.GetKeyDown(keyboardRecenterKey) || Input.GetKeyDown(recenterButton);

        // If pressed, begin recentering
        if (recenterPressed)
        {
            recentering = true;

            // Disable player camera input so it doesn't fight the recenter
            if (axisController != null)
            {
                axisController.enabled = false;
            }
        }
    }

    void LateUpdate()
    {
        // Only run recenter logic if currently recentering
        if (!recentering || sonic == null || cinemachineCam == null || orbitalFollow == null)
            return;

        // Get direction from Sonic to the camera (current camera position)
        Vector3 currentDir = cinemachineCam.transform.position - sonic.position;

        // Ignore vertical difference
        currentDir.y = 0f;

        // If direction is too small, stop (prevents errors)
        if (currentDir.sqrMagnitude < 0.001f)
            return;

        // Normalize to get direction only (no magnitude)
        currentDir.Normalize();

        // Get the direction directly behind Sonic
        Vector3 desiredDir = -sonic.forward;

        // Ignore vertical component
        desiredDir.y = 0f;

        // Safety check
        if (desiredDir.sqrMagnitude < 0.001f)
            return;

        // Normalize desired direction
        desiredDir.Normalize();

        // Convert both directions into angles (yaw in degrees)
        float currentYaw = Mathf.Atan2(currentDir.x, currentDir.z) * Mathf.Rad2Deg;
        float desiredYaw = Mathf.Atan2(desiredDir.x, desiredDir.z) * Mathf.Rad2Deg;

        // Calculate shortest angle difference between current and desired
        float delta = Mathf.DeltaAngle(currentYaw, desiredYaw);

        // Clamp how much we rotate this frame based on speed
        float step = Mathf.Sign(delta) * Mathf.Min(Mathf.Abs(delta), recenterSpeed * Time.deltaTime);

        // Apply rotation change to Cinemachine's orbit axis
        orbitalFollow.HorizontalAxis.Value += step;

        // If we're very close to target, stop recentering
        if (Mathf.Abs(delta) < 0.5f)
        {
            recentering = false;

            // Re-enable player camera control
            if (axisController != null)
                axisController.enabled = true;
        }
    }
}