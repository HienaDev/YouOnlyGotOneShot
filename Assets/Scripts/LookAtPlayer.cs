using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [System.Serializable]
    public class LookAtTarget
    {
        public Transform targetObject;
        public Vector3 localForwardAxis = Vector3.forward; // e.g., (1,0,0) for +X, (0,1,0) for +Y, etc.
    }

    [SerializeField] private Transform player;
    [SerializeField] private LookAtTarget[] objectsToRotate;

    private void Start()
    {
        if(player == null)
        {
            player = FindAnyObjectByType<PlayerCharacter>().transform; // Assuming Player is a class that has a Transform
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        foreach (var lookAt in objectsToRotate)
        {
            if (lookAt.targetObject == null) continue;

            Vector3 directionToPlayer = (player.position - lookAt.targetObject.position).normalized;

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
                Quaternion adjustedRotation = targetRotation * Quaternion.FromToRotation(Vector3.forward, lookAt.localForwardAxis.normalized);
                lookAt.targetObject.rotation = adjustedRotation;
            }
        }
    }
}
