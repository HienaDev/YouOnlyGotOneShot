using UnityEngine;

public class GnomeAttack : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;

    [Header("Launch Settings")]
    public string launchTrigger = "Launch";
    public string launchStateName = "Launch"; // Name of the launch animation state

    [Header("Homing Settings")]
    public float homingSpeed = 8f;
    public float rotationSpeed = 5f;
    public float archHeight = 5f; // How high the arc goes
    public float archIntensity = 2f; // How pronounced the arc effect is

    private Animator animator;
    private Rigidbody rb;
    private bool isLaunching = false;
    private bool isHoming = false;
    private Vector3 velocity;

    void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("GnomeAttack: No player found! Please assign a player or tag your player GameObject with 'Player' tag.");
            }
        }

        // Make sure we have required components
        if (animator == null)
        {
            Debug.LogError("GnomeAttack: No Animator found! Please add an Animator component.");
        }

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false; // We'll handle our own movement
        }
    }

    // Public method to start the attack sequence
    public void StartAttack()
    {
        if (isLaunching || isHoming) return; // Already attacking

        if (animator != null)
        {
            isLaunching = true;
            animator.SetTrigger(launchTrigger);

            // Get the animation duration and use it as delay
            float animationDuration = GetAnimationDuration(launchStateName);
            Invoke(nameof(StartHoming), animationDuration);
        }
    }

    // Get the duration of a specific animation state
    private float GetAnimationDuration(string stateName)
    {
        if (animator == null) return 1f;

        // Get all animation clips from the animator
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in clips)
        {
            if (clip.name == stateName)
            {
                return clip.length;
            }
        }

        // If animation not found, return default duration
        Debug.LogWarning($"GnomeAttack: Animation '{stateName}' not found! Using default duration of 1 second.");
        return 1f;
    }

    // Start the homing behavior
    private void StartHoming()
    {
        isLaunching = false;
        isHoming = true;

        // Disable gravity and make rigidbody kinematic for custom movement
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (isHoming && player != null)
        {
            HomingMovement();
        }
    }

    private void HomingMovement()
    {
        // Calculate direction to player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Calculate desired velocity toward player
        Vector3 targetVelocity = directionToPlayer * homingSpeed;

        // Add arching effect by adding upward force based on horizontal distance
        float horizontalDistance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x, 0, player.position.z)
        );

        // Create arch by adding upward velocity when far, downward when close
        float archForce = Mathf.Sin(horizontalDistance / archIntensity) * archHeight;
        targetVelocity.y += archForce;

        // Smoothly adjust current velocity toward target velocity
        velocity = Vector3.Lerp(velocity, targetVelocity, rotationSpeed * Time.deltaTime);

        // Move the gnome
        transform.position += velocity * Time.deltaTime;

        // Rotate to face movement direction
        if (velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Public method to stop the attack
    public void StopAttack()
    {
        isLaunching = false;
        isHoming = false;

        // Cancel any pending homing start
        CancelInvoke(nameof(StartHoming));

        // Re-enable gravity if needed
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    // Public method to check if currently attacking
    public bool IsAttacking()
    {
        return isLaunching || isHoming;
    }

    // Reset the gnome (useful for pooling or restarting)
    public void ResetGnome()
    {
        StopAttack();
        velocity = Vector3.zero;
    }

    // Handle collisions during homing
    void OnTriggerEnter(Collider other)
    {
        if (isHoming && other.transform == player)
        {
            // Hit the player! Do damage or whatever here
            Debug.Log("Gnome hit the player!");
            StopAttack();

            // You can add damage dealing, effects, etc. here
            // For example: other.GetComponent<PlayerHealth>()?.TakeDamage(10);
        }
    }

    // Draw debug info in Scene view
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = isHoming ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position, player.position);

            if (isHoming)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, velocity);
            }
        }
    }
}