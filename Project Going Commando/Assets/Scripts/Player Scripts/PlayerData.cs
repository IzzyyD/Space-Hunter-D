using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Gravity")]
    // downwards force needed for the desired jumpHeight & jumpTimeToApex   
    [HideInInspector] public float gravityStrength;  

    // strength of the player's gravity as a multiplier of gravity
    [HideInInspector] public float gravityScale; 

    [Space(5)]
    [Tooltip("Multiplier to the player's gravityScale when falling.")]
    public float fallGravityMultiplier;

    [Tooltip("Maximum fall speed of the player when falling.")]
    public float maxFallSpeed;    

    [Space(10)]
    [Header("Run")]
    [Tooltip("Target speed we want the player to reach.")]
    public float runMaxSpeed;

    [Tooltip("Speed at which the player accelerates to max speed, can be set to runMaxSpeed for instant acceleration down to 0 for none at all.")]
    public float runAcceleration;

    // actual force (multiplied by speedDiff) applied to the player
    [HideInInspector] public float runAccelAmount; 

    [Tooltip("Speed at which our player decelerates from their current speed, can be set to runMaxSpeed for instant deceleration down to 0 for none at all")]
    public float runDeceleration;

    // actual force (multiplied with speedDiff) applied to the player
    [HideInInspector] public float runDeccelAmount; 
    
    
    [Space(5)]
    [Tooltip("Multipliers applied to acceleration rate when airborne.")]
    [Range(0f, 1)] public float accelInAir;
    [Range(0f, 1)] public float deccelInAir;
    [Space(5)]
    public bool doConserveMomentum = true;

    
    
    [Header("Jump")]
    [Tooltip("Height of the player's jump")]
    public float jumpHeight;

    [Tooltip("Time between applying the jump force and reaching the desired jump height. These values also control the player's gravity and jump force.")]
    public float jumpTimeToApex;
    
    // the actual force applied (upwards) to the player when they jump
    [HideInInspector] public float jumpForce; 

    
    
    [Header("Both Jumps")]
    [Tooltip("Multiplier to increase gravity if the player releases the jump button while still jumping.")]
    public float jumpCutGravityMultiplier;
    
    [Tooltip("Reduces gravity while close to the apex of the jump.")]
    [Range(0f, 1)] public float jumpHangGravityMultiplier;
    
    [Tooltip("Speeds (close to 0) where the player will experience extra \"jump hang\". The player's linearVelocityY is closest to 0 at the jump's apex.")]
    public float jumpHangTimeThreshold;    
    [Space(0.5f)]
    public float jumpHangAccelerationMultiplier;
    public float jumpHangMaxSpeedMultiplier;
    
    
    
    [Header("Wall Jump")]
    [Tooltip("The actual force applied to the player when wall jumping.")]
    
    public Vector2 wallJumpForce; 
    [Space(5)]
    [Tooltip("Reduces the effect of player's movement while wall jumping.")]
    [Range(0f, 1f)] public float wallJumpRunLerp;

    [Tooltip("Time after wall jumping the player's movement is slowed for.")]
    [Range(0f, 1.5f)] public float wallJumpTime;

    [Tooltip("Player will rotate to face wall jumping direction")]
    public bool doTurnOnWallJump;

    [Space(20)]


    [Header("Slide")]
    public float slideSpeed;
    public float slideAccel;

    [Header("Assists")]
    [Tooltip("Grace period after falling off a platform, where you can still jump")]
    [Range(0.01f, 0.5f)] public float coyoteTime;

    [Tooltip("Grace period after pressing jump where a jump will be automatically performed once the requirements (eg. being grounded) are met.")]
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime;

    [Space(20)]

    [Header("Dash")]
    public int dashAmount;
    public float dashSpeed;

    [Tooltip("Duration for which the game freezes when we press dash but before we read directional input and apply a force.")]
    public float dashSleepTime;
    [Space(5)]
    public float dashAttackTime;

    [Tooltip("Dash jump grace period timer")]
    public float dashJumpBufferDuration;
    [Space(5)]

    [Tooltip("Time after you finish the inital drag phase, smoothing the transition back to idle (or any standard state).")]
    public float dashEndTime;

    [Tooltip("Slows down player, makes dash feel more responsive (used in Celeste).")]
    public Vector2 dashEndSpeed;

    [Tooltip("Slows the effect of player movement while dashing.")]
    [Range(0f, 1f)] public float dashEndRunLerp;
    
    [Space(5)]    
    public float dashRefillTime;

    [Space(5)]

    [Range(0.01f, 0.5f)] public float dashInputBufferTime;


    /// <summary>
    /// Unity Callback, called when the inspector updates
    /// </summary>
    private void OnValidate()
    {
        // calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        // calculate the rigidbody's gravity scale (ie: gravity strength relative to unity's gravity value, see project settings/Physics2D)
        gravityScale = gravityStrength / Physics2D.gravity.y;

        // calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDeceleration) / runMaxSpeed;

        // calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        #region Variable Ranges
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDeceleration = Mathf.Clamp(runDeceleration, 0.01f, runMaxSpeed);
        #endregion
    }








}
