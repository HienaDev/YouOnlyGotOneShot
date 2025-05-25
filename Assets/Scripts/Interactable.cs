using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [Tooltip("Maximum distance from player camera to be interacted with")]
    public float interactDistance = 5f;

    [Tooltip("Invoked when the object is looked at")]
    public UnityEvent onLookAt;

    [Tooltip("Invoked when the object is interacted with")]
    public UnityEvent onInteract;

    public void HandleLook()
    {
        onLookAt?.Invoke();
    }

    public void HandleInteract()
    {
        onInteract?.Invoke();
    }
}
