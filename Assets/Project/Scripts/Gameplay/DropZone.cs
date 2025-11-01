using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic; // Keep this for potential future list use

[RequireComponent(typeof(Image))] // Assumes the drop zone is an Image
public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Title("Shopping Cart Logic")]
    [Required(ErrorMessage = "Assign the RectTransform that will hold the piled items.")]
    [Tooltip("The parent RectTransform where dropped items will be placed.")]
    public RectTransform cartContentParent;

    [Required(ErrorMessage = "Assign the prefab for items added to the cart.")]
    [Tooltip("The prefab to create inside the cart (e.g., a simple UI Image).")]
    public GameObject cartItemPrefab;

    [Title("Cart Capacity")]
    [Tooltip("The maximum number of items allowed in the cart.")]
    public int maxCartCapacity = 9;

    [Tooltip("The color the *item* will flash when you try to add it to a full cart.")]
    public Color cartFullColor = Color.red;

    [Title("Piling Effect Settings")]
    [Required(ErrorMessage = "Assign the Left Border marker.")]
    [Tooltip("Visual markers for the piling area")]
    public RectTransform leftBorder;
    [Required(ErrorMessage = "Assign the Right Border marker.")]
    public RectTransform rightBorder;
    [Required(ErrorMessage = "Assign the Bottom Border marker.")]
    public RectTransform bottomBorder;

    [Tooltip("How much vertical space (in pixels) should be between each row?")]
    public float rowHeight = 30f;

    [Tooltip("How 'messy' should the pile be? (in pixels)")]
    public float positionJitter = 30f;

    [Tooltip("How 'tilted' should the items be? (in degrees)")]
    public float rotationJitter = 45f;

    // --- Private Piling State ---
    private DraggableItem currentlyHoveringItem = null;
    private int itemsInCartCount = 0;

    // The "cursor" for where the next item should be placed
    private float currentPileX;
    private float currentPileY;

    private void Awake()
    {
        InitializePileCursor();
    }

    // Initialize or reset the starting position for the pile
    private void InitializePileCursor()
    {
        if (bottomBorder != null && leftBorder != null)
        {
            currentPileX = leftBorder.anchoredPosition.x;
            currentPileY = bottomBorder.anchoredPosition.y;
        }
        else
        {
            Debug.LogError("DropZone is missing its Border references! Piling will not work correctly.", this);
            currentPileX = 0;
            currentPileY = 0;
        }
    }

    // --- Interface Implementations (These were missing) ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return; // Nothing is being dragged

        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            currentlyHoveringItem = draggable; // Keep track of the item hovering
            bool isCartFull = itemsInCartCount >= maxCartCapacity;

            if (isCartFull)
            {
                // Cart is full, make the item flash the "full" color
                if (draggable.itemImage != null) // Safety check
                {
                    draggable.itemImage.color = cartFullColor;
                }
            }
            else
            {
                // Cart has space, make the item use its "valid drop" color
                draggable.SetHoverState(true);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // If an item was hovering and now leaves, reset its color
        if (currentlyHoveringItem != null)
        {
            currentlyHoveringItem.ResetToDefaultColor();
            currentlyHoveringItem = null; // No longer hovering
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // --- 1. Check Cart Capacity ---
        if (itemsInCartCount >= maxCartCapacity)
        {
            Debug.Log("Cart is full! Item rejected.");
            // Reset item color if it was flashing red
            if (currentlyHoveringItem) currentlyHoveringItem.ResetToDefaultColor();
            currentlyHoveringItem = null; // Ensure we clear the hover reference
            // NOTE: isDropSuccessful remains false on the item, so it will snap back
            return; // Stop processing the drop
        }

        // --- 2. Get the Dropped Item ---
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            // --- 3. Process Successful Drop ---
            // Tell the original item it was dropped successfully.
            // This will cause it to Destroy itself in its OnEndDrag.
            draggable.isDropSuccessful = true;

            // Create a visual copy in the cart pile
            PlaceItemInPile(draggable.itemImage.sprite, draggable.originalColor);

            // Run game logic (check tags, add points, etc.)
            RunItemLogic(draggable);

            // Increment the count of items in the cart
            itemsInCartCount++;
        }

        // Clear the hover reference, drop is complete
        currentlyHoveringItem = null;
    }

    // --- Piling Logic ---
    private void PlaceItemInPile(Sprite itemSprite, Color itemColor)
    {
        // Safety checks for required references
        if (cartContentParent == null || leftBorder == null || rightBorder == null || bottomBorder == null || cartItemPrefab == null)
        {
            Debug.LogError("DropZone is missing required references in the Inspector! Cannot place item.", this);
            return;
        }

        // 1. Create a new item instance from the prefab
        GameObject newItemGO = Instantiate(cartItemPrefab, cartContentParent);
        Image newItemImage = newItemGO.GetComponent<Image>();
        RectTransform itemRect = newItemGO.GetComponent<RectTransform>();

        if (newItemImage == null || itemRect == null)
        {
            Debug.LogError("CartItemPrefab is missing Image or RectTransform component!", newItemGO);
            Destroy(newItemGO); // Clean up bad instance
            return;
        }

        // 2. Set the visual properties (sprite and color)
        newItemImage.sprite = itemSprite;
        newItemImage.color = itemColor;

        // 3. Ensure scale is correct after instantiating
        itemRect.localScale = Vector3.one;

        // 4. Determine item width based on the prefab's RectTransform size
        float itemWidth = itemRect.rect.width * itemRect.localScale.x;

        // --- 5. Row Wrapping Logic ---
        if (currentPileX + itemWidth > rightBorder.anchoredPosition.x && currentPileX != leftBorder.anchoredPosition.x)
        {
            currentPileY += rowHeight;
            currentPileX = leftBorder.anchoredPosition.x;
            Debug.Log("Starting new row at Y: " + currentPileY);
        }

        // --- 6. Calculate Position with Jitter ---
        float baseXPos = currentPileX + (itemWidth / 2);
        float baseYPos = currentPileY;
        float xJitter = Random.Range(-positionJitter, positionJitter);
        float yJitter = Random.Range(-positionJitter / 2, positionJitter / 2);
        float finalXPos = baseXPos + xJitter;
        float finalYPos = baseYPos + yJitter;

        // --- 7. Apply Position and Rotation ---
        itemRect.anchoredPosition = new Vector2(finalXPos, finalYPos);
        float randomRotation = Random.Range(-rotationJitter, rotationJitter);
        itemRect.localRotation = Quaternion.Euler(0, 0, randomRotation);

        // --- 8. Update X Cursor for Next Item ---
        currentPileX += itemWidth;

        // --- 9. Optional: Keep items within bounds (simple clamp) ---
        Vector2 clampedPos = itemRect.anchoredPosition;
        float halfWidth = itemWidth / 2f;
        clampedPos.x = Mathf.Clamp(clampedPos.x, leftBorder.anchoredPosition.x + halfWidth, rightBorder.anchoredPosition.x - halfWidth);
        itemRect.anchoredPosition = clampedPos;
    }

    // --- Game Logic Hook ---
    private void RunItemLogic(DraggableItem draggable)
    {
        if (draggable.CompareTag("Good_Item"))
        {
            Debug.Log("A GOOD item was added. (Add survival points here)");
            // Example: FindObjectOfType<SurvivalMeter>().AddPoints(10);
        }
        else if (draggable.CompareTag("Bad_Item"))
        {
            Debug.Log("A BAD item was added. (Subtract survival points here)");
            // Example: FindObjectOfType<SurvivalMeter>().AddPoints(-10);
        }
        else
        {
            Debug.Log($"Item dropped with unhandled tag: {draggable.tag}");
        }
    }
}

