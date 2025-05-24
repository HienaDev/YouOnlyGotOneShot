using UnityEngine;
using UnityEngine.Events;

public class EnemyProximity : MonoBehaviour
{
    [Header("Proximity Settings")]
    public Transform player;
    public float triggerDistance = 3f;
    public bool triggerOnce = true; // If true, only triggers once until reset

    [Header("Events")]
    public UnityEvent OnPlayerClose; // Called when player gets close
    public UnityEvent OnPlayerFar;   // Called when player moves away (optional)

    private bool playerIsClose = false;
    private bool hasTriggered = false;

    void Start()
    {
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
                Debug.LogWarning("EnemyProximity: No player found! Please assign a player or tag your player GameObject with 'Player' tag.");
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within trigger distance
        bool isPlayerClose = distanceToPlayer <= triggerDistance;

        // Player just entered the trigger zone
        if (isPlayerClose && !playerIsClose)
        {
            if (!triggerOnce || !hasTriggered)
            {
                OnPlayerClose.Invoke();
                hasTriggered = true;
            }
            playerIsClose = true;
        }
        // Player just left the trigger zone
        else if (!isPlayerClose && playerIsClose)
        {
            OnPlayerFar.Invoke();
            playerIsClose = false;
        }
    }

    // Public method to reset the trigger (useful if triggerOnce is true)
    public void ResetTrigger()
    {
        hasTriggered = false;
        playerIsClose = false;
    }

    // Public method to change trigger distance at runtime
    public void SetTriggerDistance(float newDistance)
    {
        triggerDistance = Mathf.Max(0f, newDistance);
    }

    // Public method to check if player is currently close
    public bool IsPlayerClose()
    {
        return playerIsClose;
    }

    // Public method to get current distance to player
    public float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }

    // Draw the trigger distance in the Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = playerIsClose ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}