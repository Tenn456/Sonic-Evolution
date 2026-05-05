using UnityEngine;
using Unity.Cinemachine;

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

    [Header("Power Drift")]
    private float powerDriftSpeed;
    public bool powerDrifting;
    public float powerDriftDeceleration = 5f;
    public float powerDriftTurnRate = 540f;
    private Vector3 powerDriftMoveDirection;
    private Vector3 powerDriftFacingDirection;

    public float stumbleDuration = 0.35f;
    public float stumbleSpeed = 4f;
    public float stumbleStopThreshold = 0.5f;
    private float powerDriftHoldTimer;
    public float stumbleHoldThreshold = 1f;

    private bool stumbling;
    private float stumbleTimer;
    private Vector3 stumbleDirection;

    private float postDriftTurnLockTimer;
    public float postDriftTurnLockDuration = 0.15f;

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

    public float quickStepDistance = 3f;      // how far Sonic shifts sideways
    public float quickStepDuration = 0.12f;   // how fast the shift happens
    public float quickStepCooldown = 0.5f;    // delay before next quick step

    public float postQuickStepTurnLockTimer;
    public float postQuickStepTurnLockDuration;

    [Header("Homing Attack")]
    public float homingRadius = 6f;
    public float homingSpeed = 35f;
    public float homingHitDistance = 1.5f;
    public LayerMask homingTargetMask;
    public float homingForwardDot = 0.2f;   // how far in front target must be
    public float bounceAmount = 30;
    public float homingDuration = 4f;
    public LayerMask homingBlockMask;

    private float homingTimer; 

    private bool homingAttacking;
    public Transform homingTarget;
    private Transform lastHomingTarget;
    public Transform CurrentHomingTarget => homingTarget;
    private bool doubleJump;

    [Header("Hurt")]
    public float hurtKnockbackSpeed = 12f;
    public float hurtUpwardForce = 8f;
    public float hurtInvincibilityTime = 1.5f;

    [Header("Ring Drop")]
    public GameObject ringPrefab;
    public int maxRingsToDrop = 20;
    public float ringDropForce = 8f;
    public float ringUpwardForce = 5f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.3f;
    public float groundCheckDistance = 0.25f;
    public float groundCheckOffset = 0.1f;

    public bool grounded;

    public TrailRenderer homingTrail;
    public TrailRenderer stompTrail;
    public TrailRenderer boostTrail;

    private bool hurt;
    private float invincibilityTimer;
    private bool hurtFaceEnemy;
    private Vector3 hurtEnemyPosition;

    private bool quickStepping;
    private Vector3 quickStepVelocity;
    private float quickStepTimer;
    private float quickStepCooldownTimer;

    private CharacterController controller;
    public Animator animator;

    // Movement
    public Vector3 velocity;           // Vertical velocity (gravity/jump)
    private float currentSpeed;          // Horizontal speed magnitude
    private Vector3 momentumDirection;   // Direction Sonic is moving

    // Environment
    //private bool grounded;
    private bool wasGrounded;
    private bool hitWall;
    private bool blockedForward;

    // Boost
    private float boostMeter;            // Current boost meter value
    public bool boosting;
    private bool wasBoosting;
    private bool boostNeedsNewPress;

    // Spindash
    private bool wasSpindashHeld;
    private float spindashCharge01;       // Charge amount (0–1)
    public bool spindashCharging;
    public bool spindashRolling;
    public bool dropDashCharging;
    private bool spindashNeedsNewPress;

    // Stomp
    public bool stomping;

    // UI
    public float Boost01 => (boostMeterMax <= 0f) ? 0f : (boostMeter / boostMeterMax);

    public bool jumping;

    private float accelerationStart;

    private bool dropDashNeedsNewPress;

    private float speed;

    public bool dead;
    public bool PowerDrifting => powerDrifting;

    public AudioSource voiceAudioSource;
    public AudioClip[] jumpClips;
    public AudioClip[] attackClips;
    public AudioClip[] hurtClips;
    public AudioClip[] cheerClips;

    public AudioSource sonicAudioSource;
    public AudioClip jumpClip;
    public AudioClip doubleJumpClip;
    public AudioClip boostClip;
    public AudioClip homingAttackClip;
    public AudioClip spindashClip;
    public AudioClip loseRingClip;
    public AudioClip dropDashClip;
    public AudioClip rollClip;
    public AudioClip stompClip;
    public AudioClip wooshClip;
    public AudioClip lockOnClip;
    public AudioClip landClip;
    public AudioClip deathClip;

    public AudioSource boostWindAudioSource;
    public AudioSource spindashChargeAudioSource;
    public AudioSource stompingAudioSource;
    public AudioSource driftAudioSource;

    public PostProcessManager post;

    public CinemachineCamera cam;

    public float normalFov = 60f;
    public float boostFov = 75f;
    public float fovTransitionTime = 5f;

    public bool invincible;


    void Start()
    {
        controller = GetComponent<CharacterController>();

        boostMeter = boostMeterMax;                        // Start with full boost
        momentumDirection = transform.forward;             // Initial facing direction
        accelerationStart = acceleration;
    }

    void Update()
    {
        grounded = CheckGrounded();

        if (grounded && !wasGrounded)
        {
            sonicAudioSource.PlayOneShot(landClip);

            // State changes
            jumping = false;
            homingAttacking = false;
            homingTarget = null;
            doubleJump = false;
        }

        // Update wasGrounded
        wasGrounded = grounded;

        if (postDriftTurnLockTimer > 0f)
        {
            postDriftTurnLockTimer -= Time.deltaTime;
        }

        if (postQuickStepTurnLockTimer > 0f)
        {
            postQuickStepTurnLockTimer -= Time.deltaTime;
        }

        // Don't accelerate too fast in the air
        if (!grounded)
        {
            acceleration = 3;
        }
        else
        {
            acceleration = accelerationStart;
        }

        if (quickStepCooldownTimer > 0f)
        {
            quickStepCooldownTimer -= Time.deltaTime;
        }

        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }


        // Input stuff
        float h = (hurt || stumbling || dead) ? 0f : Input.GetAxis("Horizontal");  // Left/right input
        float v = (hurt || stumbling || dead) ? 0f : Input.GetAxis("Vertical");    // Forward/back input

        Vector2 raw = new Vector2(h, v);       // Combine into 2D vector
        float inputStrength = Mathf.Clamp01(raw.magnitude); // Analog strength (0–1)

        bool boostHeld = Input.GetKey(boostKey) || Input.GetKey(boostButton);

        if (!boostHeld)
        {
            boostNeedsNewPress = false;
        }

        // Unroll input
        bool unroll = Input.GetKeyDown(unrollButton) || Input.GetKeyDown(unrollKey) && grounded && !dead;

        // Spindash input
        bool spindashHeld = (Input.GetKey(spindashKey) || Input.GetKey(spindashButton)) && !unroll && !dead;


        bool spindashReleased = !spindashHeld && wasSpindashHeld; // Detect release
        wasSpindashHeld = spindashHeld;                           // Store for next frame

        if (!spindashHeld && grounded)
        {
            spindashNeedsNewPress = false;
            dropDashNeedsNewPress = false;
        }

        // Start charging drop dash only while airborne
        if (!grounded && spindashHeld && !dropDashNeedsNewPress && !spindashRolling && !spindashCharging && !hurt && !powerDrifting)
        {
            dropDashCharging = true;
            spindashNeedsNewPress = true;
            dropDashNeedsNewPress = true;

            // Sound Effect
            sonicAudioSource.PlayOneShot(dropDashClip);
        }

        // If player releases button before landing, cancel drop dash
        if (!grounded && dropDashCharging && spindashReleased)
        {
            dropDashCharging = false;
        }

        bool quickStepLeftPressed = (Input.GetKeyDown(quickStepLeftKey) || Input.GetKeyDown(quickStepLeftButton)) && !dead && !stumbling && !hurt;

        bool quickStepRightPressed = (Input.GetKeyDown(quickStepRightKey) || Input.GetKeyDown(quickStepRightButton)) && !dead && !stumbling && !hurt;

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
        bool canBoost = boostMeter > 0.01f && grounded && !spindashCharging && !powerDrifting &&!dead && !stumbling && !hurt;

        boosting = boostHeld && canBoost && !boostNeedsNewPress && !hurt && !stumbling;

        bool boostStarted = boosting && !wasBoosting;

        if (boostStarted)
        {
            if (spindashRolling)
            {
                spindashRolling = false;
            }
            currentSpeed = Mathf.Max(currentSpeed, boostMaxSpeed);          // Instant boost speed
            boostMeter = Mathf.Max(0f, boostMeter - initialBoostDrain);     // Initial boost meter cost

            boostTrail.Clear();
            boostTrail.emitting = true;

            // Voiceline
            PlayRandomAttack();
            // Sound Effect
            sonicAudioSource.PlayOneShot(boostClip);
            boostWindAudioSource.Play();
        }

        wasBoosting = boosting; // Check for boostStarted

        float current = cam.Lens.FieldOfView;

        if (boosting)
        {
            boostMeter = Mathf.Max(0f, boostMeter - boostDrainPerSecond * Time.deltaTime);  // Constant boost meter drain

            // Post Processing
            post.Boost();
            cam.Lens.FieldOfView = Mathf.Lerp(current, boostFov, Time.deltaTime * fovTransitionTime);
        }
        else
        {
            boostWindAudioSource.Stop();

            boostTrail.emitting = false;

            // Post Processing
            post.Normal();
            cam.Lens.FieldOfView = Mathf.Lerp(current, normalFov, Time.deltaTime * fovTransitionTime);
        }

        bool hasMomentum = momentumDirection.sqrMagnitude > 0.001f;
        float align = (hasInput && hasMomentum) ? Vector3.Dot(momentumDirection, inputDir) : 1f;    // Calculates how aligned the player input direction is with Sonic's current direction
        bool turning = hasInput && hasMomentum && align < 0.99f;                                     // Checks if turning Sonic
        bool braking = hasInput && hasMomentum && align < -0.2f;                                    // True if player is holding opposite direction

        float activeMaxSpeed = boosting ? boostMaxSpeed : maxSpeed;                 // If Sonic is boosting, use boost top speed, else use normal top speed

        float activeAcceleration = turning ? 0f : boosting ? boostAcceleration : acceleration;     // If Sonic is turning, no accel, else if boosting, use boost accel, else use normal accel

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

        // Spindash stuff
        if (grounded && spindashHeld && !spindashNeedsNewPress && !hurt && currentSpeed <= 5 && !powerDrifting)
        {
            if (!spindashCharging)
            {
                spindashCharging = true;
                spindashRolling = false;

                // Sound Effect
                spindashChargeAudioSource.Play();
            }


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

                // Sound Effect
                spindashChargeAudioSource.Stop();
                sonicAudioSource.PlayOneShot(spindashClip);
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

        // Power Drift stuff 
        if (grounded && spindashHeld && !spindashNeedsNewPress && !hurt && currentSpeed > 5 && !stumbling)
        {
            // Enter power drift state if not already
            if (!powerDrifting)
            {
                powerDrifting = true;
                powerDriftSpeed = currentSpeed;
                powerDriftHoldTimer = 0f;

                // Get two instances of direction
                powerDriftMoveDirection = momentumDirection.normalized;
                powerDriftFacingDirection = momentumDirection.normalized;

                if (spindashRolling)
                {
                    spindashRolling = false; 
                }
                
                // Sound effect
                driftAudioSource.Play();
                PlayRandomAttack();
            }

            // Timer to check how long button is held
            powerDriftHoldTimer += Time.deltaTime;

            // Deccelerate Sonic to line up drift
            currentSpeed -= powerDriftDeceleration * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f);

            // If Sonic slows down too much, enter stumble state
            if ((currentSpeed <= stumbleStopThreshold))
            {
                powerDrifting = false;

                stumbling = true;
                stumbleTimer = stumbleDuration;
                stumbleDirection = powerDriftFacingDirection.normalized;
                currentSpeed = stumbleSpeed;

                spindashNeedsNewPress = true;

                PlayRandomHurt();
                driftAudioSource.Stop();
            }

            // Allow turning freely while drifting without changing Sonic's velocity
            if (hasInput)
            {
                float maxRadians = powerDriftTurnRate * Mathf.Deg2Rad * Time.deltaTime;
                Vector3 newFacing = Vector3.RotateTowards(powerDriftFacingDirection, inputDir, maxRadians, 0f);
                newFacing.y = 0f;

                if (newFacing.sqrMagnitude > 0.0001f)
                {
                    powerDriftFacingDirection = newFacing.normalized;
                }
            }

            // Keep velocity locked
            momentumDirection = powerDriftMoveDirection;
        }

        // If releasing the power drift button
        if (powerDrifting && spindashReleased)
        {
            powerDrifting = false;
            spindashNeedsNewPress = true;

            // If the button is released too quickly, enter stumble state
            if (!stumbling && powerDriftHoldTimer < stumbleHoldThreshold)
            {
                stumbling = true;
                stumbleTimer = stumbleDuration;
                stumbleDirection = powerDriftFacingDirection.normalized;
                currentSpeed = stumbleSpeed;

                // Sound Effect
                driftAudioSource.Stop();
                PlayRandomHurt();
            }
            else
            {
                // To lock direction so that camera swap does not influence Sonic's launch direction
                postDriftTurnLockTimer = postDriftTurnLockDuration;

                // Launch in facing direction
                if (powerDriftFacingDirection.sqrMagnitude > 0.0001f)
                {
                    momentumDirection = powerDriftFacingDirection.normalized;
                }

                // Use conserved speed
                currentSpeed = powerDriftSpeed;

                // Sound Effect
                driftAudioSource.Stop();
                sonicAudioSource.PlayOneShot(spindashClip);
                PlayRandomCheer();
            }
        }

        // Stumble state
        if (stumbling)
        {
            stumbleTimer -= Time.deltaTime;

            // Small push in current facing direction
            momentumDirection = stumbleDirection;
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, 12f * Time.deltaTime);

            // Stay in stumble state until time finishes
            if (stumbleTimer <= 0f)
            {
                stumbling = false;
                currentSpeed = 0f;
            }
        }

        // Quick Step Stuff
        if (!quickStepping && quickStepCooldownTimer <= 0f && grounded && !spindashCharging && !spindashRolling && currentSpeed > 10f && !powerDrifting)
        {
            if (quickStepLeftPressed)
            {
                quickStepping = true;
                quickStepTimer = quickStepDuration;
                quickStepCooldownTimer = quickStepCooldown;

                // Move left relative to Sonic's facing direction
                quickStepVelocity = -transform.right * (quickStepDistance / quickStepDuration);

                // To lock direction so Sonic doesn't turn while quick stepping
                postQuickStepTurnLockTimer = postQuickStepTurnLockDuration;

                // Sound Effect
                sonicAudioSource.PlayOneShot(wooshClip);

                // Voiceclip
                PlayRandomJump();
            }
            else if (quickStepRightPressed)
            {
                quickStepping = true;
                quickStepTimer = quickStepDuration;
                quickStepCooldownTimer = quickStepCooldown;

                // Move right relative to Sonic's facing direction
                quickStepVelocity = transform.right * (quickStepDistance / quickStepDuration);

                // To lock direction so Sonic doesn't turn while quick stepping
                postQuickStepTurnLockTimer = postQuickStepTurnLockDuration;

                // Sound Effect
                sonicAudioSource.PlayOneShot(wooshClip);

                // Voiceclip
                PlayRandomJump();
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

        // Turning stuff
        if (hasInput && !spindashCharging && !homingAttacking && !quickStepping && !powerDrifting && postDriftTurnLockTimer <= 0f && postQuickStepTurnLockTimer <= 0f)
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
        if (braking && !spindashCharging && !spindashRolling && !powerDrifting && !stumbling)
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
        else if (spindashRolling && !powerDrifting && !stumbling)
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
        else if (hasInput && !spindashCharging && !homingAttacking && !powerDrifting && !stumbling)
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
        else if (hasMomentum && !stumbling && !powerDrifting)
        {
            // Decelerate
            currentSpeed -= deceleration * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f);
        }

        // Sonic Rotation
        if (hurtFaceEnemy) // Lock Sonic rotation towards enemy if hit
        {
            Vector3 faceDir = (hurtEnemyPosition - transform.position);
            faceDir.y = 0f;

            if (faceDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(faceDir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
            }
        }
        else if (powerDrifting && powerDriftFacingDirection.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(powerDriftFacingDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }
        else if (!quickStepping && momentumDirection.sqrMagnitude > 0.1f) // Rotate Sonic torwards momentum direction
        {
            Quaternion targetRot = Quaternion.LookRotation(momentumDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        // Jump and gravity stuff
        bool jumpPressed = (Input.GetKeyDown(jumpKey) || Input.GetKeyDown(jumpButton)) && !dead && !stumbling && !hurt;
        bool jumpHeld = (Input.GetKey(jumpKey) || Input.GetKey(jumpButton)) && !dead && !stumbling && !hurt;

        if (grounded)
        {
            // Small downward force to keep grounded (Stickyness to ground)
            if (velocity.y < 0)
            {
                velocity.y = -15f;
            }

            if (jumpPressed && !hurt && !stumbling)
            {
                // Apply jump force
                velocity.y = jumpForce;
                jumping = true;

                // Voiceclip
                PlayRandomJump();
                // Sound Effect
                sonicAudioSource.PlayOneShot(jumpClip);

                // Cancel spindash if doing it
                if (spindashRolling)
                {
                    spindashRolling = false;
                }

            }

            if (unroll && !boosting && !stomping && !powerDrifting && !stumbling && !hurt)
            {
                if (spindashRolling)
                {
                    spindashRolling = false;
                }
                else if (!spindashRolling && currentSpeed > spindashExitSpeed + 5)
                {
                    spindashRolling = true;

                    //Sound Effect
                    sonicAudioSource.PlayOneShot(rollClip);
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
            if (jumping && velocity.y < 0)
            {
                velocity.y += gravity * (fallMultiplier - 1f) * Time.deltaTime;
            }
        }

        // Apply base gravity every frame
        velocity.y += gravity * Time.deltaTime;

        // Homing Attack Stuff
        if (!grounded && !homingAttacking)
        {
            homingTarget = FindHomingTarget();

            if (homingTarget != lastHomingTarget)
            {
                // Sound Effect
                sonicAudioSource.PlayOneShot(lockOnClip);
            }

            lastHomingTarget = homingTarget;
        }
        else if (!homingAttacking)
        {
            homingTarget = null;
            lastHomingTarget = null;
        }



        // If jump is pressed in the air
        if (!grounded && jumpPressed && !hurt && !spindashCharging && !powerDrifting)
        {
            Transform target = FindHomingTarget();

            // If there is a target
            if (target != null)
            {
                // Set target and change state to homing attack
                homingTarget = target;
                homingAttacking = true;

                jumping = false;

                // Enable trail renderer
                homingTrail.Clear();
                homingTrail.emitting = true;

                // Cancel normal vertical motion
                velocity.y = 0f;

                // Start timer
                homingTimer = homingDuration;

                // Voiceline
                PlayRandomAttack();
                // Sound Effect
                sonicAudioSource.PlayOneShot(homingAttackClip);
            }
            else
            {
                // Double jump
                if (!doubleJump)
                {
                    doubleJump = true;
                    velocity.y = jumpForce;

                    // Voiceline
                    PlayRandomJump();
                    //Sound Effect
                    sonicAudioSource.PlayOneShot(doubleJumpClip);
                }
            }
        }

        if (homingAttacking)
        {
            if (grounded)
            {
                homingAttacking = false;
                currentSpeed = 0;

                homingTrail.emitting = false;

                return;
            }

            // Decrease timer every frame
            homingTimer -= Time.deltaTime;

            // Cancel if time runs out
            if (homingTimer <= 0f)
            {
                homingAttacking = false;
                currentSpeed = 0;

                homingTrail.emitting = false;

                return;
            }

            // If target stops existing for whatever reason, cancel homing attack
            if (homingTarget == null)
            {
                homingAttacking = false;
                currentSpeed = 0;

                homingTrail.emitting = false;
            }
            else
            {
                // Calculate target distance
                Vector3 toTarget = homingTarget.position - transform.position;
                float distance = toTarget.magnitude;

                // If Sonic reaches target
                if (distance <= homingHitDistance)
                {
                    // End homing attack
                    homingAttacking = false;
                    // Reset double jump
                    doubleJump = false;
                    // Give a small upward force (bounce)
                    velocity.y = bounceAmount;
                    currentSpeed = 5f;

                    homingTrail.emitting = false;

                    // Destroy Enemy
                    EnemyDeath enemy = homingTarget.GetComponent<EnemyDeath>();
                    if (enemy != null)
                    {
                        GainBoost(10f);
                        enemy.TakeHit();
                    }
                    else
                    {
                        //Destroy(homingTarget.gameObject);
                    }
                }
                else
                {
                    // Move towards target
                    Vector3 homingDir = toTarget.normalized;

                    // Set movement direction and speed
                    momentumDirection = new Vector3(homingDir.x, 0f, homingDir.z).normalized;
                    currentSpeed = homingSpeed;
                    velocity.y = homingDir.y * homingSpeed;
                }
            }
        }

        // Stomp Stuff
        bool stompPressed = (Input.GetKeyDown(stompKey) || Input.GetKeyDown(stompButton)) && !dead && !stumbling && !hurt;

        // Start stomp only while airborne
        if (!grounded && stompPressed && !hurt && !stomping)
        {
            // State changes
            jumping = false;
            stomping = true;

            // Trail
            stompTrail.Clear();
            stompTrail.emitting = true;

            // Voiceline
            PlayRandomJump();
            // Sound Effect
            stompingAudioSource.Play();
        }

        if (stomping)
        {
            // Force a consistent downward velocity (straight down)
            velocity.y = stompSpeed;
            currentSpeed = 0f;
        }

        // Movement
        Vector3 horizontalMove = momentumDirection * currentSpeed;                          // Calculate horizontal movement vector
        Vector3 move = (horizontalMove + quickStepMove + velocity) * Time.deltaTime;        // Combine horizontal movement and vertical velocity

        CollisionFlags flags = controller.Move(move);                       // Move the CharacterController and get collision info
        
        grounded = CheckGrounded();
        hitWall = (flags & CollisionFlags.Sides) != 0;

        if (powerDrifting && !grounded)
        {
            powerDrifting = false;
            spindashNeedsNewPress = true;

            driftAudioSource.Stop();
        }

        // If Sonic was launched by enemy and just landed, stop all movement
        if (hurt && grounded && !wasGrounded)
        {
            currentSpeed = 0f;
            velocity.y = -2f;
            hurt = false;
            hurtFaceEnemy = false;
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

            dropDashNeedsNewPress = true;

            // Enter roll state
            spindashRolling = true;

            // Sound Effect
            sonicAudioSource.PlayOneShot(spindashClip);
        }

        // Stop Stomping once grounded
        if (grounded && stomping)
        {
            stomping = false;

            stompTrail.emitting = false;

            // Kill momentum
            currentSpeed = 0f;

            // Stick to ground after impact
            if (velocity.y < 0f)
            {
                velocity.y = stompStickDownForce;
            }

            // Sound Effect
            stompingAudioSource.Stop();
            sonicAudioSource.PlayOneShot(stompClip);
        }

        // Wall detection
        Vector3 hv = controller.velocity;                                   // Get velocity from CharacterController
        hv.y = 0f;                                                          // Ignore vertical movement

        blockedForward = hitWall && hv.magnitude < currentSpeed * 0.25f;    // If hitting a wall and barely moving forward, consider movement blocked

        if (blockedForward)
        {
            // If pushing into wall, kill speed
            currentSpeed = 0;

            if (powerDrifting && !stumbling)
            {
                powerDrifting = false;

                stumbling = true;
                stumbleTimer = stumbleDuration;
                stumbleDirection = transform.forward;
                currentSpeed = stumbleSpeed;

                spindashNeedsNewPress = true;
                dropDashNeedsNewPress = true;

                // Sound Effects
                driftAudioSource.Stop();
                PlayRandomHurt();
            }
        }

        float turnAmount = 0f;

        if (hasInput && momentumDirection.sqrMagnitude > 0.001f && currentSpeed > 1f && postDriftTurnLockTimer < 0 && postQuickStepTurnLockTimer < 0 && !braking)
        {
            turnAmount = Vector3.Dot(transform.right, inputDir);
            turnAmount = Mathf.Clamp(turnAmount, -1f, 1f);
        }

        // Send to Animator
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);



        speed = hv.magnitude;

        // Force stop boost if Sonic slows down too much
        if (boosting && speed < boostCancelSpeed)
        {
            boosting = false;
            wasBoosting = false;
            boostNeedsNewPress = true;
        }

        if (boosting || jumping || spindashCharging || spindashRolling || dropDashCharging || stomping || invincibilityTimer > 0f || doubleJump)
        {
            invincible = true;
        }
        else
        {
            invincible = false;
        }
    }

    private void FixedUpdate()
    {
        // Animation stuff
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        animator.SetBool("Jump", jumping || doubleJump);
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Boosting", boosting);
        animator.SetBool("SpindashCharge", spindashCharging || dropDashCharging);
        animator.SetBool("Spindash", spindashRolling);
        animator.SetBool("Stomping", stomping);
        animator.SetBool("HomingAttacking", homingAttacking);
        animator.SetBool("Drifting", powerDrifting);
        animator.SetBool("Stumbling", stumbling || hurt);
        animator.SetBool("Dead", dead);
    }

    void OnDrawGizmosSelected()
    {
        // Make sure the controller exists
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (controller == null)
            return;

        Gizmos.color = Color.yellow;

        // Same values used in CheckGrounded()
        Vector3 origin = transform.position + controller.center * groundCheckOffset;
        float sphereRadius = groundCheckRadius;
        float castDistance = groundCheckDistance;

        // Draw the starting sphere
        Gizmos.DrawWireSphere(origin, sphereRadius);

        // Draw the ending sphere
        Vector3 end = origin + Vector3.down * castDistance;
        Gizmos.DrawWireSphere(end, sphereRadius);

        // Draw a line between them to visualize the cast path
        Gizmos.DrawLine(origin, end);

        // Homing Attack radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, homingRadius);
    }

    bool CheckGrounded()
    {
        Vector3 origin = transform.position + controller.center * groundCheckOffset;
        float sphereRadius = groundCheckRadius;
        float castDistance = groundCheckDistance;

        return Physics.SphereCast(
            origin,
            sphereRadius,
            Vector3.down,
            out _,
            castDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    public void TakeDamage(Vector3 enemyPosition)
    {
        // Ignore damage if invincible
        if (invincible)
        {
            return;
        }

        RingCounter ringCounter = GetComponent<RingCounter>();

        // If Sonic has no rings, die instead of dropping rings
        if (ringCounter.rings <= 0)
        {
            dead = true;
            hurt = true;
            hurtFaceEnemy = true;
            hurtEnemyPosition = enemyPosition;

            boosting = false;
            powerDrifting = false;
            spindashRolling = false;
            spindashCharging = false;
            homingAttacking = false;
            stomping = false;

            // knockback logic
            Vector3 knockDir = (transform.position - enemyPosition).normalized;
            knockDir.y = 0f;

            if (knockDir.sqrMagnitude < 0.001f)
            {
                knockDir = -transform.forward;
            }

            momentumDirection = knockDir;
            currentSpeed = hurtKnockbackSpeed;
            velocity.y = hurtUpwardForce;

            //PlayRandomHurt();
            sonicAudioSource.PlayOneShot(deathClip);

            if (DeathManager.Instance != null)
            {
                DeathManager.Instance.HandleDeath();
            }

            return;
        }

        // Lose Rings
        if (ringCounter != null)
        {
            //ringCounter.LoseAllRings();
            int ringAmount = ringCounter.rings; // or whatever your variable is
            ringCounter.LoseAllRings();

            DropRings(ringAmount);
        }

        // Enter hurt state
        hurt = true;
        hurtFaceEnemy = true;
        hurtEnemyPosition = enemyPosition;

        // Start hurt invincibility
        invincibilityTimer = hurtInvincibilityTime;

        // Calculate direction AWAY from enemy (knockback)
        Vector3 normalknockDir = (transform.position - enemyPosition).normalized;
        normalknockDir.y = 0f;

        // Safety fallback in case Sonic and enemy are in the same spot (No knockDir)
        if (normalknockDir.sqrMagnitude < 0.001f)
        {
            normalknockDir = -transform.forward;
        }

        // Apply knockback
        momentumDirection = normalknockDir;
        currentSpeed = hurtKnockbackSpeed;

        // Small upward pop
        velocity.y = hurtUpwardForce;

        // Voiceclip
        PlayRandomHurt();
        // Sound Effect
        sonicAudioSource.PlayOneShot(loseRingClip);
    }

    void DropRings(int amount)
    {
        int ringsToDrop = Mathf.Min(amount, maxRingsToDrop);

        for (int i = 0; i < ringsToDrop; i++)
        {
            float angle = (i / (float)ringsToDrop) * Mathf.PI * 2f;

            Vector3 spawnOffset = new Vector3(
                Mathf.Cos(angle),
                0f,
                Mathf.Sin(angle)
            ) * 0.5f;

            GameObject ring = Instantiate(
                ringPrefab,
                transform.position + Vector3.up * 1f + spawnOffset,
                Quaternion.identity
            );

            Rigidbody rb = ring.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 dir = spawnOffset.normalized;

                Vector3 force = dir * ringDropForce + Vector3.up * ringUpwardForce;

                rb.linearDamping = 2f;

                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }

    Transform FindHomingTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            homingRadius,
            homingTargetMask,
            QueryTriggerInteraction.Ignore
        );

        Transform bestTarget = null;
        float bestDistance = float.MaxValue;

        Vector3 origin = transform.position + Vector3.up * 1.0f;

        foreach (Collider hit in hits)
        {
            Vector3 targetPoint = hit.bounds.center;
            Vector3 toTarget = targetPoint - origin;
            float distance = toTarget.magnitude;

            if (distance <= 0.001f)
                continue;

            Vector3 dir = toTarget / distance;

            float dot = Vector3.Dot(transform.forward, dir);
            if (dot < homingForwardDot)
                continue;

            if (Physics.Raycast(
                    origin,
                    dir,
                    out RaycastHit wallHit,
                    distance,
                    homingBlockMask,
                    QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = hit.transform;
            }
        }

        return bestTarget;
    }

    public void GainBoost(float amount)
    {
        boostMeter += amount;
        boostMeter = Mathf.Clamp(boostMeter, 0f, boostMeterMax);
    }

    public void PlayRandomJump()
    {
        int index = Random.Range(0, jumpClips.Length);
        voiceAudioSource.PlayOneShot(jumpClips[index]);
    }

    public void PlayRandomAttack()
    {
        int index = Random.Range(0, attackClips.Length);
        voiceAudioSource.PlayOneShot(attackClips[index]);
    }

    public void PlayRandomHurt()
    {
        int index = Random.Range(0, hurtClips.Length);
        voiceAudioSource.PlayOneShot(hurtClips[index]);
    }

    public void PlayRandomCheer()
    {
        int index = Random.Range(0, cheerClips.Length);
        voiceAudioSource.PlayOneShot(cheerClips[index]);
    }
}
