using UnityEngine;
using System.Collections;
using Unity.VisualScripting;


public class NewPlayerMovement : MonoBehaviour
{
    public PlayerData Data;


    #region Components
    public Rigidbody2D rb {  get; private set; }
    #endregion

    #region State Parameters

    /// <summary>
    /// Variables that control the various actions the player can perform at any time.
    /// Other scripts can read these fields but they can only be privately written to.
    /// </summary>
    public bool isFacingRight {  get; private set; }
    public bool isJumping {  get; private set; }
    public bool isWallJumping { get; private set; }
    public bool isDashing { get; private set; }
    public bool isSliding { get; private set; }

    // timers
    public float lastOnGroundTime { get; private set; }
    public float lastOnWallTime { get; private set; }
    public float lastOnWallRightTime { get; private set; }
    public float lastOnWallLeftTime { get; private set; }

    // jump
    private bool isJumpCut;
    private bool isJumpFalling;

    // wall jump
    private float wallJumpStartTime;
    private int lastWallJumpDir;

    // dash
    private int dashesLeft;
    private bool dashRefilling;
    private Vector2 lastDashDir;
    private bool isDashAttacking;

    #endregion

    #region Input Parameters
    private Vector2 moveInput;
    public float lastPressedJumpTime { get; private set; }
    public float lastPressedDashTime { get; private set; }
    #endregion

    #region Check Parameters
    [Header("Checks")]
    [SerializeField] private Transform groundCheckPoint;

    [Tooltip("Size of groundCheck should be slightly smaller than the width of the character (for ground) and height (for the wall check).")]
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform frontWallCheckPoint;
    [SerializeField] private Transform backWallCheckPoint;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region Layers & Tags
    [Header("Layers & Tags")]
    [SerializeField] private LayerMask groundLayer;
    #endregion

    private void Awake()
    { rb = GetComponent<Rigidbody2D>(); }

    private void Start()
    {
        SetGravityScale(Data.gravityScale);
        isFacingRight = true;
    }

    private void Update()
    {
        #region Timers
        lastOnGroundTime -= Time.deltaTime;
        lastOnWallTime -= Time.deltaTime;
        lastOnWallRightTime -= Time.deltaTime;
        lastOnWallLeftTime -= Time.deltaTime;

        lastPressedJumpTime -= Time.deltaTime;
        lastPressedDashTime -= Time.deltaTime;
        #endregion

        #region Input Handler
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.x != 0)
            CheckDirectionToFace(moveInput.x > 0);

        if (Input.GetKeyDown(KeyCode.Space))
            { OnJumpInput(); }

        if (Input.GetKeyUp(KeyCode.Space))
            { OnJumpUpInput(); }

        if (Input.GetKeyDown(KeyCode.X))
            { OnDashInput(); }
        #endregion


