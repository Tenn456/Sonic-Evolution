using UnityEngine;

public class Sonic : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 20f;
    public float acceleration = 30f;
    public float deceleration = 20f;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float lowJumpMultiplier = 3f;   // Short-hop strength
    public float fallMultiplier = 2f;      // Faster fall
    public float gravity = -30f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private float currentSpeed;
    private Vector3 momentumDirection;

    private bool isMoving;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Grab movement input data
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Grab camera direction
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        // Removes tilt from value
        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        bool isMoving = controller.velocity.sqrMagnitude > 0.01f;

        // Convert movement input data based on camera direction (The direction of the camera is always forward)
        Vector3 inputDir = (camForward * v + camRight * h).normalized;

        // Checking if the player is inputing a direction
        bool hasInput = inputDir.sqrMagnitude > 0.01f;

        // If the player is inputing a direction...
        if (hasInput)
        {
            // Update movement direction
            momentumDirection = inputDir;
            
            // Increase speed while making sure it never goes above max speed
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }
        else
        {
            // Decrease speed while making sure it never goes below zero
            currentSpeed -= deceleration * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f);
        }

        // If Sonic is moving
        if (momentumDirection.sqrMagnitude > 0.1f)
        {
            // Sonic faces the direction he is moving
            Quaternion targetRot = Quaternion.LookRotation(momentumDirection, Vector3.up);

            // Slerp between previous rotation and new direction
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        // Grab jump input data
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool jumpHeld = Input.GetButton("Jump");

        // If Sonic is grounded...
        if (controller.isGrounded)
        {
            // This makes it so Sonic slightly sticks to the ground and doesnt go flying at every elevation change or slope
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
            
            // If jump button is pressed...
            if (jumpPressed)
            {
                // Apply jump force
                velocity.y = jumpForce;
            }
        }
        else
        {
            // If the jump button is let go
            if (!jumpHeld && velocity.y > 0)
            {
                // Short hop
                velocity.y -= lowJumpMultiplier * Time.deltaTime;
            }

            // Faster descent
            if (velocity.y < 0)
            {
                velocity.y += gravity * (fallMultiplier - 1f) * Time.deltaTime;
            }
        }

        // Apply base gravity every frame
        velocity.y += gravity * Time.deltaTime;

        // Horizontal movement calculated from direction and speed
        Vector3 horizontalMove = momentumDirection * currentSpeed;

        // Move Sonic
        controller.Move((horizontalMove + velocity) * Time.deltaTime);

        // Animation control
        animator.SetBool("isRunning", isMoving && controller.isGrounded);
    }
}
