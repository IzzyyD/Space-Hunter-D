using UnityEngine;


public class StraightMovingPlatform2D : KinematicPlatform2D
{
    public Vector2 endPoint = new Vector2(5, 0);
    Vector2 origin;


    private void Start()
    { origin = transform.position; }


    protected override void Update()
    {
        // needed since it manages the cycle time
        base.Update();

        // calculate where the platform should be
        Vector2 targetPosition = Vector2.Lerp(origin, origin + endPoint, currentCycleTime / cycleRunTime);

        // move the rigidbody instead of transform
        rb.MovePosition(targetPosition);
    }

    protected override void Reset()
    {
        base.Reset();

        // set the endpoint to always be a straight line forward by default
        endPoint = transform.position + transform.right * 8;

        cycleType = CycleType.pingPong;
      
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 start = Application.isPlaying ? origin : (Vector2)transform.position, 
            end = start + endPoint;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.25f);
        Gizmos.DrawWireSphere(end, 0.25f);
    }
}
