using Unity.VisualScripting;
using UnityEngine;

public class CamTransition : MonoBehaviour
{
    public enum TransitionDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    [Header("Transition Settings")]
    [SerializeField] private TransitionDirection direction;
    [SerializeField] private float distance = 10f;
    [SerializeField] private float scrollSpeed = 20f;

    [Header("Camera Controller")]
    [SerializeField] private CamController cameraController;
    
    private Vector2 GetDirection()
    {
        switch (direction)
        {
            case TransitionDirection.Left:
                return Vector2.left;
            
            case TransitionDirection.Right:
                return Vector2.right;

            case TransitionDirection.Up:
                return Vector2.up;
            
            case TransitionDirection.Down:
                return Vector2.down;

            default:
                return Vector2.zero;
        
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Something entered the transition: " + collision.name);

        if (!collision.CompareTag("Player"))
            return;

        Debug.Log("PLAYER ENTERED TRANSITION!");

        Vector2 transitionDirection = GetDirection();

        cameraController.StartTransition(
            transitionDirection,
            distance,
            scrollSpeed
        );
    }

}
