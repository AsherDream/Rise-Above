using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using DG.Tweening; // <--- ADD THIS for animations

public class EndScreenSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [ReadOnly] public InspectableItemData myData;
    private EndScreenManager manager;
    private RectTransform rectTransform; // Cache this for better performance

    public void Setup(InspectableItemData data, EndScreenManager screenManager)
    {
        myData = data;
        manager = screenManager;
        rectTransform = GetComponent<RectTransform>();
    }

    // --- INTERACTION LOGIC ---

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null && myData != null)
        {
            // 1. Visual Feedback: "Punch" effect (Quick bounce)
            transform.DOKill(); // Stop any hover animations so they don't fight
            transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 10, 1);

            // 2. Open the details
            manager.ShowItemDetails(myData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Hover: Scale Up slightly
        transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Un-Hover: Return to normal size
        transform.DOScale(1.0f, 0.2f).SetEase(Ease.OutQuad);
    }
}