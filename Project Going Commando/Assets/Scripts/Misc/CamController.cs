using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform cinemachineCamera;

    private CinemachinePositionComposer posComposer;

    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    private void Awake()
    { posComposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>(); }

    public void StartTransition(Vector2 direction, float distance, float scrollSpeed)
    {
        if (isTransitioning)
            return;

        StartCoroutine(MoveCamera(direction, distance, scrollSpeed));
                
    }

    private System.Collections.IEnumerator MoveCamera(Vector2 direction, float distance, float scrollSpeed)
    {
        isTransitioning = true;

        // temporarily stop cinemachine following the player
        posComposer.enabled = false;

        Vector3 startPosition = cinemachineCamera.position;

        Vector3 targetPosition = startPosition + new Vector3(direction.x, direction.y, 0f) * distance;

        float transitionDistance = distance;

        while (Vector3.Distance(cinemachineCamera.position, targetPosition) > 0.01f)
        {
            cinemachineCamera.position = Vector3.MoveTowards(cinemachineCamera.position, targetPosition, scrollSpeed * Time.deltaTime);
            yield return null;
        }

        cinemachineCamera.position = targetPosition;

        posComposer.enabled = true;

        isTransitioning = false;
    }

    

}
