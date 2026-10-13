using UnityEngine;

[RequireComponent(typeof(Rigidbody2D),typeof(Collider2D))]
public abstract class KinematicPlatform2D : MonoBehaviour
{
    [Tooltip("Time it takes to complete 1 cycle of movement")]
    public float cycleRunTime;
    
    [Tooltip("How long to wait before starting another cycle")]
    public float cycleWaitTime;
    
    [Tooltip("For subclasses to check what the current cycle time is")]
    protected float currentCycleTime; 
    
    [Tooltip("For subclasses to check what the current wait time is ")]
    protected float currentWaitTime;

    [Tooltip("After a cycle completes, does the time start from 0 again or does it go in the opposite direction?")]
    public enum CycleType
    { repeat, pingPong }

    public CycleType cycleType;

    protected sbyte cycleDirection = 1; // only for ping pong mode
    protected Rigidbody2D rb;

    protected bool movementBlocked;

    // awake is used here so we don't have to override start in child classes
    protected virtual void Awake()
    { rb = GetComponent<Rigidbody2D>(); }

    protected virtual void Reset()
    {
        // force the rigidbody to kinematic mode when this component is added
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic; 
    }

    // protected because it needs to be overriden
    protected virtual void Update()
    {
        if (movementBlocked) return;

        if (currentWaitTime <= 0)
        {
            currentCycleTime += Time.deltaTime * cycleDirection;
            HandleCycleEnd();
        }
        else
        {
            currentWaitTime -= Time.deltaTime;
            HandleCycleRestart();
        }
    }

    public void SetMovementBlocked(bool blocked)
    { movementBlocked = blocked; }

    void HandleCycleRestart()
    {
        if (currentWaitTime < 0)
        {
            currentCycleTime -= currentWaitTime;
            currentWaitTime = 0;
        }
    }

    /// <summary>
    /// checks if the cycle has ended. if it has, set the object to wait mode.
    /// algorithm works if cycleWaitTime is 0. 
    /// </summary>
    void HandleCycleEnd()
    {
        // if the cycle is finished, set the wait time and the currentCycleTime. works if cycleWaitTime is 0
        switch(cycleType)
        {
            case CycleType.repeat:

                if (currentCycleTime > cycleRunTime)
                {
                    // we also subtract the wait time by the time we already exceeded
                    currentWaitTime = cycleWaitTime - (currentCycleTime - cycleRunTime);
                    currentCycleTime = 0; // the next cycle time to start from
                }               
         
            
                break;

            case CycleType.pingPong:

                // invert the cycle direction if we exceed the run time
                if (currentCycleTime > cycleRunTime)
                {
                    // we account for if the currentCycleTime exceeds the cycleRunTime
                    currentWaitTime = cycleWaitTime - (currentCycleTime - cycleRunTime);
                    cycleDirection = -1;

                    // set the next cycle time to start from
                    currentCycleTime = cycleRunTime;
                }
                else if (currentCycleTime < 0)
                {
                    currentWaitTime = cycleWaitTime - currentCycleTime;
                    cycleDirection = 1;

                    // set the next cycle time to start from
                    currentCycleTime = 0;
                }

                break;
              
        }
    }  



}
