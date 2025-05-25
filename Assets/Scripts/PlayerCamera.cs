using UnityEngine;

public struct CameraInput
{
    public Vector2 look;
}

public class PlayerCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private float sensitivity = 1f;

    [Header("Pitch Clamping")]
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Smoothing")]
    [SerializeField] private float rotationSmoothTime = 0.05f;

    private Vector3 targetEulerAngles;
    private Vector3 currentEulerAngles;
    private Vector3 smoothVelocity;

    public void Initialize(Transform target)
    {
        transform.position = target.position;
        currentEulerAngles = target.eulerAngles;
        targetEulerAngles = currentEulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        targetEulerAngles.x -= input.look.y * sensitivity;
        targetEulerAngles.y += input.look.x * sensitivity;

        // Clamp vertical angle
        targetEulerAngles.x = Mathf.Clamp(targetEulerAngles.x, minPitch, maxPitch);

        // Smoothly interpolate rotation
        currentEulerAngles = Vector3.SmoothDamp(currentEulerAngles, targetEulerAngles, ref smoothVelocity, rotationSmoothTime);

        transform.eulerAngles = currentEulerAngles;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }

    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = newSensitivity;
    }

    public float GetSensitivity()
    {
        return sensitivity;
    }
}
