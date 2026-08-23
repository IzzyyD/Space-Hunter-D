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
        if (!collision.CompareTag("Player"))
            return;  

        Vector2 transitionDirection = GetDirection();

        // determines which side of the transition the player came from
        Vector2 playerToTransition = collision.transform.position - transform.position;

        // reverses direction if the player is approaching from the opposite side
        if (direction == TransitionDirection.Left || direction == TransitionDirection.Right)
        {
            if (playerToTransition.x > 0)
                transitionDirection = Vector2.left;
            else
                transitionDirection = Vector2.right;
        }
        else
        {
            if (playerToTransition.y > 0)
                transitionDirection = Vector2.down;
            else
                transitionDirection = Vector2.up;
        }

        cameraController.StartTransition(transitionDirection, distance, scrollSpeed);

        Debug.Log("Something entered the transition: " + collision.name);

        if (!collision.CompareTag("Player"))
            return;

        Debug.Log("PLAYER ENTERED TRANSITION!");
    }

}
