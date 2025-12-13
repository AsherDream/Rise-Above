using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening; // Required for DOTween

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Title("Item Data")]
    [Required, InlineEditor]
    public InspectableItemData itemData;

    [Title("Components")]
    [Tooltip("Assign the main Image component of this item.")]
    public Image itemImage;

    [Title("Item Properties")]
    [Tooltip("The color the item will tint to when hovering over it.")]
    public Color hoverColor = new Color(1, 1, 0.8f, 1);

    [Tooltip("The color the item will tint to when hovering over a valid drop zone.")]
    public Color validDropColor = Color.yellow;

    [Title("State (Read-Only)")]
    [ReadOnly]
    public Color originalColor { get; private set; }

    [ReadOnly]
    public bool isDropSuccessful { get; set; }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private Vector2 startPosition;
    private Transform startParent;
    private ItemInspector itemInspector;
    private Vector3 originalScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        if (itemImage == null)
            itemImage = GetComponent<Image>();

        if (itemImage != null)
            originalColor = itemImage.color;

        originalScale = transform.localScale;

        itemInspector = FindFirstObjectByType<ItemInspector>();
        if (itemInspector == null)
        {
            Debug.LogError($"[DraggableItem] Could not find ItemInspector in scene on {gameObject.name}!", this);
        }

        if (itemImage != null && itemData != null && itemImage.sprite == null)
        {
            itemImage.sprite = itemData.itemSprite;
        }
    }

    [Button("Force Update Sprite")]
    public void UpdateSpriteFromData()
    {
        if (itemImage != null && itemData != null)
        {
            if (itemData.itemSprite != null)
            {
                itemImage.sprite = itemData.itemSprite;
                itemImage.color = Color.white;
                originalColor = Color.white;
            }
            else
            {
                Debug.LogWarning($"[DraggableItem] ItemData '{itemData.name}' has no sprite assigned!", this);
            }
        }
    }

    // --- Tweens and Logic ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemInspector.IsInspecting) return;

        if (itemImage != null) itemImage.color = hoverColor;

        // Juice: Scale up
        transform.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutBack);

        if (SisterReactionController.Instance != null)
        {
            SisterReactionController.Instance.RegisterHoverStart(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemInspector.IsInspecting) return;

        if (itemImage != null) itemImage.color = originalColor;

        // Juice: Reset Scale
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);

        if (SisterReactionController.Instance != null)
        {
            SisterReactionController.Instance.RegisterHoverEnd(this);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (itemInspector != null && itemData != null)
            {
                itemInspector.InspectItem(itemData);
                if (itemImage != null) itemImage.color = originalColor;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ItemInspector.IsInspecting) return;
        if (eventData.button == PointerEventData.InputButton.Right) return;

        // FIX: Stop all tweens instantly so they don't fight the drag
        transform.DOKill();
        transform.localScale = originalScale;

        if (itemImage != null) itemImage.color = originalColor;
        startPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;
        isDropSuccessful = false;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(rootCanvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ItemInspector.IsInspecting) return;
        if (eventData.button == PointerEventData.InputButton.Right) return;

        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup.blocksRaycasts == true) return;

        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        if (isDropSuccessful)
        {
            // FIX: Kill tweens before destroying to prevent errors!
            transform.DOKill();
            Destroy(gameObject);
        }
        else
        {
            transform.SetParent(startParent);
            rectTransform.DOAnchorPos(startPosition, 0.3f).SetEase(Ease.OutQuad);
            ResetToDefaultColor();
        }
    }

    // Safety cleanup just in case
    private void OnDestroy()
    {
        transform.DOKill();
    }

    public void SetHoverState(bool isValidDrop)
    {
        if (itemImage != null)
        {
            itemImage.color = isValidDrop ? validDropColor : originalColor;
        }
    }

    public void ResetToDefaultColor()
    {
        if (itemImage != null)
        {
            itemImage.color = originalColor;
        }
    }
}