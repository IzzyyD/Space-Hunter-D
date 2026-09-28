using UnityEngine;

public class LancerBullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float destroyTime;
    [SerializeField] private LayerMask groundLayer;    
 

    [Header("Ricochet Properties")]
    [SerializeField] private int maxRicochets;


    private float maxDistance;
    private int currentRicochets;
    private Vector2 direction;
    private Vector2 startPos;
    private Rigidbody2D rb;  


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        direction = transform.right;

        rb.linearVelocity = direction * bulletSpeed;

        Destroy(gameObject, destroyTime);

    }

    void Update()
    {
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        { Destroy(gameObject); }

    }

    public void Initialize(float range)
    {
        maxDistance = range;
        startPos = transform.position;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((groundLayer) & (1 << collision.gameObject.layer)) != 0)
        {
            Vector2 bulletPos = transform.position;
            Vector2 hitPoint = collision.ClosestPoint(bulletPos);
            Vector2 normal = (bulletPos - hitPoint).normalized;          

            Ricochet(normal);
        }
    }

    void Ricochet(Vector2 normal)
    {
        currentRicochets++;

        if (currentRicochets > maxRicochets)
        {
            Destroy(gameObject);
            return;
        }

        direction = Vector2.Reflect(direction, normal).normalized;

        rb.linearVelocity = direction * bulletSpeed;

        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

    }

}


