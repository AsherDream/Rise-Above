using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Title("Dialogue Reactions")]
    [Required] public DialogueNode goodItemReaction;
    [Required] public DialogueNode badItemReaction;

    [Title("Shopping Cart Logic")]
    [Required] public RectTransform cartContentParent;
    [Required] public GameObject cartItemPrefab;

    [Title("Cart Capacity")]
    public int maxCartCapacity = 9;
    public Color cartFullColor = Color.red;

    [Title("Piling Effect Settings")]
    [Required] public RectTransform leftBorder;
    [Required] public RectTransform rightBorder;
    [Required] public RectTransform bottomBorder;

    public float rowHeight = 30f;
    public float positionJitter = 30f;
    public float rotationJitter = 45f;

    // --- Private Piling State ---
    private DraggableItem currentlyHoveringItem = null;
    private int itemsInCartCount = 0;
    private float currentPileX;
    private float currentPileY;

    private void Awake()
    {
        InitializePileCursor();
    }

    private void InitializePileCursor()
    {
        if (bottomBorder != null && leftBorder != null)
        {
            currentPileX = leftBorder.anchoredPosition.x;
            currentPileY = bottomBorder.anchoredPosition.y;
        }
        else
        {
            currentPileX = 0;
            currentPileY = 0;
        }
    }

    // --- Interface Implementations ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            currentlyHoveringItem = draggable;
            bool isCartFull = itemsInCartCount >= maxCartCapacity;

            if (isCartFull)
            {
                if (draggable.itemImage != null)
                    draggable.itemImage.color = cartFullColor;
            }
            else
            {
                draggable.SetHoverState(true);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentlyHoveringItem != null)
        {
            currentlyHoveringItem.ResetToDefaultColor();
            currentlyHoveringItem = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (itemsInCartCount >= maxCartCapacity)
        {
            Debug.Log("Cart is full! Item rejected.");
            if (currentlyHoveringItem) currentlyHoveringItem.ResetToDefaultColor();
            currentlyHoveringItem = null;
            return;
        }

        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.isDropSuccessful = true;
            PlaceItemInPile(draggable.itemImage.sprite, draggable.originalColor);

            // Trigger all logic (Dialogue + Survival Meter)
            RunItemLogic(draggable);

            itemsInCartCount++;
        }

        currentlyHoveringItem = null;
    }

    private void PlaceItemInPile(Sprite itemSprite, Color itemColor)
    {
        if (cartContentParent == null || leftBorder == null || rightBorder == null || bottomBorder == null || cartItemPrefab == null)
            return;

        GameObject newItemGO = Instantiate(cartItemPrefab, cartContentParent);
        Image newItemImage = newItemGO.GetComponent<Image>();
        RectTransform itemRect = newItemGO.GetComponent<RectTransform>();

        if (newItemImage == null || itemRect == null)
        {
            Destroy(newItemGO);
            return;
        }

        newItemImage.sprite = itemSprite;
        newItemImage.color = itemColor;
        itemRect.localScale = Vector3.one;

        float itemWidth = itemRect.rect.width * itemRect.localScale.x;

        if (currentPileX + itemWidth > rightBorder.anchoredPosition.x && currentPileX != leftBorder.anchoredPosition.x)
        {
            currentPileY += rowHeight;
            currentPileX = leftBorder.anchoredPosition.x;
        }

        float baseXPos = currentPileX + (itemWidth / 2);
        float baseYPos = currentPileY;
        float xJitter = Random.Range(-positionJitter, positionJitter);
        float yJitter = Random.Range(-positionJitter / 2, positionJitter / 2);
        float finalXPos = baseXPos + xJitter;
        float finalYPos = baseYPos + yJitter;

        itemRect.anchoredPosition = new Vector2(finalXPos, finalYPos);
        float randomRotation = Random.Range(-rotationJitter, rotationJitter);
        itemRect.localRotation = Quaternion.Euler(0, 0, randomRotation);

        currentPileX += itemWidth;

        Vector2 clampedPos = itemRect.anchoredPosition;
        float halfWidth = itemWidth / 2f;
        clampedPos.x = Mathf.Clamp(clampedPos.x, leftBorder.anchoredPosition.x + halfWidth, rightBorder.anchoredPosition.x - halfWidth);
        itemRect.anchoredPosition = clampedPos;
    }

    // --- MAIN LOGIC HUB ---
    private void RunItemLogic(DraggableItem draggable)
    {
        // 1. Check for GOOD Item (Healing)
        if (draggable.CompareTag("Good_Item"))
        {
            // Dialogue
            if (goodItemReaction != null && DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(goodItemReaction);

            // Survival Meter
            if (SurvivalMeter.Instance != null)
                SurvivalMeter.Instance.OnGoodItemClick();
        }
        // 2. Check for BAD Item (Damage)
        else if (draggable.CompareTag("Bad_Item"))
        {
            // Dialogue
            if (badItemReaction != null && DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(badItemReaction);

            // Survival Meter
            if (SurvivalMeter.Instance != null)
                SurvivalMeter.Instance.OnBadItemClick();
        }

        // 3. Specific Item Dialogue (Optional override)
        if (draggable.itemData != null && draggable.itemData.sisterReaction != null)
        {
            DialogueManager.Instance.StartDialogue(draggable.itemData.sisterReaction);
        }
    }
}