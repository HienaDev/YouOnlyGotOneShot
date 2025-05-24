using Newtonsoft.Json.Bson;
using UnityEngine;

public class CameraSpring : MonoBehaviour
{

    [Min(0.01f)]
    [SerializeField] private float halflife = 0.075f; // Time it takes to reach half the distance to the target
    [Space]
    [SerializeField] private float frequency = 18f;
    [Space]
    [SerializeField] private float angularDisplacement = 2f;
    [SerializeField] private float linearDisplacement = 0.05f;

    private Vector3 springPosition;
    private Vector3 springVelocity;
    public void Initialize()
    {
        springPosition = transform.position;
        springVelocity = Vector3.zero;
    }

    public void UpdateSpring(float deltaTime, Vector3 up)
    {
        transform.localPosition = Vector3.zero;

        Spring(ref springPosition, ref springVelocity, transform.position, halflife, frequency, deltaTime);

        var localSpringPosition = springPosition - transform.position;
        var springHeight = Vector3.Dot(localSpringPosition, up); 

        transform.localEulerAngles = new Vector3(-springHeight * angularDisplacement, 0f, 0f);
        transform.localPosition = localSpringPosition * linearDisplacement;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position,springPosition);
        Gizmos.DrawSphere(springPosition, 0.1f);
    }

    // https://allenchou.net/2015/04/game-math-more-on-numeric-springing/
    private static void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halflife, float frequency, float timeStep)
    {
        var dampingRatio = -Mathf.Log(0.5f) / (frequency * halflife);
        var f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
        var oo = frequency * frequency;
        var hoo = timeStep * oo;
        var hhoo = timeStep * hoo;
        var detInv = 1.0f / (f + hhoo);
        var detX = f * current + timeStep * velocity + hhoo * target;
        var detV = velocity + hoo * (target - current);
        current = detX * detInv;
        velocity = detV * detInv;
    }

}
