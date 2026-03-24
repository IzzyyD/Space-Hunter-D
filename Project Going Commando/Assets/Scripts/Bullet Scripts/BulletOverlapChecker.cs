using UnityEngine;

public class BulletOverlapChecker : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    
    public bool canFire = true;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        { canFire = false; }
        
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        { canFire = true; }

    }
}
