using UnityEngine;
using DG.Tweening;

public class SetTextColor : MonoBehaviour
{
    [SerializeField] private Color highlightedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private TMPro.TextMeshProUGUI text;

    private Vector3 originalScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalScale =transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Highlight()
    {
        
        if (text != null)
        {
            text.color = highlightedColor;
        }
    }

    public void ResetColor()
    {
        if (text != null)
        {
            text.color = normalColor;
        }
    }

    public void ScaleUp()
    {
        transform
            .DOScale(originalScale * 1.2f, 0.1f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); // ✅ This makes it ignore timeScale
    }

    public void ScaleDown()
    {
        transform
            .DOScale(originalScale, 0.1f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); // ✅ This makes it ignore timeScale
    }

}
