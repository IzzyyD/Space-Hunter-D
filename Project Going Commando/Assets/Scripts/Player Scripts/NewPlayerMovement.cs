using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Windows;

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

    }
}