        #region Collision Checks
        if (!isJumping)
        {
            // ground check
            if (Physics2D.OverlapBox(groundCheckPoint.position, wallCheckSize, 0, groundLayer) && !isJumping) // checks if set box overlaps with ground
            { lastOnGroundTime = Data.coyoteTime; } // if so, sets the lastGrounded to coyoteTime

            // right wall check
            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !isFacingRight)
            || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !isFacingRight)) && !isWallJumping)
                lastOnWallRightTime = Data.coyoteTime;

            // left wall check
            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !isFacingRight)
            || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) && isFacingRight)) && !isWallJumping)
                lastOnWallLeftTime = Data.coyoteTime;

            // two checks needed for both left and right walls since whenever the player turns the wall checkPoints swap sides
            lastOnWallTime = Mathf.Max(lastOnWallLeftTime, lastOnWallRightTime);
        }
        #endregion

        #region Jump Checks
        if (isJumping && rb.linearVelocityY < 0)
        {
            isJumping = false;

            if(!isWallJumping)
                isJumpFalling = true;
        }

        if (isWallJumping && Time.time - wallJumpStartTime > Data.wallJumpTime)
        { isWallJumping = false; }

        if (lastOnGroundTime > 0 && !isJumping && !isWallJumping)
        {
            isJumpCut = false;

            if (!isJumping)
                isJumpFalling = false;            
        }

        // jump
        if (CanJump() && lastPressedJumpTime > 0)
        {
            isJumping = true;
            isWallJumping = false;
            isJumpCut = false;
            isJumpFalling = false;
            Jump();
        }
        // wall jump
        else if (CanWallJump() && lastPressedJumpTime > 0)
        {
            isWallJumping = true;
            isJumping = false;
            isJumpCut = false;
            isJumpFalling = false;
            wallJumpStartTime = Time.time;
            lastWallJumpDir = (lastOnWallRightTime > 0) ? -1 : 1;

            WallJump(lastWallJumpDir);
        }
        #endregion

        #region Slide Checks
        if (CanSlide() && ((lastOnWallLeftTime > 0 && moveInput.x < 0) || (lastOnWallRightTime > 0 && moveInput.x > 0)))
            isSliding = true;
        else
            isSliding = false;
        #endregion

        #region Gravity
        // higher gravity if we've released the jump input or are falling
        if (isSliding)
        { SetGravityScale(0); }
        
        else if (rb.linearVelocityY < 0 && moveInput.y < 0)
        {
            // much higher gravity if holding down
            SetGravityScale(Data.gravityScale * Data.fastFallGravityMultiplier);
            // caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(rb.linearVelocityY, -Data.maxFastFallSpeed));
        }

        else if (isJumpCut)
        {
            // higher gravity if jump button released
            SetGravityScale(Data.gravityScale * Data.jumpCutGravityMultiplier);
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(rb.linearVelocityY, -Data.maxFallSpeed));
        }

        else if ((isJumping || isWallJumping || isJumpFalling) && Mathf.Abs(rb.linearVelocityY) < Data.jumpHangTimeThreshold)
        { SetGravityScale(Data.gravityScale * Data.jumpHangGravityMultiplier); }

        else if (rb.linearVelocityY < 0)
        {
            // higher gravity if falling
            SetGravityScale(Data.gravityScale * Data.fallGravityMultiplier);
            // caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(rb.linearVelocityY, -Data.maxFallSpeed));
        }

        else
        {
            // default gravity if standing on a platform or moving upwards
            SetGravityScale(Data.gravityScale);
        }
        #endregion

    }

    private void FixedUpdate()
    {
        // handle run
        if (isWallJumping)
            Run(Data.wallJumpRunLerp);
        else
            Run(1);

        // handle slide
        if (isSliding)
            Slide();
    }

    #region Input Callbacks
    // methods which handle inputs detected in Update
    public void OnJumpInput()
    { lastPressedJumpTime = Data.jumpInputBufferTime; }

    public void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            isJumpCut = true;
    }
    #endregion

    #region General Methods
    public void SetGravityScale(float scale)
    { rb.gravityScale = scale; }
    #endregion

    // movement methods
    #region Run Methods
    private void Run(float lerpAmount)
    {
        // calculate the direction we want to move in and our desired velocity
        float targetSpeed = moveInput.x * Data.runMaxSpeed;
        // we can reduce our control using Lerp() this smooths changes to are direction and speed
        targetSpeed = Mathf.Lerp(rb.linearVelocityX, targetSpeed, lerpAmount);

        #region Calculate AccelRate
        float accelRate;

        /// <summary>
        /// gets an acceleration value based on if we're accelerating (includes turning)
        /// or trying to decelerate
        /// </summary>
        if (lastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
        #endregion

        #region Add Bonus Jump Apex Acceleration
        // increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
        if ((isJumping || isWallJumping || isJumpFalling) && Mathf.Abs(rb.linearVelocityY) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMultiplier;
            targetSpeed *= Data.jumpHangMaxSpeedMultiplier;
        }
        #endregion




    }
}
