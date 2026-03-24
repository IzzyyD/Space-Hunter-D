using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Movement : MonoBehaviour
{
    public InputSystem_Actions actions;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;  
    [SerializeField] private ContactFilter2D contactFilter;

  
    private float move;    
    private Rigidbody2D rb;
    private bool ShouldJump;
    public bool IsGrounded => rb.IsTouching(contactFilter);    

     void Awake()
    { actions = new InputSystem_Actions(); }    

    void OnEnable()
    {
        actions.Player.Enable();
        actions.Player.Move.performed += Movement;
        actions.Player.Jump.performed += Jumping;

        actions.Player.Move.canceled += Movement;
        actions.Player.Jump.canceled += Jumping;
    }

    private void OnDisable()
    {
        actions.Player.Disable();
        actions.Player.Move.performed -= Movement;
        actions.Player.Jump.performed -= Jumping;
    }

    void Movement(InputAction.CallbackContext ctx)
    { move = ctx.ReadValue<Vector2>().x; }

    void Jumping(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        { ShouldJump = true; }     
              
    }

    void Start()
    { rb = GetComponent<Rigidbody2D>(); }    

    void FixedUpdate()
    {
        rb.linearVelocityX = move * moveSpeed;
       
        if (ShouldJump && IsGrounded)
        { rb.linearVelocityY = jumpForce; }

        ShouldJump = false;
    }

   



}
