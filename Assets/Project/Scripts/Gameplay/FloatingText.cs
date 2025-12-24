using UnityEngine;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

public class FloatingText : MonoBehaviour
{
    [Required] public TextMeshProUGUI tmpText;

    [Title("Animation Settings")]
    public float moveDistance = 150f;
    public float duration = 1.2f;
    public Ease moveEase = Ease.OutCubic;
    public Ease fadeEase = Ease.InQuad;

    private void Awake()
    {
        if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();
    }

    public void Initialize(string text, Color color)
    {
        tmpText.text = text;
        tmpText.color = color;

        // Ensure alpha starts at 1
        tmpText.alpha = 1f;

        // Create Animation Sequence
        Sequence seq = DOTween.Sequence();

        // 1. Move Up relative to current position
        seq.Append(transform.DOLocalMoveY(transform.localPosition.y + moveDistance, duration).SetEase(moveEase));

        // 2. Fade Out concurrently
        seq.Join(tmpText.DOFade(0f, duration * 0.8f).SetDelay(duration * 0.2f).SetEase(fadeEase));

        // 3. Destroy when done so they don't pile up in hierarchy
        seq.OnComplete(() => Destroy(gameObject));
    }
}