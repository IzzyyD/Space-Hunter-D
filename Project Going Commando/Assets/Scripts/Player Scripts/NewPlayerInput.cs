using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerInput : MonoBehaviour
{
    private PlayerControls controls;

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool dashPressed;
    [HideInInspector] public bool shootPressed;

    private void Awake()
    {
        controls = new PlayerControls();

        // movement 
        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;

        // jump
        controls.Gameplay.Jump.performed += ctx => jumpPressed = true;
        controls.Gameplay.Jump.canceled += ctx => jumpPressed = false;

        // dash
        controls.Gameplay.Dash.performed += ctx => dashPressed = true;
        controls.Gameplay.Dash.canceled += ctx => dashPressed = false;

        // shoot
        controls.Gameplay.Shoot.performed += ctx => shootPressed = true;
        controls.Gameplay.Shoot.canceled += ctx => shootPressed = false;
    }

    private void OnEnable() => controls.Gameplay.Enable();
    private void OnDisable() => controls.Gameplay.Disable();
}
