using UnityEngine;
using DG.Tweening;

public class AnimateButton : MonoBehaviour
{
    [Header("Target Transform to Animate")]
    [SerializeField] private Transform target;

    [Header("Animation Settings")]
    [SerializeField] private float pressDistance = 0.1f;
    [SerializeField] private float pressDuration = 0.2f;
    [SerializeField] private float delayBetweenPresses = 1f;
    [SerializeField] private Ease easing = Ease.InOutSine;

    private Vector3 initialPosition;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("AnimateButton: Target Transform is not assigned.");
            return;
        }

        initialPosition = target.localPosition;

    }

    public void StartAnimating()
    {
        Sequence buttonSequence = DOTween.Sequence();

        buttonSequence.Append(target.DOLocalMoveZ(initialPosition.z - pressDistance, pressDuration).SetEase(easing));
        buttonSequence.Append(target.DOLocalMoveZ(initialPosition.z, pressDuration).SetEase(easing));
    }
}
