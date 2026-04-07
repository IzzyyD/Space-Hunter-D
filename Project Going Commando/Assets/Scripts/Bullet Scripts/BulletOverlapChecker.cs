using UnityEngine;

public class BulletOverlapChecker : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallsLayer;
    
    public bool canFire = true;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((groundLayer | wallsLayer) & (1 << collision.gameObject.layer)) != 0)
        { canFire = false; }
        
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (((groundLayer | wallsLayer) & (1 << collision.gameObject.layer)) != 0)
        { canFire = true; }

    }
}
