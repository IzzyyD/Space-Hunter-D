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

    [Header("Starting Camera Section")]

    [SerializeField] private bool cameraStartsOnLeft = true;

    private bool cameraIsOnLeft;

    [Header("Camera Controller")]
    [SerializeField] private CamController cameraController;

    private void Start()
    { cameraIsOnLeft = cameraStartsOnLeft; }
   

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

        // determines which side of the transition the player came from
        Vector2 playerToTransition = collision.transform.position - transform.position;

        bool playerIsOnLeft;

        // horizontal transition
        if (direction == TransitionDirection.Left || direction == TransitionDirection.Right)
        { playerIsOnLeft = playerToTransition.x < 0; }
        else
        { playerIsOnLeft = playerToTransition.y < 0; }


        // if the player entered from the side camera is already on then it won't scroll again
        if (playerIsOnLeft == cameraIsOnLeft)
            return;

        Vector2 transitionDirection;

        if (direction == TransitionDirection.Left || direction == TransitionDirection.Right)
        { transitionDirection = playerIsOnLeft ? Vector2.left : Vector2.right;}
        else
        { transitionDirection = playerIsOnLeft ? Vector2.down : Vector2.up; }

        cameraController.StartTransition(transitionDirection, distance, scrollSpeed);

        // camera has now moved to the player's side
        cameraIsOnLeft = playerIsOnLeft;
        
    }

}
