using UnityEngine;
using UnityEngine.Events;

public class EnemyProximity : MonoBehaviour
{
    [Header("Proximity Settings")]
    public Transform player;
    public float triggerDistance = 3f;
    public float veryCloseDistance = 1f;

    [Tooltip("Cooldown in seconds between OnPlayerClose triggers")]
    public float closeTriggerCooldown = 5f;

    [Tooltip("Cooldown in seconds between OnPlayerVeryClose triggers")]
    public float veryCloseTriggerCooldown = 5f;

    [Header("Events")]
    public UnityEvent OnPlayerClose;
    public UnityEvent OnPlayerVeryClose;
    public UnityEvent OnPlayerFar;

    private float lastCloseTriggerTime = -Mathf.Infinity;
    private float lastVeryCloseTriggerTime = -Mathf.Infinity;
    private ProximityState currentState = ProximityState.Far;

    private enum ProximityState
    {
        Far,
        Close,
        VeryClose
    }

    void Start()
    {
        FindPlayerIfNull();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayerIfNull();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        CheckProximityStates(distanceToPlayer);
    }

    private void FindPlayerIfNull()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("EnemyProximity: No player found!");
            }
        }
    }

    private void CheckProximityStates(float distance)
    {
        // Check for state transitions
        if (distance <= veryCloseDistance)
        {
            HandleVeryCloseState();
        }
        else if (distance <= triggerDistance)
        {
            HandleCloseState();
        }
        else
        {
            HandleFarState();
        }
    }

    private void HandleVeryCloseState()
    {
        if (currentState != ProximityState.VeryClose)
        {
            // State transition to VeryClose
            currentState = ProximityState.VeryClose;
            TryTriggerVeryCloseEvent();
        }
        else
        {
            // Already in VeryClose state - check cooldown
            TryTriggerVeryCloseEvent();
        }
    }

    private void HandleCloseState()
    {
        if (currentState != ProximityState.Close)
        {
            // State transition to Close
            currentState = ProximityState.Close;
            TryTriggerCloseEvent();
        }
        else
        {
            // Already in Close state - check cooldown
            TryTriggerCloseEvent();
        }
    }

    private void HandleFarState()
    {
        if (currentState != ProximityState.Far)
        {
            // State transition to Far
            currentState = ProximityState.Far;
            OnPlayerFar.Invoke();
        }
    }

    private void TryTriggerVeryCloseEvent()
    {
        if (Time.time - lastVeryCloseTriggerTime >= veryCloseTriggerCooldown)
        {
            OnPlayerVeryClose.Invoke();
            lastVeryCloseTriggerTime = Time.time;
            // Also trigger close event if cooldown allows
            if (Time.time - lastCloseTriggerTime >= closeTriggerCooldown)
            {
                OnPlayerClose.Invoke();
                lastCloseTriggerTime = Time.time;
            }
        }
    }

    private void TryTriggerCloseEvent()
    {
        if (Time.time - lastCloseTriggerTime >= closeTriggerCooldown)
        {
            OnPlayerClose.Invoke();
            lastCloseTriggerTime = Time.time;
        }
    }

    public void ResetTrigger()
    {
        lastCloseTriggerTime = -Mathf.Infinity;
        lastVeryCloseTriggerTime = -Mathf.Infinity;
        currentState = ProximityState.Far;
    }

    public void SetTriggerDistance(float newDistance)
    {
        triggerDistance = Mathf.Max(0f, newDistance);
    }

    public bool IsPlayerClose()
    {
        return currentState == ProximityState.Close || currentState == ProximityState.VeryClose;
    }

    public bool IsPlayerVeryClose()
    {
        return currentState == ProximityState.VeryClose;
    }

    public float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = IsPlayerClose() ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);

        Gizmos.color = IsPlayerVeryClose() ? Color.magenta : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, veryCloseDistance);
    }
}