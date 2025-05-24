using Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;

    private PlayerInputActions inputActions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        inputActions = new PlayerInputActions();
        inputActions.Enable();

        playerCharacter.Initialize();   
        playerCamera.Initialize(playerCharacter.CameraTarget);  
    }

    private void OnDestroy()
    {
        inputActions.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        var input = inputActions.Gameplay;

        // Get camera input and update its rotation.
        var cameraInput = new CameraInput
        {
            look = input.Look.ReadValue<Vector2>()
        };
        playerCamera.UpdateRotation(cameraInput);

        // Get character input and update it.
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame(),
            Crouch = input.Crouch.WasPressedThisFrame() ? CrouchInput.Toggle : CrouchInput.None
        };
        playerCharacter.UpdateInput(characterInput);
    }

    private void LateUpdate()
    {
        playerCamera.UpdatePosition(playerCharacter.CameraTarget);
    }
}
