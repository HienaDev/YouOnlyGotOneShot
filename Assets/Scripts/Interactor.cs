using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private float maxCheckDistance = 5f;
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private Camera cam;
    private Interactable currentInteractable;
    private Animator animator;

    private void Start()
    {
        cam = Camera.main;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxCheckDistance, interactableLayers))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                currentInteractable = interactable;
                interactable.HandleLook();

                if (Input.GetKeyDown(interactKey))
                {
                    animator.SetTrigger("Interact"); // Assuming you have an Animator component with an "Interact" trigger
                    interactable.HandleInteract();
                }

                return;
            }
        }

        currentInteractable = null;
    }
}
