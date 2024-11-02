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

        public ParticleSystem shotEffect; // Particle effect for shooting
        public AudioClip shootSound; // Audio clip for shooting
        public AudioClip reloadSound; // Audio clip for reloading
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
    public bool gunsUnlocked = true; // Controls whether guns are visible/unlocked

    [Header("Recoil Settings")]
    public float recoilDuration = 0.1f; // Duration of the recoil effect
    public float recoilBackwardAmount = 0.02f; // Amount of backward movement recoil
    public float recoilUpwardRotation = 5f; // Amount of upward rotation recoil in degrees

    private Coroutine shootingCoroutine; // Coroutine for continuous shooting when holding down the button
    private Vector3 originalPosition; // Original local position of the gun
    private Quaternion originalRotation; // Original local rotation of the gun
    private AudioSource audioSource; // Audio source component

    private void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        audioSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource component
        SelectWeapon(0); // Start with the first weapon if desired
        UpdateAmmoDisplay(); // Initialize ammo display
    }

    private void Update()
    {
        if (!gunsUnlocked)
        {
            HideAllWeapons();
            return;
        }

        HandleWeaponSwitching();

        if (isReloading) return;

        HandleAiming();

        if (currentWeapon != null)
        {
            if (currentWeapon.isAutomatic)
            {
                // For automatic guns, start and stop shooting based on whether left mouse is held
                if (Input.GetMouseButton(0) && canShoot && shootingCoroutine == null)
                {
                    shootingCoroutine = StartCoroutine(ContinuousShooting());
                }
                else if (!Input.GetMouseButton(0) && shootingCoroutine != null)
                {
                    StopCoroutine(shootingCoroutine);
                    shootingCoroutine = null;
                }
            }
            else
            {
                // For non-automatic guns, shoot only once when left mouse button is pressed
                if (Input.GetMouseButtonDown(0) && canShoot)
                {
                    Shoot();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && bulletsInMag < currentWeapon.bulletsPerMag)
        {
            StartCoroutine(Reload());
        }
    }

    void HandleWeaponSwitching()
    {
        int previousWeaponIndex = currentWeaponIndex;

        // Press 1, 2, or 3 to switch weapons
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentWeaponIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) && weapons.Length > 1) currentWeaponIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3) && weapons.Length > 2) currentWeaponIndex = 2;

        // Scroll wheel to cycle weapons
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
        if (scroll < 0f) currentWeaponIndex = (currentWeaponIndex - 1 + weapons.Length) % weapons.Length;

        if (previousWeaponIndex != currentWeaponIndex) SelectWeapon(currentWeaponIndex);
    }

    void SelectWeapon(int index)
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }

        foreach (Weapon weapon in weapons)
        {
            weapon.weaponObject.SetActive(false);
        }

        if (gunsUnlocked && index >= 0 && index < weapons.Length)
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

    void HideAllWeapons()
    {
        foreach (Weapon weapon in weapons)
        {
            weapon.weaponObject.SetActive(false);
        }
        ammoDisplay.text = ""; // Clear ammo display
    }

    void HandleAiming()
    {
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

        for (int i = 0; i < currentWeapon.bulletsPerShot; i++)
        {
            Vector3 shootDirection = currentWeapon.bulletSpawnPoint.forward;

            float randomSpreadX = Random.Range(-currentWeapon.bulletAccuracy, currentWeapon.bulletAccuracy);
            float randomSpreadY = Random.Range(-currentWeapon.bulletAccuracy, currentWeapon.bulletAccuracy);

            shootDirection = Quaternion.Euler(randomSpreadX, randomSpreadY, 0) * shootDirection;

            Instantiate(currentWeapon.bulletPrefab, currentWeapon.bulletSpawnPoint.position, Quaternion.LookRotation(shootDirection));
        }

        // Play particle effect and sound
        if (currentWeapon.shotEffect != null)
        {
            currentWeapon.shotEffect.Play();
        }
        if (currentWeapon.shootSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.shootSound);
        }

        Debug.Log("Shoot");
        bulletsInMag--; // Decrease bullets count
        UpdateAmmoDisplay();
        Invoke(nameof(ResetShoot), currentWeapon.timeBetweenShots);

        StartCoroutine(ApplyRecoil()); // Apply recoil effect
    }

    void ResetShoot()
    {
        canShoot = true;
    }

    IEnumerator ApplyRecoil()
    {
        Vector3 currentTargetPosition = Input.GetMouseButton(1) ? positionB : positionA;

        // Move the gun backward slightly and rotate it upward for recoil
        Vector3 recoilPosition = currentTargetPosition - transform.forward * recoilBackwardAmount;
        Quaternion recoilRotation = originalRotation * Quaternion.Euler(-recoilUpwardRotation, 0, 0);

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

    IEnumerator ContinuousShooting()
    {
        while (true)
        {
            if (canShoot && !isReloading) // Check that we're not reloading
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

        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }

        Vector3 loweredPosition = originalPosition + Vector3.down;
        float lowerSpeed = 5f;

        while (Vector3.Distance(transform.localPosition, loweredPosition) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, loweredPosition, Time.deltaTime * lowerSpeed);
            yield return null;
        }

        if (currentWeapon.reloadSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.reloadSound);
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
