using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour
{
    [Header("References")]   
    [SerializeField] private Transform cinemachineCamera;
    [SerializeField] private CinemachineConfiner2D confiner;
    [SerializeField] private CamRoom currentRoom;   

    private CinemachinePositionComposer posComposer;
    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;
    public CamRoom CurrentRoom => currentRoom;

    private void Awake()
    { posComposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>(); }

    public void SetRoom(CamRoom newRoom)
    {
        if (newRoom == null)
            return;

        currentRoom = newRoom;

        // tells cinemachine to stop using room A's bounds and use the next room bounds
        confiner.BoundingShape2D = newRoom.RoomBounds;
        confiner.InvalidateBoundingShapeCache();
    }

    public void StartTransition(CamRoom targetRoom, float scrollSpeed)
    {
        if (isTransitioning || targetRoom == null)
            return;

        StartCoroutine(MoveCamera(targetRoom, scrollSpeed));
                
    }

    private IEnumerator MoveCamera(CamRoom targetRoom, float scrollSpeed)
    {
        isTransitioning = true;

        // temporarily stop cinemachine following the player
        posComposer.enabled = false;

        // current camera position
        Vector3 startPosition = cinemachineCamera.position;

        // move towards the center of the new room
        Vector3 targetPosition = targetRoom.transform.position;

        // keep the camera's Z position
        targetPosition.z = startPosition.z;

        while (Vector3.Distance(cinemachineCamera.position, targetPosition) > 0.01f)
        {
            cinemachineCamera.position = Vector3.MoveTowards(cinemachineCamera.position, targetPosition, scrollSpeed * Time.deltaTime);
            yield return null;
        }

        cinemachineCamera.position = targetPosition;

        // switch to the new room bounds
        SetRoom(targetRoom);

        // resume following the player
        posComposer.enabled = true;

        isTransitioning = false;

    }

    

}
