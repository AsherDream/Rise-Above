using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector; // UPDATED: Added Odin

// UPDATED: Added IPointerEnterHandler and IPointerExitHandler for hover
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Title("Item Properties")] // UPDATED
    [Tooltip("Assign the main Image component of this item.")]
    public Image itemImage;

    // UPDATED: Added a new color field for hovering
    [Tooltip("The color the item will tint to when hovering over it.")]
    public Color hoverColor = new Color(1, 1, 0.8f, 1); // A light yellow

    [Tooltip("The color the item will tint to when hovering over a valid drop zone.")]
    public Color validDropColor = Color.yellow;

    [Title("State (Read-Only)")] // UPDATED
    [ReadOnly] // UPDATED
    public Color originalColor { get; private set; }

    [ReadOnly] // UPDATED
    public bool isDropSuccessful { get; set; }

    // --- Private variables ---
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private Vector2 startPosition;
    private Transform startParent;
    private ItemInspector itemInspector;
    private bool isHovering = false; // UPDATED: State to track hovering

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
            // UPDATED: Improved log message
            Debug.LogError($"[DraggableItem] Could not find ItemInspector in scene on {gameObject.name}! Inspect mechanic will not work.", this);
        }
    }

    // --- UPDATED: NEW HOVER METHODS ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemInspector.IsInspecting) return; // Don't hover if inspecting

        isHovering = true;
        itemImage.color = hoverColor;
        Debug.Log($"[DraggableItem] Hover ENTER on {gameObject.name}.", this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (ItemInspector.IsInspecting) return; // Don't reset color if we just clicked inspect

        itemImage.color = originalColor;
        Debug.Log($"[DraggableItem] Hover EXIT on {gameObject.name}.", this);
    }
    // --- END NEW HOVER METHODS ---

    public void OnPointerDown(PointerEventData eventData)
    {
        // Check for Right Mouse Button
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // UPDATED: Added debug log
            Debug.Log($"[DraggableItem] Right-click detected on {gameObject.name}.", this);
            if (itemInspector != null)
            {
                itemInspector.InspectItem(itemImage.sprite);
                Debug.Log($"[DraggableItem] Inspect panel opened via {gameObject.name}.", this);
                isHovering = false; // Stop hovering
                itemImage.color = originalColor; // Reset color
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ItemInspector.IsInspecting) return;
        if (eventData.button == PointerEventData.InputButton.Right) return;

        Debug.Log($"[DraggableItem] Begin DRAG on {gameObject.name}.", this);
        isHovering = false; // Stop hovering
        itemImage.color = originalColor; // Ensure it's not the hover color

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
            // UPDATED: Added debug log
            Debug.Log($"[DraggableItem] End DRAG on {gameObject.name} - SUCCESSFUL.", this);
            Destroy(gameObject);
        }
        else
        {
            // UPDATED: Added debug log
            Debug.Log($"[DraggableItem] End DRAG on {gameObject.name} - FAILED (snapping back).", this);
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startPosition;
            ResetToDefaultColor();
        }
    }

    // --- Public methods for the DropZone to call ---
    // This is for hovering over the DROP ZONE
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

