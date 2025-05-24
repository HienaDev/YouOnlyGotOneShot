using UnityEngine;
using KinematicCharacterController;
using Newtonsoft.Json.Bson;
using Mono.Cecil.Cil;

public enum CrouchInput
{
    None,
    Toggle,
}

public enum Stance
{
    Stand,
    Crouch,
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public CrouchInput Crouch;
}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    public Transform CameraTarget => cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float crouchSpeed = 7f;
    [Space]
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float gravity = -90f;
    [Space]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;

    // These are normalized heights, for how far they are from the bottom
    // to the top of the character
    [Range(0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;


    private Stance stance;

    private Quaternion requestedRotation;
    private Vector3 requestedMovement;
    private bool requestedJump;
    private bool requestedCrouch;

    private Collider[] uncrouchOverlapResults;

    public void Initialize()
    {
        stance = Stance.Stand;
        uncrouchOverlapResults = new Collider[8];

        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        requestedRotation = input.Rotation;
        requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        // ClampMagnituned keeps it always 1 or under, while normalize forces it to 1
        // seems useful for when you have like accelerated movement, that isnt always
        // 0 or 1 but could be ramping up so its 0.5
        requestedMovement = Vector3.ClampMagnitude(requestedMovement, 1f);
        // Orient the input so it's relative to the direction the player is facing.
        requestedMovement = input.Rotation * requestedMovement;

        // If it's true it stays true, as the updateinput could be called
        // multiple times before the physics tick
        requestedJump = requestedJump || input.Jump;

        requestedCrouch = input.Crouch switch 
        { 
            CrouchInput.Toggle => !requestedCrouch,
            CrouchInput.None => requestedCrouch,
            _ => requestedCrouch
        };
    }

    public void UpdateBody()
    {
        var currentHeight = motor.Capsule.height;
        var normalizeHeight = currentHeight / standHeight;
        var cameraTargetHeight = currentHeight * (stance is Stance.Stand ? standCameraTargetHeight : crouchCameraTargetHeight);

        Debug.Log(currentHeight);

        var rootTargetScale = new Vector3(1f, normalizeHeight, 1f);

        cameraTarget.localPosition = new Vector3(0f, cameraTargetHeight, 0f);
        root.localScale = rootTargetScale;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // Update the character's rotation to face in the same direction as the
        // requested rotation (camera rotation).

        // We dont want the character to pitch up and down, so the direction the character
        // looks should always be "falttened."

        // This is done by projecting a vector pointing in the same direction that
        // the player is looking onto a flat ground plane

        var forward = Vector3.ProjectOnPlane
        (
            requestedRotation * Vector3.forward,
            motor.CharacterUp

        );

        if (forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // If on the ground
        if (motor.GroundingStatus.IsStableOnGround)
        {
            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * requestedMovement.magnitude;

            var speed = stance is Stance.Crouch ? crouchSpeed : walkSpeed;

            currentVelocity = groundedMovement * speed;
        }
        // else, in the air
        else
        {
            // Gravity
            currentVelocity += motor.CharacterUp * gravity * deltaTime;
        }

        if(requestedJump)
        {
            requestedJump = false; // Reset jump request    

            // Unstick the player from the ground
            motor.ForceUnground(time: 0f);

            // Set minimum vertical speed to the jump speed
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
            // Add the difference in current and target vertial speed to the character velocity
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }

    }
    public void AfterCharacterUpdate(float deltaTime)
    {
        // Uncrouch after, because it will require extra logic for the player 
        // to not uncrouch into ceilings
        if (!requestedCrouch && stance is not Stance.Stand)
        {
            //Tentaively "standup" the character capsule
            stance = Stance.Stand;
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: standHeight,
                yOffset: standHeight * 0.5f
            );

            // Then see if the capsule overlaps any colliders before actually
            // allowing the character to standup
            var pos = motor.TransientPosition;
            var rot = motor.TransientRotation;
            var mask = motor.CollidableLayers;
            if (motor.CharacterOverlap(pos, rot, uncrouchOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0)
            {
                // This method CharacterOverlap returns the number of detected overlaps
                // if it's greater than 0, we cant uncrouch, so we revert the stance back to crouch
                // Re-crouch
                requestedCrouch = true;
                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: crouchHeight,
                    yOffset: crouchHeight * 0.5f
                );
            }
            else
            {
                // If there are no overlaps, we can safely uncrouch
                stance = Stance.Stand;
            }
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        // Crouch here, because we want to crouch before the speed update
        if(requestedCrouch && stance is Stance.Stand)
        {
            stance = Stance.Crouch;
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: crouchHeight * 0.5f
            );
        }
    }


    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {

    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

    }


}
