using UnityEngine;

public class PlatformPlayerDetector : MonoBehaviour
{
    private KinematicPlatform2D platform;
    private BoxCollider2D boxCollider;


    private void Awake()
    {
        platform = GetComponentInParent<KinematicPlatform2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        { platform.SetMovementBlocked(true); }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        { platform.SetMovementBlocked(false); }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();

        if (box != null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
    }
}
