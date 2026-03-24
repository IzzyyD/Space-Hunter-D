using UnityEngine;

[CreateAssetMenu (menuName = "Weapons/Cluster Bomb")]
public class ClusterBomb : WeaponData
{
    public override void HandleFire(Transform spawnPoint, WeaponController owner)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);

        var bullet = bulletObj.GetComponent<BouncerBullet>();

        if (bullet != null)
        { bullet.Initialize(range); }
        
    }
}
