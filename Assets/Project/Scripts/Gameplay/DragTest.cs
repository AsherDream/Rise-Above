using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DragTest : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Assign the main Image component of this item.")]
    public Image itemImage; // Used by the DropZone to get the sprite

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    // --- Private variables to remember our "home" ---
    private Vector2 startPosition;
    private Transform startParent;

    // This will be set by a DropZone if the drop is successful
    // It's the "signal" that we were accepted.
    public bool isDropSuccessful { get; set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (itemImage == null)
        {
            itemImage = GetComponent<Image>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. Remember where we came from
        startPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;
        isDropSuccessful = false; // Reset the flag

        // 2. Become a "ghost"
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // CRITICAL: This lets the mouse "see" the DropZone underneath

        // 3. Fly on top of all other UI
        transform.SetParent(rootCanvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 4. Follow the mouse
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 5. We let go!
        canvasGroup.alpha = 1.0f; // Return to normal
        canvasGroup.blocksRaycasts = true;

        // 6. Did a DropZone accept us?
        if (isDropSuccessful)
        {
            // Yes! The DropZone's logic will handle adding to the cart.
            // This item's job is done.
            Debug.Log("Drop successful. Destroying item.");
            Destroy(gameObject);
        }
        else
        {
            // No! Snap back home.
            Debug.Log("Drop failed. Returning to start.");
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startPosition;
        }
    }
}
