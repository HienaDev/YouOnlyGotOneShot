using UnityEngine;

public class Knockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForce = 15f;
    public float upwardForce = 5f;
    public float randomDirectionAngle = 45f; // How much randomness in degrees

    private Rigidbody rb;

    void Start()
    {
        // Get Rigidbody component
        rb = GetComponent<Rigidbody>();

        // Make sure we have a Rigidbody
        if (rb == null)
        {
            Debug.LogWarning("Knockback: No Rigidbody found! Adding one automatically.");
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }

    // Simple default knockback - throws object back with random direction
    public void GetKnockedBack()
    {
        // Generate a random backward direction
        float randomAngle = Random.Range(-randomDirectionAngle, randomDirectionAngle);
        Vector3 baseDirection = -transform.forward; // Backwards from where object is facing

        // Rotate the direction by random angle around Y axis
        Vector3 randomDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * baseDirection;

        ApplyKnockback(randomDirection);
    }

    // Main knockback method - gets hit direction from attacker position
    public void GetKnockedBack(Vector3 attackerPosition)
    {
        // Calculate direction from attacker to this object
        Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
        ApplyKnockback(knockbackDirection);
    }

    // Overload - specify exact direction for knockback
    public void GetKnockedBack(Vector3 knockbackDirection, bool normalizeDirection = true)
    {
        if (normalizeDirection)
            knockbackDirection = knockbackDirection.normalized;

        ApplyKnockback(knockbackDirection);
    }

    // Apply the actual knockback force
    private void ApplyKnockback(Vector3 direction)
    {
        // Make sure Rigidbody is not kinematic
        rb.isKinematic = false;
        rb.useGravity = true;
        // Apply the knockback force as an instant impulse
        Vector3 force = direction * knockbackForce + Vector3.up * upwardForce;
        Debug.Log(force);
        rb.AddForce(force, ForceMode.Impulse);
    }

    // Method to set knockback force at runtime
    public void SetKnockbackForce(float force)
    {
        knockbackForce = force;
    }

    // Method to set upward force at runtime
    public void SetUpwardForce(float force)
    {
        upwardForce = force;
    }
}