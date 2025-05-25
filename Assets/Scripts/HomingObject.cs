using UnityEngine;

public class HomingObject : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public bool autoFindTarget = false;
    public string targetTag = "Player";

    [Header("Movement Settings")]
    public float speed = 10f;
    public float rotationSpeed = 5f;
    public bool useRigidbody = true;

    [Header("Axis Configuration")]
    public TowardsAxis towardsAxis = TowardsAxis.Forward;

    [Header("Homing Behavior")]
    public float homingRange = 50f;
    public float impactDistance = 0.5f;

    [Header("Optional Settings")]
    public bool destroyOnImpact = true;
    public GameObject impactEffect;
    public float maxLifetime = 10f;

    private bool homing = false;

    [SerializeField] private float initialBoost = 30f;

    public enum TowardsAxis
    {
        Forward,    // +Z (default for most objects)
        Back,       // -Z
        Right,      // +X
        Left,       // -X
        Up,         // +Y
        Down        // -Y
    }

    private Rigidbody rb;
    private Vector3 targetDirection;
    private float startTime;
    private bool hasImpacted = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startTime = Time.time;

        // Auto-find target if enabled
        if (autoFindTarget && target == null)
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObj != null)
                target = targetObj.transform;
        }
    }

    public void LauncAnimation()
    {
        GetComponent<Animator>().SetTrigger("Launch");
    }

    public void StartHoming()
    {
        if (!homing)
        {
            homing = true;
            startTime = Time.time;

            // Add small upward boost
            if (rb != null)
            {
                rb.AddForce(Vector3.up * initialBoost, ForceMode.Impulse);
            }
        }
    }

    void Update()
    {
        if (!homing)
            return;

        // Check lifetime
        if (Time.time - startTime > maxLifetime)
        {
            OnImpact();
            return;
        }

        // Exit if no target or already impacted
        if (target == null || hasImpacted) return;

        // Check if target is in range
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > homingRange) return;

        // Check for impact
        if (distanceToTarget <= impactDistance)
        {
            OnImpact();
            return;
        }

        // Calculate direction to target
        targetDirection = (target.position - transform.position).normalized;

        // Handle rotation
        RotateTowardsTarget();

        // Handle movement
        if (!useRigidbody)
        {
            MoveWithTransform();
        }
    }

    void FixedUpdate()
    {
        if (useRigidbody && target != null && !hasImpacted)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= homingRange)
            {
                MoveWithRigidbody();
            }
        }
    }

    void RotateTowardsTarget()
    {
        if (targetDirection == Vector3.zero) return;

        // Get the axis vector based on the selected towards axis
        Vector3 axisVector = GetAxisVector();

        // Calculate the rotation needed to align the chosen axis with the target direction
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, transform.up);

        // Adjust rotation based on the chosen axis
        if (towardsAxis == TowardsAxis.Back)
            targetRotation *= Quaternion.Euler(0, 180, 0);
        else if (towardsAxis == TowardsAxis.Right)
            targetRotation *= Quaternion.Euler(0, -90, 0);
        else if (towardsAxis == TowardsAxis.Left)
            targetRotation *= Quaternion.Euler(0, 90, 0);
        else if (towardsAxis == TowardsAxis.Up)
            targetRotation *= Quaternion.Euler(-90, 0, 0);
        else if (towardsAxis == TowardsAxis.Down)
            targetRotation *= Quaternion.Euler(90, 0, 0);

        // Smoothly rotate towards target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    Vector3 GetAxisVector()
    {
        switch (towardsAxis)
        {
            case TowardsAxis.Forward: return transform.forward;
            case TowardsAxis.Back: return -transform.forward;
            case TowardsAxis.Right: return transform.right;
            case TowardsAxis.Left: return -transform.right;
            case TowardsAxis.Up: return transform.up;
            case TowardsAxis.Down: return -transform.up;
            default: return transform.forward;
        }
    }

    void MoveWithTransform()
    {
        transform.position += targetDirection * speed * Time.deltaTime;
    }

    void MoveWithRigidbody()
    {
        Vector3 force = targetDirection * speed;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, force, Time.fixedDeltaTime * 2f);
    }

    void OnImpact()
    {
        hasImpacted = true;

        // Spawn impact effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, transform.rotation);
        }

        if (destroyOnImpact)
        {
            DestroyObject();
        }
    }

    void DestroyObject()
    {
        Destroy(gameObject, 0.1f);
    }

    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        hasImpacted = false;
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget.transform;
        hasImpacted = false;
    }

    public void ResetHoming()
    {
        hasImpacted = false;
        startTime = Time.time;
    }

    // Visual debugging
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Draw line to target
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);

            // Draw homing range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, homingRange);

            // Draw stopping distance
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position, impactDistance);

            // Draw towards axis
            Gizmos.color = Color.blue;
            Vector3 axisVector = GetAxisVector();
            Gizmos.DrawRay(transform.position, axisVector * 2f);
        }
    }
}