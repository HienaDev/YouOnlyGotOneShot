using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator leftArmAnimator;
    [SerializeField] private Transform firePoint;            // Where the bullet spawns (e.g., tip of the pistol)
    [SerializeField] private GameObject bulletPrefab;        // The bullet to shoot
    [SerializeField] private Camera playerCamera;            // Main camera for raycasting
    [SerializeField] private float bulletSpeed = 20f;        // Speed of the bullet
    [SerializeField] private float reloadTime = 3f;

    [SerializeField] private GameObject reloadText;
    [SerializeField] private Image reloadBarUI;
    [SerializeField] private GameObject reclaimedText;

    private bool hasBullet;

    [SerializeField] private float kickbackAngle = 30f;     // How far to rotate back
    private float KickbackAngle; // How far to rotate back when shooting
    [SerializeField] private float kickbackSpeed = 0.05f;   // How fast the kickback happens
    [SerializeField] private float returnSpeed = 5f;        // How fast it returns
    [SerializeField] private Transform rightArmToKickBack;

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private bool isReturning = false;

    private List<GameObject> bulletsSpawned;
    private Coroutine reloadCoroutine;
    private bool reloading = false;
    public void Initialize()
    {
        bulletsSpawned = new List<GameObject>();
        hasBullet = true;
        originalRotation = rightArmToKickBack.localRotation;
    }

    public void UpdateInput(CharacterInput input)
    {
        if (input.Shoot && hasBullet)
        {
            Shoot();
        }

        if (input.Melee)
        {
            Punch();
        }

        if (input.Reload)
        {
            Reload();
        }
    }

    public void Shoot()
    {
        hasBullet = false;
        reloadText.SetActive(true);
        ApplyKickback();

        // Raycast from center of screen to determine where player is aiming
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            // If nothing hit, shoot forward from camera
            targetPoint = ray.GetPoint(100f);
        }

        // Calculate direction from firePoint to targetPoint
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        // Instantiate and launch the bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }

        Debug.Log("Shooting at: " + targetPoint);
    }

    public void Punch()
    {
        leftArmAnimator.SetTrigger("Punch");
    }

    public void Reload()
    {
        if(hasBullet || reloading)
        {
            Debug.Log("Already has a bullet, no need to reload.");
            return;
        }
        reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    public void PickUpBullet()
    {
        foreach( GameObject bullet in bulletsSpawned)
        {
            Destroy(bullet);
        }
        bulletsSpawned.Clear();
        reloading = false;
        hasBullet = true;
        reloadBarUI.enabled = false;
        reclaimedText.SetActive(true);
        reloadText.SetActive(false);
        if(reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);
    }

    private IEnumerator ReloadCoroutine()
    {
        reloading = true;
        reloadText.SetActive(false);
        reloadBarUI.enabled = true;
        reloadBarUI.fillAmount = 1f;
        reloadBarUI.color = Color.red;

        float elapsed = 0f;

        while (elapsed < reloadTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / reloadTime);

            // Fill from 1 to 0
            reloadBarUI.fillAmount = 1f - t;

            // Color from red to green
            reloadBarUI.color = Color.Lerp(Color.red, Color.green, t);

            yield return null;
        }

        reloadBarUI.enabled = false;
        hasBullet = true;
        reloading = false;

        foreach (GameObject bullet in bulletsSpawned)
        {
            Destroy(bullet);
        }
        bulletsSpawned.Clear();
        Debug.Log("Reloaded");
    }

    public void ApplyKickback()
    {
        StartCoroutine(KickbackCoroutine(kickbackAngle));
    }

    private System.Collections.IEnumerator KickbackCoroutine(float kickBackStrength = -15f)
    {
        // Rotate quickly back (simulate recoil)
        Quaternion kickbackRotation = originalRotation * Quaternion.Euler(-kickbackAngle, 0f, 0f);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / kickbackSpeed;
            rightArmToKickBack.localRotation = Quaternion.Slerp(originalRotation, kickbackRotation, t);
            yield return null;
        }

        // Slowly return to original rotation
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            rightArmToKickBack.localRotation = Quaternion.Slerp(kickbackRotation, originalRotation, t);
            yield return null;
        }

        rightArmToKickBack.localRotation = originalRotation;
    }

}
