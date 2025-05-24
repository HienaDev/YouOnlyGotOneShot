using UnityEngine;

public struct CameraInput
{
    public Vector2 look;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 1f;

    // Added fields for vertical angle clamping
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;



    private Vector3 eulerAngles;

    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = eulerAngles = target.eulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        eulerAngles.x -= input.look.y * sensitivity;
        eulerAngles.y += input.look.x * sensitivity;

        // Clamp the vertical rotation (pitch)
        eulerAngles.x = Mathf.Clamp(eulerAngles.x, minPitch, maxPitch);

        transform.eulerAngles = eulerAngles;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}
