using UnityEngine;

public abstract class WeaponData : ScriptableObject
{
    [Header("General Weapon Information")]
    public string weaponName;    
    public GameObject weaponPrefab;

    [Header("Bullet Type")]
    public GameObject bulletPrefab;

    [Header("General Properties")]
    public float range;
    public float cooldown;    

    [Header("Special Properties")]
    public bool isSemiAuto;
    public bool isCharge;

    [Header("Charge Properties")]
    public float maxChargeTime;
    public float chargeSpeed;

    public virtual void HandleFire(Transform spawnPoint, WeaponController owner, float charge)
    { HandleFire(spawnPoint, owner); }



    public abstract void HandleFire(Transform spawnPoint, WeaponController owner);
   

}
