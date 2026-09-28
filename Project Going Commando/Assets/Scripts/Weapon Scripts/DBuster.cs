using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/DBuster")]
public class DBuster : WeaponData
{
    [Header("Charge Shot")]
    public GameObject chargedBulletPrefab;

    public override void HandleFire(Transform spawnPoint, WeaponController owner)
    {
        GameObject bulletObj = Instantiate(bulletPrefab , spawnPoint.position, spawnPoint.rotation);

        var bullet = bulletObj.GetComponent<BusterBullet>();

        if (bullet != null )
        { bullet.Initialize(range); }
        
    }

    public override void HandleFire(Transform spawnPoint, WeaponController owner, float charge)
    {
        GameObject bulletObj = Instantiate(chargedBulletPrefab, spawnPoint.position, spawnPoint.rotation);

        var bullet = bulletObj.GetComponent<ChargeBusterBullet>();

        if (bullet != null)
        {  bullet.Initialize(range); }
    }
    
    
    
    
}
