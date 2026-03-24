using UnityEngine;

[CreateAssetMenu (menuName = "Weapons/Rapid Fire")]
public class RapidFireGun : WeaponData
{
    public override void HandleFire(Transform spawnPoint, WeaponController owner)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
       
        var bullet = bulletObj.GetComponent<LancerBullet>();
        
        if (bullet != null)
        { bullet.Initialize(range);}
            

    }
}
