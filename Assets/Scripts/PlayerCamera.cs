using UnityEngine;

public struct CameraInput
{
    public Vector2 look;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 1f;
    private Vector3 eulerAngles;

    public void Initialize( Transform target)
    {
       transform.position = target.position;
       transform.eulerAngles = eulerAngles = target.eulerAngles;    
    }

    public void UpdateRotation(CameraInput input)
    {
        eulerAngles += new Vector3(-input.look.y, input.look.x, 0f) * sensitivity;
        transform.eulerAngles = eulerAngles;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}
