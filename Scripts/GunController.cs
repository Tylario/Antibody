using System.Collections;
using UnityEngine;
using TMPro;

public class GunController : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public GameObject weaponObject; // Weapon GameObject to enable/disable
        public int bulletsPerMag; // Bullets per magazine
        public int bulletsPerShot; // Bullets fired per shot
        public float bulletAccuracy; // Spread angle for bullet in degrees
        public float reloadTime; // Time to reload
        public float timeBetweenShots; // Time between each shot
        public bool isAutomatic; // If true, gun will fire automatically when holding down the button
        public Transform bulletSpawnPoint; // Where bullets spawn
        public GameObject bulletPrefab; // Bullet prefab
    }

    public Weapon[] weapons; // Array of all weapons
    private int currentWeaponIndex = -1; // -1 means no weapon selected
    private Weapon currentWeapon; // Current weapon reference
    private bool isReloading = false; // Is the gun reloading
    private bool canShoot = true; // Can the gun shoot
    private int bulletsInMag; // Bullets left in the magazine

    public Vector3 positionA; // Default position for the gun manager
    public Vector3 positionB; // Aim down sights position for the gun manager
    public float aimSpeed = 10f; // Speed of aiming transition
    public TextMeshProUGUI ammoDisplay; // TextMeshPro object for ammo display

    [Header("Recoil Settings")]
    public float recoilDuration = 0.1f; // Duration of the recoil effect
    public float recoilPositionAmount = 0.02f; // Amount of position recoil
    public float recoilRotationAmount = 1f; // Amount of rotation recoil in degrees

    private Coroutine shootingCoroutine; // Coroutine for continuous shooting when holding down the button
    private Vector3 originalPosition; // Original local position of the gun
    private Quaternion originalRotation; // Original local rotation of the gun

    private void Start()
    {
        SelectWeapon(0); // Start with the first weapon if desired
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        transform.localPosition = positionA; // Set starting position for the gun manager
        UpdateAmmoDisplay(); // Initialize ammo display
    }

    private void Update()
    {
        HandleWeaponSwitching();

        if (isReloading) return;

        HandleAiming();

        if (currentWeapon != null)
        {
            if (currentWeapon.isAutomatic)
            {
                if (Input.GetMouseButtonDown(0) && canShoot)
                {
                    shootingCoroutine = StartCoroutine(ContinuousShooting());
                }
                if (Input.GetMouseButtonUp(0) && shootingCoroutine != null)
                {
                    StopCoroutine(shootingCoroutine);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && canShoot) Shoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && bulletsInMag < currentWeapon.bulletsPerMag) StartCoroutine(Reload());
    }

    void HandleWeaponSwitching()
    {
        int previousWeaponIndex = currentWeaponIndex;

        if (Input.GetKeyDown(KeyCode.Alpha1)) currentWeaponIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) && weapons.Length > 1) currentWeaponIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3) && weapons.Length > 2) currentWeaponIndex = 2;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
        if (scroll < 0f) currentWeaponIndex = (currentWeaponIndex - 1 + weapons.Length) % weapons.Length;

        if (previousWeaponIndex != currentWeaponIndex) SelectWeapon(currentWeaponIndex);
    }

    void SelectWeapon(int index)
    {
        foreach (Weapon weapon in weapons)
        {
            weapon.weaponObject.SetActive(false);
        }

        if (index >= 0 && index < weapons.Length)
        {
            weapons[index].weaponObject.SetActive(true);
            currentWeapon = weapons[index];
            bulletsInMag = currentWeapon.bulletsPerMag;
            UpdateAmmoDisplay();
        }
        else
        {
            currentWeapon = null;
            ammoDisplay.text = ""; // Clear ammo display
        }
    }

    void HandleAiming()
    {
        // Set the target position based on whether the player is holding right-click to aim
        Vector3 targetPosition = Input.GetMouseButton(1) ? positionB : positionA;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * aimSpeed);
    }

    void Shoot()
    {
        if (bulletsInMag <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        canShoot = false;

        // Fire the bullets with spread
        for (int i = 0; i < currentWeapon.bulletsPerShot; i++)
        {
            Vector3 shootDirection = currentWeapon.bulletSpawnPoint.forward;

            // Apply random spread on both axes
            float randomSpreadX = Random.Range(-currentWeapon.bulletAccuracy, currentWeapon.bulletAccuracy);
            float randomSpreadY = Random.Range(-currentWeapon.bulletAccuracy, currentWeapon.bulletAccuracy);

            shootDirection = Quaternion.Euler(randomSpreadX, randomSpreadY, 0) * shootDirection;
            Instantiate(currentWeapon.bulletPrefab, currentWeapon.bulletSpawnPoint.position, Quaternion.LookRotation(shootDirection));
        }

        Debug.Log("Shoot");
        bulletsInMag--; // Decrease bullets count
        UpdateAmmoDisplay();
        Invoke(nameof(ResetShoot), currentWeapon.timeBetweenShots);

        StartCoroutine(ApplyRecoil()); // Apply recoil effect
    }

    IEnumerator ApplyRecoil()
    {
        // Determine the current target position based on aiming state
        Vector3 currentTargetPosition = Input.GetMouseButton(1) ? positionB : positionA;

        // Calculate random recoil offset relative to the current target position
        Vector3 recoilPosition = currentTargetPosition + new Vector3(
            Random.Range(-recoilPositionAmount, recoilPositionAmount),
            Random.Range(-recoilPositionAmount, recoilPositionAmount),
            0
        );

        // Define a consistent backward rotation for the recoil effect
        Quaternion recoilRotation = originalRotation * Quaternion.Euler(
            -recoilRotationAmount, // Consistent backward rotation on the X-axis
            0,
            0
        );

        // Move to recoil position and rotation
        float elapsed = 0f;
        while (elapsed < recoilDuration)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, recoilPosition, elapsed / recoilDuration);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, recoilRotation, elapsed / recoilDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return smoothly to the current target position and original rotation
        elapsed = 0f;
        while (elapsed < recoilDuration)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, currentTargetPosition, elapsed / recoilDuration);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, elapsed / recoilDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure it's exactly at the current target position and original rotation
        transform.localPosition = currentTargetPosition;
        transform.localRotation = originalRotation;
    }



    void ResetShoot()
    {
        canShoot = true;
    }

   

    IEnumerator ContinuousShooting()
    {
        while (true)
        {
            if (canShoot)
            {
                Shoot();
            }
            yield return new WaitForSeconds(currentWeapon.timeBetweenShots);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        canShoot = false;

        Vector3 loweredPosition = originalPosition + Vector3.down * 0.5f; // Lower the gun manager for reload
        float lowerSpeed = 5f;

        while (Vector3.Distance(transform.localPosition, loweredPosition) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, loweredPosition, Time.deltaTime * lowerSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        bulletsInMag = currentWeapon.bulletsPerMag;
        UpdateAmmoDisplay();

        while (Vector3.Distance(transform.localPosition, originalPosition) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * lowerSpeed);
            yield return null;
        }

        isReloading = false;
        canShoot = true;
    }

    void UpdateAmmoDisplay()
    {
        if (currentWeapon != null)
        {
            ammoDisplay.text = $"{bulletsInMag}/{currentWeapon.bulletsPerMag}";
        }
        else
        {
            ammoDisplay.text = ""; // Clear ammo display if no weapon is selected
        }
    }
}
