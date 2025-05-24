using Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [SerializeField] private Volume volume;
    [SerializeField] private StanceVignette stanceVignette; 

    private PlayerInputActions inputActions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        inputActions = new PlayerInputActions();
        inputActions.Enable();

        playerCharacter.Initialize();   
        playerCamera.Initialize(playerCharacter.CameraTarget);
        playerCombat.Initialize();
        cameraSpring.Initialize();
        cameraLean.Initialize();

        stanceVignette.Initialize(volume.profile);
    }

    private void OnDestroy()
    {
        inputActions.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        var input = inputActions.Gameplay;

        var deltaTime = Time.deltaTime; 

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
            JumpSustain = input.Jump.IsPressed(),
            Crouch = input.Crouch.IsPressed() ? CrouchInput.Hold : CrouchInput.None,
            // Toggle Crouch
            // Crouch = input.Crouch.WasPressedThisFrame() ? CrouchInput.Toggle : CrouchInput.None,
            Sprint = input.Sprint.IsPressed(),
            Melee = input.Melee.WasPressedThisFrame(),
            Interact = input.Interact.WasPressedThisFrame(),
            Reload = input.Reload.WasPressedThisFrame(),
            Aim = input.Aim.IsPressed(),
            Shoot = input.Shoot.WasPressedThisFrame(),
        };

        playerCharacter.UpdateInput(characterInput);

        playerCharacter.UpdateBody(deltaTime);

        playerCombat.UpdateInput(characterInput);

        playerCamera.UpdatePosition(playerCharacter.CameraTarget);
        

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log(hit);
                Teleport(hit.point);
            }
        }
#endif
    }

    private void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var state = playerCharacter.State;

        cameraSpring.UpdateSpring(deltaTime, playerCharacter.CameraTarget.up);
        cameraLean.UpdateLean(deltaTime, state.Stance is Stance.Slide ,state.Acceleration ,playerCharacter.CameraTarget.up);
    
        stanceVignette.UpdateVignette(deltaTime, state.Stance);
    }

    public void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }
}
