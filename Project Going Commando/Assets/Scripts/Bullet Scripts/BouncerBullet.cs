using UnityEngine;
using System.Collections;

public class BouncerBullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletGravity;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallsLayer;
    

    [Header("Bomblet Properties")]
    [SerializeField] private GameObject bomblet; 
    [SerializeField] private GameObject explosion;
    [SerializeField] private int bombletCount;

    private float bombletDelay = 0.15f;
    private float maxDistance;
    private Vector2 startPos;
    private Rigidbody2D rb;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * bulletSpeed;
        rb.gravityScale = bulletGravity;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((groundLayer | wallsLayer) & (1 << collision.gameObject.layer)) != 0) 
        {
            
            Instantiate(explosion, transform.position, Quaternion.identity);
            
            StartCoroutine(DelayedBomblets());
          
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;

           
        }
    }


    IEnumerator DelayedBomblets()
    {
        yield return new WaitForSeconds(bombletDelay);

        SpawnBomblets();

        Destroy(gameObject);
    }


    void Update()
    {
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
            Destroy(gameObject);
    }


    public void Initialize(float range)
    {
        maxDistance = range;
        startPos = transform.position;
    }


    void SpawnBomblets()
    {
        for (int i = 0; i < bombletCount; i++)
        {
            GameObject bomb = Instantiate(bomblet, transform.position, Quaternion.identity);
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();

            Vector2 randomVel = new Vector2(Random.Range(-3f, 3f), Random.Range(2f, 5f));
            rb.linearVelocity = randomVel;
        }
    }

}