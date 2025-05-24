using UnityEngine;
using DG.Tweening;
using static UnityEditor.MaterialProperty;

public class LeftRightMover : MonoBehaviour
{
    [SerializeField] private float moveDistance = 1f;
    [SerializeField] private float moveDuration = 1f;

    private void Start()
    {
        // Save the starting position
        Vector3 startPosition = transform.position;

        // Create a left-right tween loop
        transform.DOMoveX(startPosition.x + moveDistance, moveDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
