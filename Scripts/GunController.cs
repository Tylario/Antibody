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

        public int currentBulletsInMag; // Bullets currently in the magazine
    }

    public Weapon[] weapons; // Array of all weapons
    private int currentWeaponIndex = -1; // -1 means no weapon selected
    private Weapon currentWeapon; // Current weapon reference
    private bool isReloading = false; // Is the gun reloading
    private bool canShoot = true; // Can the gun shoot

    public Vector3 positionA; // Default position for the gun manager
    public Vector3 positionB; // Aim down sights position for the gun manager
    public float aimSpeed = 10f; // Speed of aiming transition
    public TextMeshProUGUI ammoDisplay; // TextMeshPro object for ammo display
    public bool gunsUnlocked = true; // Controls whether guns are visible/unlocked
    private bool previousGunsUnlocked = false; // Track previous state of gunsUnlocked

    [Header("Recoil Settings")]
    public float recoilDuration = 0.1f; // Duration of the recoil effect
    public float recoilBackwardAmount = 0.02f; // Amount of backward movement recoil
    public float recoilUpwardRotation = 5f; // Amount of upward rotation recoil in degrees

    private Coroutine shootingCoroutine; // Coroutine for continuous shooting when holding down the button
    private Coroutine reloadCoroutine; // Coroutine for reloading
    private Vector3 originalPosition; // Original local position of the gun
    private Quaternion originalRotation; // Original local rotation of the gun
    private AudioSource audioSource; // Audio source component

    private void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        audioSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource component

        // Initialize currentBulletsInMag for each weapon
        foreach (Weapon weapon in weapons)
        {
            weapon.currentBulletsInMag = weapon.bulletsPerMag;
        }

        SelectWeapon(0); // Start with the first weapon if desired
        UpdateAmmoDisplay(); // Initialize ammo display
    }

    private void Update()
    {
        if (gunsUnlocked != previousGunsUnlocked)
        {
            // Check if gunsUnlocked has just been turned true
            if (gunsUnlocked)
            {
                SelectWeapon(currentWeaponIndex >= 0 ? currentWeaponIndex : 0);
            }
            previousGunsUnlocked = gunsUnlocked; // Update previous state
        }

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
                // Handle automatic shooting
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
                // Handle single-shot firing
                if (Input.GetMouseButtonDown(0) && canShoot)
                {
                    Shoot();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentWeapon.currentBulletsInMag < currentWeapon.bulletsPerMag && !isReloading)
        {
            reloadCoroutine = StartCoroutine(Reload());
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

        if (isReloading)
        {
            if (reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                reloadCoroutine = null;
            }
            isReloading = false;
            canShoot = true;
            // Reset gun position and rotation
            Vector3 targetPosition = Input.GetMouseButton(1) ? positionB : positionA;
            transform.localPosition = targetPosition;
            transform.localRotation = originalRotation;
        }

        foreach (Weapon weapon in weapons)
        {
            weapon.weaponObject.SetActive(false);
        }

        if (gunsUnlocked && index >= 0 && index < weapons.Length)
        {
            weapons[index].weaponObject.SetActive(true);
            currentWeapon = weapons[index];
            currentWeaponIndex = index;
            canShoot = true;
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
        if (currentWeapon.currentBulletsInMag <= 0)
        {
            reloadCoroutine = StartCoroutine(Reload());
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

        currentWeapon.currentBulletsInMag--; // Decrease bullets count
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

        float reloadTimeRemaining = currentWeapon.reloadTime;
        while (reloadTimeRemaining > 0f)
        {
            // If we switched weapons during reload, exit coroutine
            if (!isReloading)
            {
                yield break;
            }
            reloadTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        currentWeapon.currentBulletsInMag = currentWeapon.bulletsPerMag;
        UpdateAmmoDisplay();

        while (Vector3.Distance(transform.localPosition, originalPosition) > 0.01f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * lowerSpeed);
            yield return null;
        }

        isReloading = false;
        canShoot = true;
        reloadCoroutine = null;
    }

    void UpdateAmmoDisplay()
    {
        if (currentWeapon != null)
        {
            ammoDisplay.text = $"{currentWeapon.currentBulletsInMag}/{currentWeapon.bulletsPerMag}";
        }
        else
        {
            ammoDisplay.text = ""; // Clear ammo display if no weapon is selected
        }
    }
}
