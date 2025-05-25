using UnityEngine;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;

public class ButtonEffectController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text text1;
    [SerializeField] private TMP_Text text2;

    [Header("Color Settings")]
    [SerializeField] private Color targetColor = Color.yellow;
    private Color originalColor;

    [Header("Scale Settings")]
    [SerializeField] private float scaleUpMultiplier = 1.1f;
    [SerializeField] private float scaleUpDuration = 0.15f;
    [SerializeField] private float scaleDownDuration = 0.1f;

    [Header("Explode Settings")]
    [SerializeField] private float explodeScaleMultiplier = 3f;
    [SerializeField] private float explodeDuration = 0.5f;

    [Header("Click Event")]
    [SerializeField] private float clickEventDelay = 0.3f;
    public UnityEvent onClickEvent;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        ResetScale();

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        originalColor = text1 != null ? text1.color : Color.white;
    }

    public void PlayScaleEffect()
    {
        transform.DOKill();
        if (text1 != null) text1.DOKill();
        if (text2 != null) text2.DOKill();

        transform.DOScale(originalScale * scaleUpMultiplier, scaleUpDuration).SetEase(Ease.OutQuad);

        if (text1 != null) text1.DOColor(targetColor, scaleUpDuration);
    }

    public void ResetScale()
    {
        transform.DOKill();
        transform.DOScale(originalScale, scaleDownDuration).SetEase(Ease.InQuad);

        if (text1 != null) text1.DOColor(originalColor, scaleUpDuration);
    }

    public void PlayExplodeEffect()
    {
        transform.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * explodeScaleMultiplier, explodeDuration).SetEase(Ease.OutBack));

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            seq.Join(cg.DOFade(0f, explodeDuration));
        }
        else
        {
            seq.Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InQuad));
        }

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            TriggerClickEventOnly();
        });
    }

    public void Click()
    {
        PlayScaleEffect();
        DOVirtual.DelayedCall(clickEventDelay, () =>
        {
            ResetScale();
            onClickEvent?.Invoke();
        });
    }

    private void TriggerClickEventOnly()
    {
        DOVirtual.DelayedCall(clickEventDelay, () =>
        {
            onClickEvent?.Invoke();
        });
    }
}
