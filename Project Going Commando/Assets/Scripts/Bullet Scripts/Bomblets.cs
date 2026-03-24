using UnityEngine;

public class Bomblets : MonoBehaviour
{
    [Header("Bomblet Properties")]
    [SerializeField] private float fuseTime;
    [SerializeField] private GameObject explosion;

    private float timer;


    void Start()
    { timer = fuseTime; }


    void Update()
    {
         timer -= Time.deltaTime;

        if (timer <= 0f)
        { Explosion(); }
        
    }

    void Explosion()
    {
        if (explosion != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);

            Destroy(this.gameObject);
        }
    }
}
