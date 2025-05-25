using UnityEngine;
using DG.Tweening;
using static UnityEditor.MaterialProperty;

public class LeftRightMover : MonoBehaviour
{
    [SerializeField] private Vector3 moveDirection = Vector3.right; // Can be Vector3.up, Vector3.forward, etc.
    [SerializeField] private float moveDistance = 1f;
    [SerializeField] private float moveDuration = 1f;

    private void Start()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + moveDirection.normalized * moveDistance;

        transform.DOMove(endPosition, moveDuration)
        .SetLoops(-1, LoopType.Yoyo) // Loop forever in a Yoyo (ping-pong) style
        .SetEase(Ease.InOutSine);
    }

    public void StopMoving()
    {
        transform.DOKill(); // Stops all DOTween animations on this transform
    }
}
