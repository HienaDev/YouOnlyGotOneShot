using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator leftArmAnimator;
    [SerializeField] private Transform firePoint;            // Where the bullet spawns (e.g., tip of the pistol)
    [SerializeField] private GameObject bulletPrefab;        // The bullet to shoot
    [SerializeField] private Camera playerCamera;            // Main camera for raycasting
    [SerializeField] private float bulletSpeed = 20f;        // Speed of the bullet

    private bool hasBullet;

    public void Initialize()
    {
        hasBullet = true;
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
    }

    public void Shoot()
    {
        //hasBullet = false;

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
}
