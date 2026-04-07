using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using TMPro;


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

    [SerializeField] private NewPlayerInput playerInput;

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
    [SerializeField] private LayerMask wallsLayer;
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
        moveInput = playerInput.moveInput;

        if (moveInput.x != 0)
            CheckDirectionToFace(moveInput.x > 0);

        if (playerInput.jumpPressed)
            { OnJumpInput(); }

        if (!playerInput.jumpPressed)
            { OnJumpUpInput(); }

        if (playerInput.dashPressed)
            { OnDashInput(); }
        #endregion

        #region Collision Checks
        if (!isJumping && !isDashing)
        {
            // ground check
            if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer) && !isJumping) // checks if set box overlaps with ground
            { lastOnGroundTime = Data.coyoteTime; } // if so, sets the lastGrounded to coyoteTime

            // right wall check
            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, wallsLayer) && isFacingRight)
            || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, wallsLayer) && !isFacingRight)) && !isWallJumping)
                lastOnWallRightTime = Data.coyoteTime;

            // left wall check
            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, wallsLayer) && !isFacingRight)
            || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, wallsLayer) && isFacingRight)) && !isWallJumping)
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

        if (!isDashing)
        {
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
        }
        #endregion

        #region Dash Checks
        if (CanDash() && lastPressedDashTime > 0)
        {
            // freeze game for a split second. adds juice and a bit of forgiveness over directional input
            Sleep(Data.dashSleepTime);

            // if no direction pressed, dash forward
            if (moveInput != Vector2.zero)
                lastDashDir = moveInput;
            else
                lastDashDir = isFacingRight ? Vector2.right : Vector2.left;

            isDashing = true;
            isJumping = false;
            isWallJumping = false;
            isJumpCut = false;

            StartCoroutine(nameof(StartDash), lastDashDir);
        }
        #endregion

        #region Slide Checks
        if (CanSlide() && ((lastOnWallLeftTime > 0 && moveInput.x < 0) || (lastOnWallRightTime > 0 && moveInput.x > 0)))
            isSliding = true;
        else
            isSliding = false;
        #endregion

        #region Gravity
        if (!isDashAttacking)
        {
            // higher gravity if we've released the jump input or are falling
            if (isSliding)
            { SetGravityScale(0); }         

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
                //Default gravity if standing on a platform or moving upwards
                SetGravityScale(Data.gravityScale);
            }
        }

        else
        {
            //No gravity when dashing (returns to normal once initial dashAttack phase over)
            SetGravityScale(0);
        }
        #endregion

    }

    private void FixedUpdate()
    {
        // handle Run
        if (!isDashing)
        {
            if (isWallJumping)
                Run(Data.wallJumpRunLerp);
            else
                Run(1);
        }

        else if (isDashAttacking)
        { Run(Data.dashEndRunLerp); }

        // handle Slide
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

    public void OnDashInput()
    { lastPressedDashTime = Data.dashInputBufferTime; }
    #endregion

    #region General Methods
    public void SetGravityScale(float scale)
    { rb.gravityScale = scale; }
    private void Sleep(float duration)
    {
        ///<summary>
        /// Method used so we don't need to call StartCoroutine everywhere
        /// nameof() notation means we don't need to input a string directly.       
        ///</summary>
        StartCoroutine(nameof(PerformSleep), duration);
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;

        // must be Realtime since timeScale will be 0 
        yield return new WaitForSecondsRealtime(duration); 
        Time.timeScale = 1;
    }
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
        // increases our acceleration and maxSpeed when at the apex of our jump, makes the jump feel a bit more bouncy, responsive and natural
        if ((isJumping || isWallJumping || isJumpFalling) && Mathf.Abs(rb.linearVelocityY) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMultiplier;
            targetSpeed *= Data.jumpHangMaxSpeedMultiplier;
        }
        #endregion

        #region Conserve Momentum
        // we won't slow the player down if they're moving in their desired direction but at a greater speed than their maxSpeed
        if (Data.doConserveMomentum && Mathf.Abs(rb.linearVelocityX) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.linearVelocityX) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && lastOnGroundTime < 0)
        {
            // conserves our current momentum by preventing any deceleration from happening
            accelRate = 0;
        }
        #endregion

        // calculate difference between current velocity and desired velocity
        float speedDif = targetSpeed - rb.linearVelocityX;

        // calculate force along x-axis to apply to the player
        float movement = speedDif * accelRate;

        // convert this to a vector and apply to rigidbody
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        rb.linearVelocity = new Vector2(rb.linearVelocityX + (Time.fixedDeltaTime * speedDif * accelRate) / rb.mass, rb.linearVelocityY);
    }

    private void Turn()
    {
        // stores scale and flips the player along the x-axis
        Vector2 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        isFacingRight = !isFacingRight;
    }
    #endregion

    #region Jump Methods
    private void Jump()
    {
        // ensures we can't call Jump multiple times from one press
        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;

        #region Perform Jump
        // sets the player's Y velocity to zero
        rb.linearVelocity = new Vector2(rb.linearVelocityY, 0f);

        rb.AddForce(Vector2.up * Data.jumpForce, ForceMode2D.Impulse);
        #endregion
    }

    private void WallJump(int dir)
    {
        // ensures we can't call Wall Jump multiple times from one press
        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;
        lastOnWallLeftTime = 0;
        lastOnWallRightTime = 0;

        // ensures we flip the player when they wall jump
        if (Data.doTurnOnWallJump)
        { CheckDirectionToFace(dir > 0); }

        #region Perform Wall Jump
        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);

        // apply force in opposite direction of wall
        force.x *= dir;
        
        if (Mathf.Sign(rb.linearVelocityX) != Mathf.Sign(force.x))
            force.x -= rb.linearVelocityX;

        // checks whether player is falling, if so we subtract the linearVelocityY. ensures the player always reaches our desired jump force or greater
        if (rb.linearVelocityY < 0)
            force.y -= rb.linearVelocityY;

        rb.AddForce(force, ForceMode2D.Impulse);
        #endregion
    }
    #endregion

    #region Dash Methods
    // dash coroutine
    private IEnumerator StartDash(Vector2 dir)
    {
        // this method of dashing aims to mimic Celeste
        lastOnGroundTime = 0;
        lastPressedDashTime = 0;

        float startTime = Time.time;

        dashesLeft--;
        isDashAttacking = true;

        SetGravityScale(0);

        // we keep the player's velocity at the dash speed during the attack phase
        while (Time.time - startTime <= Data.dashAttackTime)
        {
            rb.linearVelocity = dir.normalized * Data.dashSpeed;
            
            // pauses the loop until the next frame, creating something of an Update loop
            yield return null;
        }

        startTime = Time.time;

        isDashAttacking = false;

        // begins the end of our dash where we return some control to the player but still limit run acceleration
        SetGravityScale(Data.gravityScale);
        rb.linearVelocity = Data.dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= Data.dashEndTime)
        { yield return null; }

        // dash over
        isDashing = false;        
    }

    // short period before the player is able to dash again
    private IEnumerator RefillDash(int amount)
    {
        // short cooldown, so we can't constantly dash along the ground
        dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        dashRefilling = false;
        dashesLeft = Mathf.Min(Data.dashAmount, dashesLeft + 1);
    }
    #endregion

    #region Other Movement Methods
    private void Slide()
    {        
        // works the same as the Run but only in the y-axis
        float speedDif = Data.slideSpeed - rb.linearVelocityY;
        float movement = speedDif * Data.slideAccel;

        // we clamp the movement here to prevent any over-corrections.
        // the force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        rb.AddForce(movement * Vector2.up);
    }
    #endregion

    #region Check Methods
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != isFacingRight)
            Turn();
    }

    private bool CanJump()
    { return lastOnGroundTime > 0 && !isJumping; }

    private bool CanWallJump()
    {
        return lastPressedJumpTime > 0 && lastOnWallTime > 0 && lastOnGroundTime <= 0 && (!isWallJumping || 
            (lastOnWallRightTime > 0 && lastWallJumpDir == 1) || (lastOnWallLeftTime > 0 && lastWallJumpDir == -1));
    }

    private bool CanJumpCut()
    { return isJumping && rb.linearVelocityY > 0; }

    private bool CanWallJumpCut()
    { return isWallJumping && rb.linearVelocityY > 0; }

    private bool CanDash()
    {
        if (!isDashing && dashesLeft < Data.dashAmount && lastOnGroundTime > 0 && !dashRefilling)
        { StartCoroutine(nameof(RefillDash), 1);  }

        return dashesLeft > 0;
    }

    public bool CanSlide()
    {
        if (lastOnWallTime > 0 && !isJumping && !isWallJumping && !isDashing && lastOnGroundTime <= 0)
            return true;
        else
            return false;
    }
    #endregion

    #region Editor Methods
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(frontWallCheckPoint.position, wallCheckSize);
        Gizmos.DrawWireCube(backWallCheckPoint.position, wallCheckSize);
    }
    #endregion
}
