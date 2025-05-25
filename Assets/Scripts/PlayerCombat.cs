using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator leftArmAnimator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float reloadTime = 3f;

    [SerializeField] private GameObject reloadText;
    [SerializeField] private Image reloadBarUI;
    [SerializeField] private GameObject reclaimedText;

    private bool hasBullet;

    [SerializeField] private float kickbackAngle = 30f;
    private float KickbackAngle;
    [SerializeField] private float kickbackSpeed = 0.05f;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private Transform rightArmToKickBack;

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private bool isReturning = false;

    private List<GameObject> bulletsSpawned;
    private Coroutine reloadCoroutine;
    private bool reloading = false;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float shootSoundVolume = 1f;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private float reloadSoundVolume = 1f;

    [Header("Punch Settings")]
    [SerializeField] private float punchCooldown = 0.5f;
    private float lastPunchTime = -Mathf.Infinity;

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

        AudioManager.Instance.PlayOneShot(shootSound, transform.position, shootSoundVolume, true);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 direction = (targetPoint - firePoint.position).normalized;

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
        if (Time.time - lastPunchTime < punchCooldown)
            return;

        lastPunchTime = Time.time;
        leftArmAnimator.SetTrigger("Punch");
    }

    public void Reload()
    {
        if (hasBullet || reloading)
        {
            Debug.Log("Already has a bullet, no need to reload.");
            return;
        }

        reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    public void PickUpBullet()
    {
        foreach (GameObject bullet in bulletsSpawned)
        {
            Destroy(bullet);
        }

        AudioManager.Instance.PlayOneShot(reloadSound, transform.position, reloadSoundVolume, true);
        bulletsSpawned.Clear();
        reloading = false;
        hasBullet = true;
        reloadBarUI.enabled = false;
        reclaimedText.SetActive(true);
        reloadText.SetActive(false);

        if (reloadCoroutine != null)
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
            reloadBarUI.fillAmount = 1f - t;
            reloadBarUI.color = Color.Lerp(Color.red, Color.green, t);
            yield return null;
        }

        reloadBarUI.enabled = false;
        hasBullet = true;
        reloading = false;

        AudioManager.Instance.PlayOneShot(reloadSound, transform.position, reloadSoundVolume, true);

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

    private IEnumerator KickbackCoroutine(float kickBackStrength = -15f)
    {
        Quaternion kickbackRotation = originalRotation * Quaternion.Euler(-kickbackAngle, 0f, 0f);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / kickbackSpeed;
            rightArmToKickBack.localRotation = Quaternion.Slerp(originalRotation, kickbackRotation, t);
            yield return null;
        }

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
