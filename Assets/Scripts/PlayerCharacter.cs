using UnityEngine;
using KinematicCharacterController;


public enum CrouchInput
{
    None,
    Toggle,
    Hold,
}

public enum Stance
{
    Stand,
    Crouch,
    Slide,
}

public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 velocity;
    public Vector3 Acceleration;
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Crouch;
    public bool Sprint;
    public bool Melee;
    public bool Interact;
    public bool Shoot;
    public bool Reload;
    public bool Aim;
}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    public Transform CameraTarget => cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float sprintSpeed = 35f;
    [SerializeField] private float crouchSpeed = 7f;
    [SerializeField] private float walkResponse = 25f;
    [SerializeField] private float crouchResponse = 20f;
    [Space]
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;
    [Space]
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float coyoteTime = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpSustainGravity = 0.4f;
    [SerializeField] private float gravity = -90f;
    [Space]
    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideSprintStartSpeed = 35f;
    [SerializeField] private float slideEndSpeed = 15f;
    [SerializeField] private float slideFriction = 0.8f;
    [SerializeField] private float slideSteerAcceleration = 5f;
    [SerializeField] private float slideGravity = -90f;
    [Space]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;

    [SerializeField] private Camera cam;
    [SerializeField] private float defaultFov = 60f; // Default FOV
    [SerializeField] private float aimingFov = 30f; // Default FOV

    [SerializeField] private Animator leftArmAnimator;

    // These are normalized heights, for how far they are from the bottom
    // to the top of the character
    [Range(0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;

    private CharacterState state;
    public CharacterState State => state;
    private CharacterState lastState;
    public CharacterState LastState => lastState;
    private CharacterState tempState;

    private Quaternion requestedRotation;
    private Vector3 requestedMovement;
    private bool requestedJump;
    private bool requestedSustainedJump;
    private bool requestedCrouch;
    private bool requestedCrouchInAir;
    private bool requestedSprint;
    private float timeSinceUngrounded;
    private float timeSinceJumpRequest;
    private bool ungroundedDueToJump;
    private bool requestedAiming;

    private Collider[] uncrouchOverlapResults;

    [SerializeField] private AudioClip slidingClip;

    public void Initialize()
    {
        state.Stance = Stance.Stand;
        lastState = state;

        uncrouchOverlapResults = new Collider[8];

        motor.CharacterController = this;

        AudioManager.Instance.PlayLoop("Sliding", slidingClip, transform, 0f, true);
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

        var wasRequestingJump = requestedJump;
        // If it's true it stays true, as the updateinput could be called
        // multiple times before the physics tick
        requestedJump = requestedJump || input.Jump;

        if(requestedJump && !wasRequestingJump)
        {
            // If the player requested a jump, we reset the time since the last jump request
            timeSinceJumpRequest = 0f;
        }

        requestedAiming = input.Aim;
        requestedSprint = input.Sprint;

        requestedSustainedJump = input.JumpSustain;

        var wasRequestingCrouch = requestedCrouch;  

        //Toggle
        //requestedCrouch = input.Crouch switch
        //{
        //    CrouchInput.Toggle => !requestedCrouch,
        //    CrouchInput.None => requestedCrouch,
        //    _ => requestedCrouch
        //};

        //Hold
        requestedCrouch = input.Crouch switch
        {
            CrouchInput.Hold => true,
            CrouchInput.None => false,
            _ => requestedCrouch
        };

        if (requestedCrouch && !wasRequestingCrouch)
        {
            requestedCrouchInAir = !state.Grounded;
        }
        else if (!requestedCrouch && wasRequestingCrouch)
        {
            requestedCrouchInAir = false;
        }

        leftArmAnimator.SetBool("Slide", state.Stance is Stance.Slide);
    }

    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalizeHeight = currentHeight / standHeight;
        var cameraTargetHeight = currentHeight * (state.Stance is Stance.Stand ? standCameraTargetHeight : crouchCameraTargetHeight);


        var rootTargetScale = new Vector3(1f, normalizeHeight, 1f);

        cameraTarget.localPosition = Vector3.Lerp
            (
                a: cameraTarget.localPosition,
                b: new Vector3(0f, cameraTargetHeight, 0f),
                t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime) // This is better at staying frame rate independent than this: crouchHeightResponse * deltaTime
            );

        root.localScale = Vector3.Lerp
                (
                    a: root.localScale,
                    b: rootTargetScale,
                    t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
                );


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
        if(state.Stance is Stance.Slide)
        {
            AudioManager.Instance.SetLoopVolume
            (
                key: "Sliding",
                volume: Mathf.Clamp01(currentVelocity.magnitude / 30f) // Assuming 30 is the max speed for sliding
            );
        }
        else
        {
            AudioManager.Instance.SetLoopVolume
                (
                key: "Sliding",
                volume: 0f // Stop sliding sound when not sliding
            );
        }
            Time.timeScale = 1f; // Slow down time when aiming
        state.Acceleration = Vector3.zero; // Reset the acceleration
        // If on the ground
        if (motor.GroundingStatus.IsStableOnGround)
        {
            timeSinceUngrounded = 0f; // Reset the time since the last time we were grounded
            ungroundedDueToJump = false;

            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * requestedMovement.magnitude;


            //Start sliding.
            {
                var moving = groundedMovement.sqrMagnitude > 0f;
                var crouching = state.Stance is Stance.Crouch;
                var wasStanding = lastState.Stance is Stance.Stand;
                var wasInAir = !lastState.Grounded;
                if (moving && crouching && (wasStanding || wasInAir))
                {

                    // If the player is moving, crouching and was standing, we start sliding
                    state.Stance = Stance.Slide;

                    // When ladning on stable ground the cahcater motor porjects the velocity onto a flat ground plane.
                    // See: KinematicCharacterMotor.HandleVelocityProjection()
                    // This is normally good, because under normal circumstances the plaeyr shouldnt slide when landing on the ground.
                    // In this case, we *want* the player to slide.
                    // Reproject the last fames (falling) velocity onto the ground normal to slide
                    if(wasInAir)
                    {
                        currentVelocity = Vector3.ProjectOnPlane
                        (
                            vector: lastState.velocity,
                            planeNormal: motor.GroundingStatus.GroundNormal
                        );
                    }


                    var effectiveSlideStartSpeed = requestedSprint? slideSprintStartSpeed : slideStartSpeed;
                    if(!lastState.Grounded && !requestedCrouchInAir)
                    {
                        effectiveSlideStartSpeed = 0f;
                        requestedCrouchInAir = false;
                    }
                    var slideSpeed = Mathf.Max(requestedSprint ? slideSprintStartSpeed : slideStartSpeed, currentVelocity.magnitude);

                    // Velocity is tangent to the surface so it sticks to surface direction
                    currentVelocity = motor.GetDirectionTangentToSurface
                        (
                            direction: currentVelocity,
                            surfaceNormal: motor.GroundingStatus.GroundNormal
                        ) * slideSpeed;
                }
            }

            if (state.Stance is Stance.Stand or Stance.Crouch)
            {

                // Calculate the speed and responsiveness of movement based on Stance
                var speed = state.Stance is Stance.Crouch ? crouchSpeed : (requestedSprint ? sprintSpeed : walkSpeed);

                var response = state.Stance is Stance.Crouch ? crouchResponse : walkResponse;

                // And smoothly move along the ground in that direction
                var targetVelocity = groundedMovement * speed;
                var moveVelocity = Vector3.Slerp
                (
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime) // This is better at staying frame rate independent than this: response * deltaTime
                );

                state.Acceleration = (moveVelocity - currentVelocity) / deltaTime; // Calculate the acceleration
                currentVelocity = moveVelocity;


            }
            else
            {
                // Slide friction
                currentVelocity -= currentVelocity * slideFriction * deltaTime;

                // Slope.
                {
                    var force = Vector3.ProjectOnPlane
                    (
                        vector: -motor.CharacterUp,
                        planeNormal: motor.GroundingStatus.GroundNormal
                    ) * slideGravity;

                    currentVelocity -= force * deltaTime; // Apply slope force
                }

                // Steer
                {
                    // Target velocity is the plaeyrs miovement direction at the current speed
                    var currentSpeed = currentVelocity.magnitude;
                    var targetVelocity = groundedMovement * currentVelocity.magnitude;
                    var steerVelocity = currentVelocity;
                    var steerForce = (targetVelocity - steerVelocity) * slideSteerAcceleration * deltaTime;
                    // Add steer force but clamp velocity so the slide speed doesnt increase due to movement input
                    steerVelocity += steerForce;
                    steerVelocity = Vector3.ClampMagnitude(currentVelocity, currentSpeed); // Limit the speed to the current speed
                
                    state.Acceleration = (steerVelocity - currentVelocity) / deltaTime; // Calculate the acceleration
                    currentVelocity = steerVelocity; // Set the current velocity to the steer velocity
                }

                // Stop
                if (currentVelocity.magnitude < slideEndSpeed)
                {
                    // If the speed is below the slide end speed, we stop sliding
                    state.Stance = Stance.Crouch;

                }
            }
        }
        // else, in the air
        else
        {
            if(requestedAiming)
                Time.timeScale = 0.2f; // Slow down time when aiming

            timeSinceUngrounded += deltaTime; // Increment the time since the last time we were grounded
            // Move in the air
            if (requestedMovement.sqrMagnitude > 0f)
            {
                var planarMovement = Vector3.ProjectOnPlane
                (
                    vector: requestedMovement,
                    planeNormal: motor.CharacterUp
                ) * requestedMovement.magnitude;

                // Current velocity on movement plane.
                var currentPlanarVelocity = Vector3.ProjectOnPlane
                (
                    vector: currentVelocity,
                    planeNormal: motor.CharacterUp
                );

                // Calculate movement force
                var movementForce = planarMovement * airAcceleration * deltaTime;

                // If moving slower than the air speed, treat movementForce as a simple steering force
                if (currentPlanarVelocity.magnitude < airSpeed)
                {
                    // add it to the current planar velocity for a target velocity.
                    var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                    // Limit target velocity to air speed-
                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }
                // Otherwise nerf the movement force when it is in the direction of the current planar velocity
                // to prevent accelrating futher beyond the max air speed.
                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                {
                    // Project  movement force onto the plane whose normal is the current planar velocity.
                    var constrainedNovementForce = Vector3.ProjectOnPlane
                    (
                        vector: movementForce,
                        planeNormal: currentPlanarVelocity.normalized
                    );

                    movementForce = constrainedNovementForce;
                }

                // Prevent air-climbing steep slopes
                if(motor.GroundingStatus.FoundAnyGround)
                {
                    //  If moving in the same direction as the resultant velocity
                    if(Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                    {

                        // Calculate Obstruction Normal 
                        var  obstructionNormal  = Vector3.Cross
                        (
                            motor.CharacterUp,
                            Vector3.Cross
                            (
                                motor.CharacterUp,
                                currentVelocity + movementForce
                            )
                        ).normalized;


                        // Project the movement force onto obstruction plane
                        movementForce = Vector3.ProjectOnPlane
                        (
                            movementForce,
                            obstructionNormal
                        );
                    }
                }

                // Steers towards current velocity
                currentVelocity += movementForce;
            }
            // Gravity
            var effeciveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            if (requestedSustainedJump && verticalSpeed > 0f)
                effeciveGravity *= jumpSustainGravity;

            currentVelocity += motor.CharacterUp * effeciveGravity * deltaTime;
        }

        if (requestedJump)
        {
            var grounded = motor.GroundingStatus.IsStableOnGround;
            var canCoyoteJump = timeSinceUngrounded < coyoteTime && !ungroundedDueToJump;

            if (grounded || canCoyoteJump)
            {
                requestedJump = false; // Reset jump request    
                requestedCrouch = false;
                requestedCrouchInAir = false;

                // Unstick the player from the ground
                motor.ForceUnground(time: 0.1f);
                ungroundedDueToJump = true;

                // Set minimum vertical speed to the jump speed
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
                // Add the difference in current and target vertial speed to the character velocity
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
            else
            {
                timeSinceJumpRequest += deltaTime; // Increment the time since the last jump request

                var canJumpLater = timeSinceJumpRequest < coyoteTime;
                // Deny jump request
                requestedJump = canJumpLater;
            }

        }

        // we'll assume 70 magnitude is the max speed for our FOV changed, can be tweaked
        var fovChange = Mathf.Min(currentVelocity.magnitude, 70f);
        var targetFov = (requestedAiming ? aimingFov : defaultFov) + fovChange;
        cam.fieldOfView = Mathf.Lerp
        (
            a: cam.fieldOfView,
            b: targetFov,
            t: 1f - Mathf.Exp(-10 * deltaTime) // This is better at staying frame rate independent than this: walkResponse * deltaTime
        );
        if (currentVelocity.magnitude > 0.1f)
        {

        }

    }
    public void AfterCharacterUpdate(float deltaTime)
    {
        // Uncrouch after, because it will require extra logic for the player 
        // to not uncrouch into ceilings
        if (!requestedCrouch && state.Stance is not Stance.Stand)
        {
            //Tentaively "standup" the character capsule
            state.Stance = Stance.Stand;
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
                // if it's greater than 0, we cant uncrouch, so we revert the state.Stance back to crouch
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
                state.Stance = Stance.Stand;
            }
        }

        state.Grounded = motor.GroundingStatus.IsStableOnGround;
        state.velocity = motor.Velocity;
        lastState = tempState;
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        tempState = state;
        // Crouch here, because we want to crouch before the speed update
        if (requestedCrouch && state.Stance is Stance.Stand)
        {
            state.Stance = Stance.Crouch;
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
        if (!motor.GroundingStatus.IsStableOnGround && state.Stance is Stance.Slide)
        {
            // If the player is in a slide and is not grounded, we stop sliding
            state.Stance = Stance.Crouch;
        }
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);
        if (killVelocity)
        {
            motor.BaseVelocity = Vector3.zero;
        }
    }

}
