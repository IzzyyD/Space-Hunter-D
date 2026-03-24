using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Explosion Properties")]
    [SerializeField] private float lifeTime;

    void Start()
    { Destroy(gameObject, lifeTime); }

}
