using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEditor.Embree;

public class WeaponController : MonoBehaviour
{    
    [Header("Gun Setup")]
    [SerializeField] private Transform gunParent;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private NewPlayerInput playerInput;
    

    [Header("Charge UI")]
    [SerializeField] private TextMeshProUGUI chargeText;  


    [Header("Weapon Scroll")]
    public WeaponData[] weapons;
    public float weaponScrollSpeed;
    private int weaponIndex = 0;
    [HideInInspector] public WeaponData currentWeapon;
    [HideInInspector] public BulletOverlapChecker bulletOverlapChecker;


    #region Bunch Of Parameters
    [HideInInspector] public bool canAim = true;
    private GameObject currentGunInstance;
    private float nextFireTime = 0f; 
    private Vector2 direction;    
    private float chargeTime = 0f;
    private bool isCharging;
    private NewPlayerMovement playerMovement;
    private bool wasShootingLastFrame;
    private bool wasShootHeld;
    private bool lastFacingRight;
    private bool nextHeld;
    private bool prevHeld;
    private float nextScrollTime;
    #endregion

    private void Awake()
    { playerMovement = GetComponentInParent<NewPlayerMovement>(); }

    void Start()
    {
        if (weapons.Length > 0)
            ApplyWeapon();

        lastFacingRight = playerMovement.isFacingRight;
    }

    void Update()
    {
        WeaponScroll();
        Aiming();

        if (currentWeapon == null) return;

        if (currentWeapon.isChargeHybrid)
        {
            HybridShooting();
            UpdateChargeUI();
        }
        else if (currentWeapon.isCharge)
        {
            ChargeShooting();
            UpdateChargeUI();
        }
        else
        {
            Shooting();
            HideChargeUI();
        }
    }

    void WeaponScroll()
    {
        if (Time.time < nextScrollTime) return;

        if (playerInput.nextWeaponPressed && !nextHeld)
        {
            weaponIndex = (weaponIndex + 1) % weapons.Length;
            ApplyWeapon();
            nextScrollTime = Time.time + weaponScrollSpeed;
        }

        if (playerInput.prevWeaponPressed && !prevHeld)
        {
            weaponIndex = (weaponIndex - 1 + weapons.Length) % weapons.Length;
            ApplyWeapon();
            nextScrollTime = Time.time + weaponScrollSpeed;
        }

        nextHeld = playerInput.nextWeaponPressed;
        prevHeld = playerInput.prevWeaponPressed;
    }

    void ApplyWeapon()
    {
        chargeTime = 0f;
        isCharging = false;

        if (weapons.Length == 0) return;

        currentWeapon = weapons[weaponIndex];

        if (currentGunInstance != null)
            Destroy(currentGunInstance);

        if (currentWeapon.weaponPrefab != null)
            currentGunInstance = Instantiate(currentWeapon.weaponPrefab, gunParent.position, gunParent.rotation, gunParent);

        if (weaponNameText != null)
            weaponNameText.text = "Equipped: " + currentWeapon.weaponName;

        HideChargeUI();
    }
  
    void Shooting()
    {
        if (currentWeapon == null || Time.time < nextFireTime) return;

        bool shouldFire = currentWeapon.isSemiAuto
            ? playerInput.shootPressed && !wasShootingLastFrame
            : playerInput.shootPressed;

        if (shouldFire && bulletOverlapChecker != null && bulletOverlapChecker.canFire)
        {
            currentWeapon.HandleFire(bulletSpawnPoint, this);
            nextFireTime = Time.time + currentWeapon.cooldown;
        }

        wasShootingLastFrame = playerInput.shootPressed;
    }


