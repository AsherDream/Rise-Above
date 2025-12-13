using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening; // REQUIRED: Make sure DOTween is imported

[RequireComponent(typeof(Image))]
public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
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

    private void Awake() { InitializePileCursor(); }

    private void InitializePileCursor()
    {
        if (bottomBorder != null && leftBorder != null)
        {
            currentPileX = leftBorder.anchoredPosition.x;
            currentPileY = bottomBorder.anchoredPosition.y;
        }
        else { currentPileX = 0; currentPileY = 0; }
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

            if (isCartFull) { if (draggable.itemImage != null) draggable.itemImage.color = cartFullColor; }
            else { draggable.SetHoverState(true); }
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
            if (currentlyHoveringItem) currentlyHoveringItem.ResetToDefaultColor();
            currentlyHoveringItem = null;

            // JUICE: Shake the cart to say "NO!"
            transform.DOShakePosition(0.3f, 10f, 20, 90f);
            return;
        }

        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.isDropSuccessful = true;

            // 1. Place the item visually AND get reference
            GameObject droppedVisual = PlaceItemInPile(draggable.itemImage.sprite, draggable.originalColor);

            // JUICE: Animate the new item landing!
            if (droppedVisual != null)
            {
                droppedVisual.transform.localScale = Vector3.zero;
                droppedVisual.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBounce);
            }

            // JUICE: Squash the cart slightly to feel the weight
            transform.DOPunchScale(new Vector3(0.05f, -0.05f, 0), 0.2f, 10, 1);

            // 2. Trigger Logic
            RunItemLogic(draggable);

            // 3. Update Counts
            itemsInCartCount++;

            // 4. Tell Scene1Manager
            if (Scene1Manager.Instance != null)
            {
                Scene1Manager.Instance.OnItemCollected();
            }
        }
        currentlyHoveringItem = null;
    }

    private GameObject PlaceItemInPile(Sprite itemSprite, Color itemColor)
    {
        if (cartContentParent == null || cartItemPrefab == null) return null; // Changed to return null

        GameObject newItemGO = Instantiate(cartItemPrefab, cartContentParent);
        Image newItemImage = newItemGO.GetComponent<Image>();
        RectTransform itemRect = newItemGO.GetComponent<RectTransform>();

        if (newItemImage == null || itemRect == null) { Destroy(newItemGO); return null; }

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

        itemRect.anchoredPosition = new Vector2(baseXPos + xJitter, baseYPos + yJitter);
        itemRect.localRotation = Quaternion.Euler(0, 0, Random.Range(-rotationJitter, rotationJitter));
        currentPileX += itemWidth;

        Vector2 clampedPos = itemRect.anchoredPosition;
        float halfWidth = itemWidth / 2f;
        clampedPos.x = Mathf.Clamp(clampedPos.x, leftBorder.anchoredPosition.x + halfWidth, rightBorder.anchoredPosition.x - halfWidth);
        itemRect.anchoredPosition = clampedPos;

        return newItemGO; // Return the object so we can animate it
    }

    private void RunItemLogic(DraggableItem draggable)
    {
        if (draggable.itemData == null) return;

        // 1. Dialogue
        if (draggable.itemData.sisterReaction != null)
        {
            DialogueManager.Instance.StartDialogue(draggable.itemData.sisterReaction);
        }

        // 2. Logic & Shake
        if (SurvivalMeter.Instance != null)
        {
            switch (draggable.itemData.itemType)
            {
                case FloodItemType.Essential:
                    SurvivalMeter.Instance.HandleEssentialItem(draggable.itemData.impactValue);
                    if (SisterReactionController.Instance != null)
                        SisterReactionController.Instance.TriggerPositiveSound();
                    break;

                case FloodItemType.Burden:
                    SurvivalMeter.Instance.HandleBurdenItem(draggable.itemData.impactValue);
                    if (UIShake.Instance != null) UIShake.Instance.ShakeBurden();
                    if (SisterReactionController.Instance != null)
                        SisterReactionController.Instance.TriggerNegativeSound();
                    break;

                case FloodItemType.Wasteful:
                    SurvivalMeter.Instance.HandleWastefulItem(draggable.itemData.impactValue);
                    if (UIShake.Instance != null) UIShake.Instance.ShakeWasteful();
                    if (SisterReactionController.Instance != null)
                        SisterReactionController.Instance.TriggerNegativeSound();
                    break;
            }
        }
    }
}