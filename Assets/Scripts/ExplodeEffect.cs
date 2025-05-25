using UnityEngine;
using DG.Tweening;

public class ExplodeEffect : MonoBehaviour
{
    [SerializeField] private float moveUpAmount = 1f;
    [SerializeField] private float moveUpDuration = 0.3f;
    [SerializeField] private float explodeScaleMultiplier = 3f;
    [SerializeField] private float explodeDuration = 0.5f;

    private Vector3 originalLocalPosition;
    private Vector3 originalScale;
    private Renderer objectRenderer;
    private Color originalColor;

    [SerializeField] private bool onEnable = true;

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
        originalScale = transform.localScale;

        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null && objectRenderer.material.HasProperty("_Color"))
        {
            originalColor = objectRenderer.material.color;
        }
    }

    private void OnEnable()
    {
        if(onEnable)
        {
            ResetState();
            Explode();
        }

    }

    private void ResetState()
    {
        transform.localPosition = originalLocalPosition;
        transform.localScale = originalScale;

        if (objectRenderer != null && objectRenderer.material.HasProperty("_Color"))
        {
            Color c = originalColor;
            c.a = 1f;
            objectRenderer.material.color = c;
        }
    }

public void Explode()
{
    Sequence seq = DOTween.Sequence();

    // Move up a little using localPosition
    seq.Append(transform.DOLocalMoveY(originalLocalPosition.y + moveUpAmount, moveUpDuration).SetEase(Ease.OutQuad));

    // Explode: scale up (relative to original scale) and fade out simultaneously
    seq.Append(transform.DOScale(originalScale * explodeScaleMultiplier, explodeDuration).SetEase(Ease.OutBack));

    if (objectRenderer != null && objectRenderer.material.HasProperty("_Color"))
    {
        seq.Join(objectRenderer.material.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0), explodeDuration));
    }
    else
    {
        seq.AppendInterval(explodeDuration);
    }

    // Then scale back down to zero quickly
    seq.Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InQuad));

    // After all, deactivate the object
    seq.OnComplete(() =>
    {
        gameObject.SetActive(false);
    });
}

}
