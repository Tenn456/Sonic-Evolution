using Unity.VisualScripting;
using UnityEngine;

public class Sonic : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 20f;        // Maximum running speed (non-boost)
    public float acceleration = 30f;    // How fast Sonic accelerates when running
    public float deceleration = 5f;     // How fast Sonic slows when no input
    public float brake = 25f;           // Strong deceleration when reversing direction

    [Header("Boost")]
    public KeyCode boostKey = KeyCode.LeftShift; // Keyboard boost key
    public string boostButton = "joystick button 0"; // Controller boost button
    public float boostMaxSpeed = 35f;            // Max speed while boosting
    public float boostAcceleration = 55f;        // Acceleration while boosting
    public float initialBoostDrain = 4f;         // Initial cose of boost
    public float boostDrainPerSecond = 5f;      // Boost meter drain per second
    [Range(0f, 100f)] public float boostMeterMax = 100f; // Max boost meter
    public float boostCancelSpeed = 10f;

    [Header("Jump Settings")]
    public KeyCode jumpKey = KeyCode.Space; // Keyboard jump key
    public string jumpButton = "joystick button 1"; // Controller jump button
    public float jumpForce = 12f;         // Initial upward jump velocity
    public float lowJumpMultiplier = 3f;  // Reduces jump height when releasing jump early
    public float fallMultiplier = 2f;     // Makes falling faster than rising
    public float gravity = -30f;          // Constant downward acceleration

    [Header("Spindash")]
    public KeyCode spindashKey = KeyCode.LeftControl;     // Keyboard spindash button
    public string spindashButton = "joystick button 7";  // Controller spindash button
    public KeyCode unrollKey = KeyCode.CapsLock;        // Keyboard unroll button 
    public string unrollButton = "joystick button 6";   // Controller unroll button

    public float spindashMinSpeed = 12f;   // Launch speed at minimum charge
    public float spindashMaxSpeed = 35f;   // Launch speed at full charge
    public float spindashChargeRate = 1.5f; // Charge buildup per second
    public float spindashFrictionWhileCharging = 25f; // Stops sliding while charging

    [Header("Spindash Rolling")]
    public float spindashRollFriction = 8f; // slows roll down over time
    public float spindashExitSpeed = 8f;    // uncurl when speed <= this
    

    [Header("Turn Rate Limit (deg/sec)")]
    public float turnRateAtLowSpeed = 720f;   // Degrees per second when slow
    public float turnRateAtHighSpeed = 720f;  // Degrees per second when fast
    public float rollTurnRateAtLowSpeed = 1080f; // Rolling turn rate when slow
    public float rollTurnRateAtHighSpeed = 360f; // Rolling turn rate when fast
    public float boostTurnRate = 360f;

    [Header("Stomp")]
    public KeyCode stompKey = KeyCode.C;             // Keyboard stomp button
    public string stompButton = "joystick button 2"; // Controller stomp button
    public float stompSpeed = -45f;                  // downward velocity when stomping
    public float stompStickDownForce = -2f;

    [Header("Drop Dash")]
    public float dropDashSpeed = 28f;

    [Header("Quick Step")]
    public KeyCode quickStepLeftKey = KeyCode.Q;
    public KeyCode quickStepRightKey = KeyCode.E;
    public string quickStepLeftButton = "joystick button 4";
    public string quickStepRightButton = "joystick button 5";

    [Header("Hurt")]
    public float hurtKnockbackSpeed = 12f;
    public float hurtUpwardForce = 8f;
    public float hurtInvincibilityTime = 1.5f;

    private bool hurt;
    private float invincibilityTimer;

    public float quickStepDistance = 3f;      // how far Sonic shifts sideways
    public float quickStepDuration = 0.12f;   // how fast the shift happens
    public float quickStepCooldown = 0.2f;    // delay before next quick step

    private bool quickStepping;
    private Vector3 quickStepVelocity;
    private float quickStepTimer;
    private float quickStepCooldownTimer;

    private CharacterController controller;
    private Animator animator;

    // Movement
    private Vector3 velocity;           // Vertical velocity (gravity/jump)
    private float currentSpeed;          // Horizontal speed magnitude
    private Vector3 momentumDirection;   // Direction Sonic is moving

    // Environment
    private bool grounded;
    private bool wasGrounded;
    private bool hitWall;
    private bool blockedForward;

    // Boost
    private float boostMeter;            // Current boost meter value
    private bool boosting;
    private bool wasBoosting;
    private bool boostNeedsNewPress;

    // Spindash
    private bool wasSpindashHeld;         
    private float spindashCharge01;       // Charge amount (0–1)
    private bool spindashCharging;
    private bool spindashRolling;
    private bool dropDashCharging;
    private bool spindashNeedsNewPress;

    // Stomp
    private bool stomping;

    // UI
    public float Boost01 => (boostMeterMax <= 0f) ? 0f : (boostMeter / boostMeterMax);

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        boostMeter = boostMeterMax;                        // Start with full boost
        momentumDirection = transform.forward;             // Initial facing direction
    }

    void Update()
    {
        // Remember last frame's ground state
        wasGrounded = grounded;

        if (quickStepCooldownTimer > 0f)
        {
            quickStepCooldownTimer -= Time.deltaTime;
        }

        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }


        // Input stuff
        float h = hurt ? 0f : Input.GetAxis("Horizontal");  // Left/right input
        float v = hurt ? 0f : Input.GetAxis("Vertical");    // Forward/back input

        Vector2 raw = new Vector2(h, v);       // Combine into 2D vector
        float inputStrength = Mathf.Clamp01(raw.magnitude); // Analog strength (0–1)

        bool boostHeld = Input.GetKey(boostKey) || Input.GetKey(boostButton);

        if (!boostHeld)
        {
            boostNeedsNewPress = false;
        }

        // Unroll input
        bool unroll = Input.GetKeyDown(unrollButton) || Input.GetKeyDown(unrollKey);

        // Spindash input
        bool spindashHeld = (Input.GetKey(spindashKey) || Input.GetKey(spindashButton)) && !unroll;


        bool spindashReleased = !spindashHeld && wasSpindashHeld; // Detect release
        wasSpindashHeld = spindashHeld;                           // Store for next frame

        if (!spindashHeld)
        {
            spindashNeedsNewPress = false;
        }

        // Start charging drop dash only while airborne
        if (!grounded && spindashHeld && !spindashRolling && !spindashCharging)
        {
            dropDashCharging = true;
            spindashNeedsNewPress = true;
        }

        // If player releases button before landing, cancel drop dash
        if (!grounded && dropDashCharging && spindashReleased)
        {
            dropDashCharging = false;
        }

        bool quickStepLeftPressed = Input.GetKeyDown(quickStepLeftKey) || Input.GetKeyDown(quickStepLeftButton);

        bool quickStepRightPressed = Input.GetKeyDown(quickStepRightKey) || Input.GetKeyDown(quickStepRightButton);

        // Camera stuff
        Vector3 camForward = Camera.main.transform.forward; // Camera forward
        Vector3 camRight = Camera.main.transform.right;     // Camera right

        camForward.y = 0f;  // Remove vertical tilt
        camRight.y = 0f;

        camForward.Normalize(); // Normalize direction
        camRight.Normalize();

        Vector3 inputDir = (camForward * v + camRight * h).normalized; // Convert input to world space

        bool hasInput = inputDir.sqrMagnitude > 0.01f; // Ignore tiny drift

        // Boost stuff
        bool canBoost = boostMeter > 0.01f && grounded && !spindashCharging;

        boosting = boostHeld && canBoost && !boostNeedsNewPress && !hurt;

        bool boostStarted = boosting && !wasBoosting;

        if (boostStarted)
        {
            if (spindashRolling)
            {
                spindashRolling = false;
            }
            currentSpeed = Mathf.Max(currentSpeed, boostMaxSpeed);          // Instant boost speed
            boostMeter = Mathf.Max(0f, boostMeter - initialBoostDrain);     // Initial boost meter cost
        }

        wasBoosting = boosting; // Check for boostStarted

        if (boosting)
        {
            boostMeter = Mathf.Max(0f, boostMeter - boostDrainPerSecond * Time.deltaTime);  // Constant boost meter drain
        } 

        float activeMaxSpeed = boosting ? boostMaxSpeed : maxSpeed;                 // If Sonic is boosting, use boost top speed, else use normal top speed
        float activeAcceleration = boosting ? boostAcceleration : acceleration;     // If Sonic is boosting, use boost accel, else use normal accel

        // If boosting with no stick input, propel forward
        if (boosting && !hasInput)
        {
            hasInput = true;
            inputStrength = 1f;
            inputDir = transform.forward;
        }

        float targetMax = activeMaxSpeed * inputStrength;   // Sets max speed based on input strenth

        // Ensure momentumDirection is never zero
        if (momentumDirection.sqrMagnitude < 0.001f)
        {
            momentumDirection = transform.forward;
        }

        bool hasMomentum = momentumDirection.sqrMagnitude > 0.001f;
        float align = (hasInput && hasMomentum) ? Vector3.Dot(momentumDirection, inputDir) : 1f;    // Calculates how aligned the player input direction is with Sonic's current direction
        bool braking = hasInput && hasMomentum && align < -0.2f;                                    // True if player is holding opposite direction

        // Spindash stuff
        if (grounded && spindashHeld && !spindashNeedsNewPress && !hurt)
        {
            // Set states
            spindashCharging = true;
            spindashRolling = false;

            // Increase charge overtime
            spindashCharge01 += spindashCharge01 < 1f ? spindashChargeRate * Time.deltaTime : 0f;
            spindashCharge01 = Mathf.Clamp01(spindashCharge01);

            // Stops Sonic from moving while charging
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, spindashFrictionWhileCharging * Time.deltaTime);

            // Face direction of player input
            momentumDirection = inputDir;
        }
        else
        {
            // Release -> start rolling
            if (grounded && spindashCharging && spindashReleased)
            {
                float launchSpeed = Mathf.Lerp(spindashMinSpeed, spindashMaxSpeed, spindashCharge01);   // Convert charge amount into launch speed
                
                // Launch Sonic forward
                momentumDirection = transform.forward;
                currentSpeed = Mathf.Max(currentSpeed, launchSpeed);

                // Set states
                spindashRolling = true;
                spindashCharging = false;

                spindashCharge01 = 0f;  // Reset charge
            }
            else if (!grounded)
            {
                // Make sure to not spindash in air
                spindashCharging = false;
                spindashCharge01 = 0f;
            }
            else
            {
                spindashCharging = false;
            }
        }

        // Turning stuff
        if (hasInput && !spindashCharging)
        {
            bool allowSteer = spindashRolling || !braking;

            if (allowSteer)
            {
                if (!hasMomentum)
                {
                    momentumDirection = inputDir;   // Sets movement direction to input direction
                }
                else
                {
                    // Setting turn rate (normal, boosting, spindash)
                    float low = 0;
                    float high = 0;

                    if (boosting || spindashRolling)
                    {
                        if (boosting)
                        {
                            high = boostTurnRate;
                        }
                        else if (spindashRolling)
                        {
                            low = rollTurnRateAtLowSpeed;
                            high = rollTurnRateAtHighSpeed;
                        }
                    }
                    else
                    {
                        low = turnRateAtLowSpeed;
                        high = turnRateAtHighSpeed;
                    }

                    float maxRefSpeed = (boosting ? boostMaxSpeed : maxSpeed);                                  // Sets refernce speed for current state
                    float speed01 = (maxRefSpeed > 0.001f) ? Mathf.Clamp01(currentSpeed / maxRefSpeed) : 0f;    // Normalize speed to 0-1 range

                    float turnRateDeg = Mathf.Lerp(low, high, speed01);                                         // Interpolate turn rate based on speed
                    float maxRadians = turnRateDeg * Mathf.Deg2Rad * Time.deltaTime;                            // Convert degrees per second into radians for this frame

                    Vector3 newDir = Vector3.RotateTowards(momentumDirection, inputDir, maxRadians, 0f);        // Gradually rotate momentum direction toward input direction
                    newDir.y = 0f;                                                                              // Prevent vertical tilting

                    // Safety net
                    if (newDir.sqrMagnitude > 0.0001f)
                    {
                        momentumDirection = newDir.normalized;
                    }
                }
            }
        }

        // Speed stuff

        // Braking
        if (braking && !spindashCharging && !spindashRolling)
        {
            // Apply strong brake
            currentSpeed -= brake * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f);

            // Stops boosting
            if (boosting)
            {
                boosting = false;
            }

            // Allow Sonic to turn around if he is slow enough
            if (currentSpeed < 1.0f)
            {
                momentumDirection = inputDir;
            }
        }
        // Spindashing
        else if (spindashRolling)
        {
            // Apply rolling decceleration
            currentSpeed -= spindashRollFriction * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f);

            // Exit Spindash rolling state when slow enough
            if (currentSpeed <= spindashExitSpeed)
            {
                spindashRolling = false;
            }

        }
        // Running
        else if (hasInput && !spindashCharging)
        {
            if (!blockedForward)
            {
                // If Sonic is below his max speed, accelerate
                if (currentSpeed < targetMax)
                {
                    currentSpeed += activeAcceleration * Time.deltaTime;
                    currentSpeed = Mathf.Min(currentSpeed, targetMax);
                }
                // If Sonic is above his max speed, decelerate (ex. Boosting -> Running)
                else if (currentSpeed > targetMax)
                {
                    currentSpeed -= deceleration * Time.deltaTime;
                    currentSpeed = Mathf.Max(currentSpeed, 0f);
                }
            }
        }
        // No Input but still moving
        else if (hasMomentum)
        {
            // Decelerate
            currentSpeed -= deceleration * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f);
        }

        // Rotate Sonic torwards momentum direction
        if (momentumDirection.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(momentumDirection, Vector3.up);                  // Create a rotation that looks in the momentum direction
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);     // Smoothly rotate toward that direction
        }

        // Start quick step
        if (!quickStepping && quickStepCooldownTimer <= 0f && grounded && !spindashCharging && !spindashRolling && currentSpeed > 10f)
        {
            if (quickStepLeftPressed)
            {
                quickStepping = true;
                quickStepTimer = quickStepDuration;
                quickStepCooldownTimer = quickStepCooldown;

                // Move left relative to Sonic's facing direction
                quickStepVelocity = -transform.right * (quickStepDistance / quickStepDuration);
            }
            else if (quickStepRightPressed)
            {
                quickStepping = true;
                quickStepTimer = quickStepDuration;
                quickStepCooldownTimer = quickStepCooldown;

                // Move right relative to Sonic's facing direction
                quickStepVelocity = transform.right * (quickStepDistance / quickStepDuration);
            }
        }

        Vector3 quickStepMove = Vector3.zero;

        if (quickStepping)
        {
            quickStepMove = quickStepVelocity;

            quickStepTimer -= Time.deltaTime;
            if (quickStepTimer <= 0f)
            {
                quickStepping = false;
                quickStepVelocity = Vector3.zero;
            }
        }

        // Jump and gravity stuff
        bool jumpPressed = Input.GetKeyDown(jumpKey) || Input.GetKeyDown(jumpButton);
        bool jumpHeld = Input.GetKey(jumpKey) || Input.GetKey(jumpButton);

        if (grounded)
        {
            // Makes sure to reset Jump Animation State
            if (animator.GetBool("Jump"))
            {
                animator.SetBool("Jump", false);
            }

            // Small downward force to keep grounded (Stickyness to ground)
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if (jumpPressed && !hurt)
            {
                // Apply jump force
                velocity.y = jumpForce;
                animator.SetBool("Jump", true);

                // Cancel spindash if doing it
                if (spindashRolling)
                {
                    spindashRolling = false;
                        
                }

            }

            if (unroll)
            {
                if (spindashRolling)
                {
                    spindashRolling = false;
                }
                else
                {
                    spindashRolling = true;
                }
            }
        }
        else
        {
            // Short hop if jump button released early
            if (!jumpHeld && velocity.y > 0)
            {
                velocity.y -= lowJumpMultiplier * Time.deltaTime;
            }
            
            // Faster fall for better jump feel
            if (velocity.y < 0)
            {
                velocity.y += gravity * (fallMultiplier - 1f) * Time.deltaTime;
            }    
        }

        // Apply base gravity every frame
        velocity.y += gravity * Time.deltaTime;

        // Stomp Stuff
        bool stompPressed = (Input.GetKeyDown(stompKey) || Input.GetKeyDown(stompButton));

        // Start stomp only while airborne
        if (!grounded && stompPressed && !hurt)
        {
            stomping = true;

            // Cancel upward motion immediately and force downward drop
            velocity.y = stompSpeed;

            // Animation change
            if (animator)
            {
                if (animator.GetBool("Jump"))
                {
                    animator.SetBool("Jump", false);
                }
            }


        }

        if (stomping)
        {
            // Force a consistent downward velocity (straight down)
            velocity.y = stompSpeed;
        }

        // Movement
        Vector3 horizontalMove = momentumDirection * currentSpeed;                          // Calculate horizontal movement vector
        Vector3 move = (horizontalMove + quickStepMove + velocity) * Time.deltaTime;        // Combine horizontal movement and vertical velocity

        CollisionFlags flags = controller.Move(move);                       // Move the CharacterController and get collision info

        grounded = (flags & CollisionFlags.Below) != 0;
        hitWall = (flags & CollisionFlags.Sides) != 0;

        // If Sonic was launched by enemy and just landed, stop all movement
        if (hurt && grounded)
        {
            currentSpeed = 0f;
            velocity.y = -2f;
            hurt = false;
        }

        if (grounded && !wasGrounded && dropDashCharging)
        {
            // Launch in the direction Sonic is facing
            momentumDirection = transform.forward;

            // Apply constant drop dash speed
            currentSpeed = Mathf.Max(currentSpeed, dropDashSpeed);

            // Exit drop dash state
            dropDashCharging = false;

            // Keep this true so holding the button does NOT start a grounded spindash
            spindashNeedsNewPress = true;

            // Enter roll state
            spindashRolling = true;
        }

        // Stop Stomping once grounded
        if (grounded && stomping)
        {
            stomping = false;

            // Kill momentum
            currentSpeed = 0f;

            // Stick to ground after impact
            if (velocity.y < 0f)
            {
                velocity.y = stompStickDownForce;
            }
        }

        // Wall detection
        Vector3 hv = controller.velocity;                                   // Get velocity from CharacterController
        hv.y = 0f;                                                          // Ignore vertical movement

        blockedForward = hitWall && hv.magnitude < currentSpeed * 0.25f;    // If hitting a wall and barely moving forward, consider movement blocked

        if (blockedForward)
        {
            // If pushing into wall, kill speed
            currentSpeed = 0;
        }

        float turnAmount = 0f;

        if (hasInput && momentumDirection.sqrMagnitude > 0.001f && currentSpeed > 1f)
        {
            turnAmount = Vector3.Dot(transform.right, inputDir);
            turnAmount = Mathf.Clamp(turnAmount, -1f, 1f);
        }

        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);

        // Smoothly send to Animator
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);



        float speed = hv.magnitude;

        // Force stop boost if Sonic slows down too much
        if (boosting && speed < boostCancelSpeed)
        {
            boosting = false;
            wasBoosting = false;
            boostNeedsNewPress = true;
        }

        // Animation stuff
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Boosting", boosting);
        animator.SetBool("SpindashCharge", spindashCharging || dropDashCharging);
        animator.SetBool("Spindash", spindashRolling);
        animator.SetBool("Stomping", stomping);
    }

    public void TakeDamage(Vector3 enemyPosition)
    {
        if (invincibilityTimer > 0f)
            return;

        RingCounter ringCounter = GetComponent<RingCounter>();
        if (ringCounter != null)
        {
            ringCounter.LoseAllRings();
        }

        hurt = true;
        invincibilityTimer = hurtInvincibilityTime;

        // Knockback direction away from enemy
        Vector3 knockDir = (transform.position - enemyPosition).normalized;
        knockDir.y = 0f;

        if (knockDir.sqrMagnitude < 0.001f)
            knockDir = -transform.forward;

        // Apply horizontal knockback
        momentumDirection = knockDir;
        currentSpeed = hurtKnockbackSpeed;

        // Apply upward pop
        velocity.y = hurtUpwardForce;

        //if (animator)
            //animator.SetTrigger("Hurt");
    }
}
