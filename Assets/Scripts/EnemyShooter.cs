using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float detectionRange = 10f;
    public float fireRate = 1f; // Shots per second
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    [Header("Line of Sight")]
    public LayerMask obstacleLayerMask = -1; // What layers block line of sight

    [SerializeField] private Transform player;
    private float nextFireTime;

    void Start()
    {

        // If no fire point is assigned, use this object's transform
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within range
        if (distanceToPlayer <= detectionRange)
        {
            // Check if we have line of sight to the player
            if (HasLineOfSight())
            {
                // Check if enough time has passed to fire again
                if (Time.time >= nextFireTime)
                {
                    ShootAtPlayer();
                    nextFireTime = Time.time + (1f / fireRate);
                }
            }

        }
    }

    bool HasLineOfSight()
    {
        Vector3 directionToPlayer = (player.position - firePoint.position).normalized;
        float distanceToPlayer = Vector3.Distance(firePoint.position, player.position);

        // Cast a ray towards the player
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, directionToPlayer, out hit, distanceToPlayer, obstacleLayerMask))
        {
            // If the ray hits something before reaching the player, line of sight is blocked
            Debug.Log("Line of sight blocked by: " + hit.collider.name);
            return false;
        }

        return true;
    }

    void ShootAtPlayer()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("No bullet prefab assigned to " + gameObject.name);
            return;
        }

        // Calculate direction to player
        Vector3 directionToPlayer = (player.position - firePoint.position).normalized;

        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(directionToPlayer));
        Debug.Log("Shot at player");
        // Add velocity to bullet (assuming it has a Rigidbody)
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = directionToPlayer * bulletSpeed;
        }

        // Optional: Destroy bullet after some time to prevent memory leaks
        Destroy(bullet, 5f);
    }

    // Visualize detection range and line of sight in the editor
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw line of sight if player exists
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                // Green line if clear line of sight, red if blocked
                Gizmos.color = HasLineOfSight() ? Color.green : Color.red;
                Gizmos.DrawLine(firePoint != null ? firePoint.position : transform.position, player.position);
            }
        }
    }
}