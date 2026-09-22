using UnityEngine;

public class CamRoom : MonoBehaviour
{
    [Header("Room Bounds")]
    [SerializeField] private Collider2D roomBounds;

    public Collider2D RoomBounds => roomBounds;
}
