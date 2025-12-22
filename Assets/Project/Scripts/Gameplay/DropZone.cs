using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;

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
    public float tossJumpPower = 100f;
    public float tossDuration = 0.4f;

    [Title("Pacing Constraint")]
    [InfoBox("Time in seconds player must wait between drops.")]
    public float minDropInterval = 0.8f;

    [Title("Audio")]
    public AudioSource dropAudioSource;
    public AudioClip dropSound;

    private float lastDropTime = -999f;
    private DraggableItem currentlyHoveringItem = null;
    private int itemsInCartCount = 0;
    private float currentPileX;
    private float currentPileY;

    private void Awake()
    {
        InitializePileCursor();
        if (dropAudioSource == null) dropAudioSource = GetComponent<AudioSource>();
    }

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
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();

        // 1. Initial Checks (Cart Full, Spamming, etc.)
        bool isCartFull = itemsInCartCount >= maxCartCapacity;
        if (Scene1Manager.Instance != null && Scene1Manager.Instance.IsCartFull()) isCartFull = true;
        bool isSpamming = (Time.time - lastDropTime < minDropInterval);

        // --- NEW: CHECK FOR DUPLICATES ---
        bool isDuplicate = false;
        if (Scene1Manager.Instance != null && draggable != null && draggable.itemData != null)
        {
            if (Scene1Manager.Instance.IsItemAlreadyInCart(draggable.itemData))
            {
                isDuplicate = true;
            }
        }
        // ---------------------------------

        // If ANY block condition is met, reject the drop
        if (isCartFull || isSpamming || isDuplicate)
        {
            if (draggable) draggable.ResetToDefaultColor();
            currentlyHoveringItem = null;

            // Visual Shake "No!"
            transform.DOShakePosition(0.3f, 10f, 20, 90f).SetUpdate(true);

            // Handle Specific Rejections
            if (isDuplicate)
            {
                // Trigger Sister's Duplicate Complaint
                if (SisterReactionController.Instance != null)
                    SisterReactionController.Instance.TriggerDuplicateReaction();
            }
            else if (isSpamming && SisterReactionController.Instance != null)
            {
                SisterReactionController.Instance.TriggerAlert();
            }
            return;
        }

        // --- SUCCESSFUL DROP ---
        if (draggable != null)
        {
            lastDropTime = Time.time;
            draggable.isDropSuccessful = true;

            if (dropAudioSource != null && dropSound != null)
            {
                dropAudioSource.pitch = Random.Range(0.9f, 1.1f);
                dropAudioSource.PlayOneShot(dropSound);
            }

            Vector3 startDropPos = draggable.transform.position;
            PlaceItemInPile(draggable.itemImage.sprite, draggable.originalColor, startDropPos);

            transform.DOPunchScale(new Vector3(0.05f, -0.05f, 0), 0.2f, 10, 1).SetDelay(tossDuration * 0.8f).SetUpdate(true);

            RunItemLogic(draggable);

            itemsInCartCount++;

            if (Scene1Manager.Instance != null)
            {
                Scene1Manager.Instance.OnItemCollected(draggable.itemData);
            }
        }
        currentlyHoveringItem = null;
    }

    private GameObject PlaceItemInPile(Sprite itemSprite, Color itemColor, Vector3 startWorldPos)
    {
        if (cartContentParent == null || cartItemPrefab == null) return null;

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

        Vector2 targetAnchoredPos = new Vector2(baseXPos + xJitter, baseYPos + yJitter);

        float halfWidth = itemWidth / 2f;
        targetAnchoredPos.x = Mathf.Clamp(targetAnchoredPos.x, leftBorder.anchoredPosition.x + halfWidth, rightBorder.anchoredPosition.x - halfWidth);

        currentPileX += itemWidth;

        Vector2 localStartPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(cartContentParent, Input.mousePosition, null, out localStartPos);
        itemRect.anchoredPosition = localStartPos;

        Sequence tossSequence = DOTween.Sequence();
        tossSequence.Append(itemRect.DOJumpAnchorPos(targetAnchoredPos, tossJumpPower, 1, tossDuration).SetEase(Ease.OutQuad));

        float randomRotation = Random.Range(-rotationJitter, rotationJitter);
        tossSequence.Join(itemRect.DORotate(new Vector3(0, 0, randomRotation), tossDuration, RotateMode.FastBeyond360));

        tossSequence.SetUpdate(true);

        return newItemGO;
    }

    private void RunItemLogic(DraggableItem draggable)
    {
        if (draggable.itemData == null) return;

        // 1. Trigger Sister Dialogue
        if (draggable.itemData.sisterReaction != null)
        {
            DialogueManager.Instance.StartDialogue(draggable.itemData.sisterReaction);
        }

        // 2. Handle Stats (HP/Morale/Shakes)
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
                    break;
            }
        }

        // 3. Handle Mess (MOVED OUTSIDE SWITCH)
        // Now, ANY item (Burden or Wasteful) will trigger the minigame if it has a texture assigned.
        if (CleanupManager.Instance != null)
        {
            // The Manager already checks if messTexture is null, so this is safe to call for every item.
            CleanupManager.Instance.TriggerMess(draggable.itemData);
        }
    }
}