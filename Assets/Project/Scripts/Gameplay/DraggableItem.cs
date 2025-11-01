using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Assign the main Image component of this item.")]
    public Image itemImage; // Used by the DropZone to get the sprite

    [Tooltip("The color the item will tint to when hovering over a valid drop zone.")]
    public Color validDropColor = Color.yellow;

    // Public property for the DropZone to read the item's true color
    public Color originalColor { get; private set; }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    // --- Private variables to remember our "home" ---
    private Vector2 startPosition;
    private Transform startParent;

    // This will be set by a DropZone if the drop is successful
    public bool isDropSuccessful { get; set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        // This is a safer way to find the root canvas
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        if (itemImage == null)
        {
            itemImage = GetComponent<Image>();
        }
        originalColor = itemImage.color;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
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
        // This logic is safer and accounts for screen scaling
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        if (isDropSuccessful)
        {
            // Yes! The DropZone created a copy. Destroy this original.
            Destroy(gameObject);
        }
        else
        {
            // No! Snap back home.
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startPosition;
            ResetToDefaultColor(); // Reset color if we flashed red
        }
    }

    // --- Public methods for the DropZone to call ---

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

