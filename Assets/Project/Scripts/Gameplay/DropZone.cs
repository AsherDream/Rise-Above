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

    [Title("Toss Animation Settings")]
    public float tossJumpPower = 100f; // How high the arc goes
    public float tossDuration = 0.4f;  // How long the flight takes

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
            transform.DOShakePosition(0.3f, 10f, 20, 90f).SetUpdate(true);
            return;
        }

        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.isDropSuccessful = true;

            // --- UPDATE START: Capture the drop position ---
            // We need to know where the item was in world space right when let go
            Vector3 startDropPos = draggable.transform.position;

            // 1. Place the item visually (triggering the toss animation)
            GameObject droppedVisual = PlaceItemInPile(draggable.itemImage.sprite, draggable.originalColor, startDropPos);
            // --- UPDATE END ---

            // JUICE: Squash the cart slightly to feel the weight of it landing
            // Delayed slightly to sync with the item landing in the cart
            transform.DOPunchScale(new Vector3(0.05f, -0.05f, 0), 0.2f, 10, 1).SetDelay(tossDuration * 0.8f).SetUpdate(true);

            // 2. Trigger Logic (Dialogue + Survival Meter + MESS)
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

    // --- UPDATED METHOD SIGNATURE AND LOGIC ---
    private GameObject PlaceItemInPile(Sprite itemSprite, Color itemColor, Vector3 startWorldPos)
    {
        if (cartContentParent == null || cartItemPrefab == null) return null;

        // 1. Create the new item
        GameObject newItemGO = Instantiate(cartItemPrefab, cartContentParent);
        Image newItemImage = newItemGO.GetComponent<Image>();
        RectTransform itemRect = newItemGO.GetComponent<RectTransform>();

        if (newItemImage == null || itemRect == null) { Destroy(newItemGO); return null; }

        newItemImage.sprite = itemSprite;
        newItemImage.color = itemColor;
        // Ensure it starts full scale
        itemRect.localScale = Vector3.one;

        // 2. Calculate the Target Position (where it belongs in the pile)
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

        Vector2 targetAnchoredPos = new Vector2(baseXPos + xJitter, baseYPos + yJitter);

        // Clamp to borders
        float halfWidth = itemWidth / 2f;
        targetAnchoredPos.x = Mathf.Clamp(targetAnchoredPos.x, leftBorder.anchoredPosition.x + halfWidth, rightBorder.anchoredPosition.x - halfWidth);

        // UPDATE CURSOR for next item
        currentPileX += itemWidth;

        // --- TOSS ANIMATION LOGIC ---

        // 3. Set Initial Position: Convert the world drop point to local space relative to the cart container
        Vector2 localStartPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(cartContentParent, Input.mousePosition, null, out localStartPos);
        itemRect.anchoredPosition = localStartPos;

        // 4. Animate: Jump from mouse position to target pile position
        Sequence tossSequence = DOTween.Sequence();

        // The Arc (Jump)
        tossSequence.Append(itemRect.DOJumpAnchorPos(targetAnchoredPos, tossJumpPower, 1, tossDuration).SetEase(Ease.OutQuad));

        // The Spin (Rotate while flying)
        float randomRotation = Random.Range(-rotationJitter, rotationJitter);
        tossSequence.Join(itemRect.DORotate(new Vector3(0, 0, randomRotation), tossDuration, RotateMode.FastBeyond360));

        // Ensure it plays even if paused (optional, depending on your design preference)
        tossSequence.SetUpdate(true);

        return newItemGO;
    }

    private void RunItemLogic(DraggableItem draggable)
    {
        if (draggable.itemData == null) return;

        // 1. Dialogue
        if (draggable.itemData.sisterReaction != null)
        {
            DialogueManager.Instance.StartDialogue(draggable.itemData.sisterReaction);
        }

        // 2. Logic & Shake & Mess
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

                    if (LightFlickerController.Instance != null)
                        LightFlickerController.Instance.OnGridDamaged(0.1f);

                    // --- Trigger Mess with Type ---
                    if (CleanupManager.Instance != null)
                    {
                        string itemName = draggable.itemData.itemName.ToLower();
                        if (itemName.Contains("milk") || itemName.Contains("egg"))
                        {
                            // Pass the name ("milk" or "egg") to choose texture
                            CleanupManager.Instance.TriggerMess(itemName);
                        }
                    }
                    break;
            }
        }
    }
}