    void ChargeShooting()
    {
        if (Time.time < nextFireTime) return;

        if (playerInput.shootPressed)
        {
            isCharging = true;
            chargeTime += Time.deltaTime * currentWeapon.chargeSpeed;
            chargeTime = Mathf.Clamp(chargeTime, 0f, currentWeapon.maxChargeTime);
        }

        if (isCharging && !playerInput.shootPressed && wasShootHeld)
        {
            if (bulletOverlapChecker != null && bulletOverlapChecker.canFire)
            {
                float charge = Mathf.Clamp01(chargeTime / currentWeapon.maxChargeTime);
                currentWeapon.HandleFire(bulletSpawnPoint, this, charge);
                nextFireTime = Time.time + currentWeapon.cooldown;
            }

            chargeTime = 0f;
            isCharging = false;
        }

        wasShootHeld = playerInput.shootPressed;
    }

    void HybridShooting()
    {
        if (Time.time < nextFireTime) return;

        if (playerInput.shootPressed)
        {
            isCharging = true;
            chargeTime += Time.deltaTime * currentWeapon.chargeSpeed;
            chargeTime = Mathf.Clamp(chargeTime, 0f, currentWeapon.maxChargeTime);
        }

        if (isCharging && !playerInput.shootPressed && wasShootHeld)
        {
            if (bulletOverlapChecker != null && bulletOverlapChecker.canFire)
            {
                if (chargeTime >= currentWeapon.chargeThreshold)
                {
                    float charge = Mathf.Clamp01(chargeTime / currentWeapon.maxChargeTime);
                    currentWeapon.HandleFire(bulletSpawnPoint, this, charge);
                }
                else
                { currentWeapon.HandleFire(bulletSpawnPoint, this); }

                nextFireTime = Time.time + currentWeapon.cooldown;
            }

            chargeTime = 0f;
            isCharging = false;
        }

        wasShootHeld = playerInput.shootPressed;
    }

    void UpdateChargeUI()
    {
        if (chargeText == null || currentWeapon == null) return;

        bool showCharge = currentWeapon.isCharge && isCharging;

        if (currentWeapon.isChargeHybrid)
        { showCharge = showCharge && chargeTime >= 0.2f * currentWeapon.chargeSpeed; }

        chargeText.gameObject.SetActive(showCharge);

        float charge01 = Mathf.Clamp01(chargeTime / currentWeapon.maxChargeTime);
        chargeText.text = $"CHARGING: {Mathf.RoundToInt(charge01 * 100)}%";

        if (charge01 < 1f)
        { 
            chargeText.color = Color.yellow;
            chargeText.enabled = true;
        }

        else
        { 
            chargeText.color = Color.red;
            chargeText.text = "FULL CHARGE!";
            chargeText.enabled = Mathf.PingPong(Time.time * 2f, 1f) > 0.5f;
        }
    }

    void HideChargeUI()
    {
        if (chargeText != null)
            chargeText.text = "";
    }
    
    void Aiming()
    {
        // handles bullet spawn pos when the player changes direction
        if (playerMovement.isFacingRight != lastFacingRight)
        {
            Vector3 localPos = bulletSpawnPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (playerMovement.isFacingRight ? 1 : -1);
            bulletSpawnPoint.localPosition = localPos;

            lastFacingRight = playerMovement.isFacingRight;
        }

        // makes sure d-buster always fires horizontally
        if (currentWeapon != null && currentWeapon.noAim)
        {
            direction = playerMovement.isFacingRight ? Vector2.right : Vector2.left;
            gunParent.right = direction;
            return;
        }

        // normal aiming
        Vector2 raw = playerInput.moveInput;
        
        float x = Mathf.Abs(raw.x) > 0.5f ? Mathf.Sign(raw.x) : 0f;
        float y = Mathf.Abs(raw.y) > 0.5f ? Mathf.Sign(raw.y) : 0f;

        Vector2 aim;

        if(x != 0 || y != 0)
            aim = new Vector2(x, y);
        else
            aim = playerMovement.isFacingRight ? Vector2.right : Vector2.left;

        direction = aim.normalized;
        gunParent.right = direction;

        if(playerMovement.isFacingRight != lastFacingRight)
        {
            Vector3 localPos = bulletSpawnPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (playerMovement.isFacingRight ? 1 : -1);
            bulletSpawnPoint.localPosition = localPos;

            lastFacingRight = playerMovement.isFacingRight;
        }


    }
}
