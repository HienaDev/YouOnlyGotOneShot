using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public float updateRate = 0.1f; // How often to update the path (in seconds)
    public float navMeshSampleDistance = 10f; // Max distance to sample for NavMesh position

    private NavMeshAgent agent;
    private float nextUpdateTime = 0f;
    private bool isFollowing = true;
    private Vector3 lastTargetPosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        lastTargetPosition = Vector3.positiveInfinity; // Initialize with impossible value
    }

    void Start()
    {
        // If no player is assigned, try to find it by tag
        if (player == null)
        {
            FindPlayerByTag();
        }
    }

    void Update()
    {
        if (!ShouldUpdatePath()) return;

        UpdatePathToPlayer();
        nextUpdateTime = Time.time + updateRate;
    }

    private bool ShouldUpdatePath()
    {
        return isFollowing &&
               player != null &&
               agent != null &&
               agent.isActiveAndEnabled &&
               agent.isOnNavMesh &&
               Time.time >= nextUpdateTime;
    }

    private void UpdatePathToPlayer()
    {
        Vector3 targetPosition = GetValidNavMeshPosition(player.position);

        // Only update if the target position has changed significantly
        if (Vector3.Distance(targetPosition, lastTargetPosition) > agent.stoppingDistance)
        {
            agent.SetDestination(targetPosition);
            lastTargetPosition = targetPosition;
        }
    }

    private Vector3 GetValidNavMeshPosition(Vector3 desiredPosition)
    {
        NavMeshHit hit;

        // First try to find position on NavMesh
        if (NavMesh.SamplePosition(desiredPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // If not on NavMesh, try raycasting down to find ground
        if (Physics.Raycast(desiredPosition, Vector3.down, out RaycastHit groundHit, Mathf.Infinity))
        {
            // Try to sample near the ground position
            if (NavMesh.SamplePosition(groundHit.point, out hit, navMeshSampleDistance * 0.5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // Fallback - return original position (agent might not be able to reach this)
        return desiredPosition;
    }

    private void FindPlayerByTag()
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

    public void StopFollowing()
    {
        isFollowing = false;
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.ResetPath();
        }
    }

    public void StartFollowing()
    {
        isFollowing = true;
        nextUpdateTime = Time.time; // Allow immediate update
    }

    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
        nextUpdateTime = Time.time; // Allow immediate update
    }

    public void SetUpdateRate(float newRate)
    {
        updateRate = Mathf.Max(0.01f, newRate);
    }
}