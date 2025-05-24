using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public float updateRate = 0.1f; // How often to update the path (in seconds)

    private NavMeshAgent agent;
    private float nextUpdateTime = 0f;

    void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // If no player is assigned, try to find it by tag
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("EnemyFollow: No player found! Please assign a player or tag your player GameObject with 'Player' tag.");
            }
        }
    }

    void Update()
    {
        // Only update if we have both a player and agent, and enough time has passed
        if (player != null && agent != null && Time.time >= nextUpdateTime)
        {
            // Try to find the closest point on the NavMesh to the player's position
            NavMeshHit hit;
            Vector3 targetPosition = player.position;

            // Sample the NavMesh to find a valid position near the player
            if (NavMesh.SamplePosition(player.position, out hit, 10f, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
            }
            else
            {
                // If player is too far from NavMesh, project their position down to find ground
                RaycastHit groundHit;
                if (Physics.Raycast(player.position, Vector3.down, out groundHit, Mathf.Infinity))
                {
                    // Try to sample near the ground position
                    if (NavMesh.SamplePosition(groundHit.point, out hit, 5f, NavMesh.AllAreas))
                    {
                        targetPosition = hit.position;
                    }
                }
            }

            // Set the destination to the valid NavMesh position
            agent.SetDestination(targetPosition);

            // Schedule the next update
            nextUpdateTime = Time.time + updateRate;
        }
    }

    // Optional: Method to change the target at runtime
    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
    }

    // Optional: Method to change update rate at runtime
    public void SetUpdateRate(float newRate)
    {
        updateRate = Mathf.Max(0.01f, newRate); // Minimum 0.01 seconds
    }
}