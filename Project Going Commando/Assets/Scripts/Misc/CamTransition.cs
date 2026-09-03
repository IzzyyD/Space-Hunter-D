using Unity.VisualScripting;
using UnityEngine;

public class CamTransition : MonoBehaviour
{
    public enum TransitionAxis
    {
        Horizontal,
        Vertical,
    }
    

    [Header("Transition Settings")]   
    [SerializeField] private float scrollSpeed;
    [SerializeField] private TransitionAxis axis;
    [SerializeField] private CamRoom targetRoom;
    

    [Header("Camera Controller")]
    [SerializeField] private CamController cameraController;
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        // don't start another transition while one is already happening
        if (cameraController.IsTransitioning)
            return;

        // if player is already in this room, don't transition again
        if (cameraController.CurrentRoom == targetRoom)
            return;

        cameraController.StartTransition(targetRoom, axis, scrollSpeed);       
       

       
        
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D boxCollider = GetComponentInChildren<BoxCollider2D>();

        if (boxCollider == null)
            return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);

        Gizmos.DrawCube(transform.TransformPoint(boxCollider.offset), new Vector3(boxCollider.size.x * transform.lossyScale.x, boxCollider.size.y * transform.lossyScale.y, 0f));
    }

}
