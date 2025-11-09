using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;

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
    // Removed the unused 'isHovering' variable to fix the warning

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        if (itemImage == null)
            itemImage = GetComponent<Image>();

        originalColor = itemImage.color;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemInspector.IsInspecting) return;
        itemImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemInspector.IsInspecting) return;
        itemImage.color = originalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (itemInspector != null && itemData != null)
            {
                itemInspector.InspectItem(itemData);
                itemImage.color = originalColor;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ItemInspector.IsInspecting) return;
        if (eventData.button == PointerEventData.InputButton.Right) return;

        itemImage.color = originalColor;
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
            Destroy(gameObject);
        }
        else
        {
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startPosition;
            ResetToDefaultColor();
        }
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