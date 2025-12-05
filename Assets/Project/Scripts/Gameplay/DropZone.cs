using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    // We remove the generic reaction nodes because reactions are now specific to items
    // [Title("Dialogue Reactions")]
    // [Required] public DialogueNode goodItemReaction; ...

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            currentlyHoveringItem = draggable;
            bool isCartFull = itemsInCartCount >= maxCartCapacity;
            if (Scene1Manager.Instance != null && Scene1Manager.Instance.IsCartFull()) isCartFull = true;

            if (isCartFull)
            {
                if (draggable.itemImage != null) draggable.itemImage.color = cartFullColor;
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
        bool isCartFull = itemsInCartCount >= maxCartCapacity;
        if (Scene1Manager.Instance != null && Scene1Manager.Instance.IsCartFull()) isCartFull = true;

        if (isCartFull)
        {
            Debug.Log("[DropZone] Cart is full! Item rejected.");
            if (currentlyHoveringItem) currentlyHoveringItem.ResetToDefaultColor();
            currentlyHoveringItem = null;
            return;
        }

        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.isDropSuccessful = true;
            PlaceItemInPile(draggable.itemImage.sprite, draggable.originalColor);

            // --- NEW: Run logic based on Data Type ---
            RunItemLogic(draggable);

            itemsInCartCount++;

            if (Scene1Manager.Instance != null)
            {
                Scene1Manager.Instance.OnItemCollected();
            }
        }

        currentlyHoveringItem = null;
    }

    private void PlaceItemInPile(Sprite itemSprite, Color itemColor)
    {
        // (This piling visual code remains identical to previous versions)
        if (cartContentParent == null || cartItemPrefab == null) return;

        GameObject newItemGO = Instantiate(cartItemPrefab, cartContentParent);
        Image newItemImage = newItemGO.GetComponent<Image>();
        RectTransform itemRect = newItemGO.GetComponent<RectTransform>();

        if (newItemImage == null || itemRect == null) { Destroy(newItemGO); return; }

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

    // --- NEW LOGIC: DATA DRIVEN ---
    private void RunItemLogic(DraggableItem draggable)
    {
        if (draggable.itemData == null)
        {
            Debug.LogError($"Item {draggable.name} has no ItemData assigned!");
            return;
        }

        // 1. Play Specific Dialogue
        if (draggable.itemData.sisterReaction != null)
        {
            DialogueManager.Instance.StartDialogue(draggable.itemData.sisterReaction);
        }

        // 2. Update Meter based on Type
        if (SurvivalMeter.Instance != null)
        {
            switch (draggable.itemData.itemType)
            {
                case FloodItemType.Essential:
                    SurvivalMeter.Instance.HandleEssentialItem(draggable.itemData.impactValue);
                    break;
                case FloodItemType.Burden:
                    SurvivalMeter.Instance.HandleBurdenItem(draggable.itemData.impactValue);
                    break;
                case FloodItemType.Wasteful:
                    SurvivalMeter.Instance.HandleWastefulItem(draggable.itemData.impactValue);
                    break;
            }
        }
    }
}