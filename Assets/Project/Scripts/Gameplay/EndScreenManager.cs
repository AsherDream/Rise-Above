using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening; // REQUIRED for Juice

public class EndScreenManager : MonoBehaviour
{
    [Title("References")]
    [Required] public GameObject itemSlotPrefab;

    [Title("Top Section")]
    [Required] public TextMeshProUGUI titleText;

    [Title("Left Panel (Stats)")]
    [Required] public TextMeshProUGUI statsText;
    [Required] public CanvasGroup statsCanvasGroup; // Add a CanvasGroup to 'Stats' object for fading

    [Title("Right Panel (Grid)")]
    [Required] public RectTransform gridContentParent;

    [Title("Bottom Panel (Details)")]
    [Required] public GameObject detailPanel;
    [Required] public Image largeItemImage;
    [Required] public Image sisterFaceImage;

    [Title("Detail Text Elements")]
    [Required] public TextMeshProUGUI itemNameText;
    [Required] public TextMeshProUGUI reasonText;

    [Title("Navigation")]
    [Required] public GameObject mainMenuButton; // <--- The Missing Button Reference

    [Title("Defaults")]
    public Sprite defaultSisterSprite;
    public string defaultReason = "Select an item to see details.";

    private bool isDetailViewOpen = false;

    private void Awake()
    {
        // Safety: Hide things instantly on boot
        if (detailPanel != null) detailPanel.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.SetActive(false);
    }

    private void Update()
    {
        if (isDetailViewOpen)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                ClearDetailView();
            }
        }
    }

    private void OnEnable()
    {
        ClearDetailView();

        // Hide button initially so it can pop in later
        if (mainMenuButton != null)
        {
            mainMenuButton.transform.localScale = Vector3.zero;
            mainMenuButton.SetActive(true);
        }

        if (Scene1Manager.Instance == null) return;

        GenerateStats();
        GenerateGrid();
    }

    private void GenerateStats()
    {
        List<InspectableItemData> items = Scene1Manager.Instance.collectedItemsList;

        int totalItems = items.Count;
        int essentials = 0;
        int burdens = 0;
        int wasteful = 0;

        foreach (var item in items)
        {
            if (item.itemType == FloodItemType.Essential) essentials++;
            else if (item.itemType == FloodItemType.Burden) burdens++;
            else wasteful++;
        }

        int hp = 0;
        if (SurvivalMeter.Instance != null) hp = SurvivalMeter.Instance.currentHP;
        int days = (hp / 20) + 1;

        string stats = $"<b>SURVIVAL RATE:</b>\n<size=150%>{hp}%</size>\n\n";
        stats += $"<b>EST. SURVIVAL:</b>\n{days} Days\n\n";
        stats += $"<b>PACKING STATS:</b>\n";
        stats += $"Total Items: {totalItems}\n";
        stats += $"<color=green>Essential: {essentials}</color>\n";
        stats += $"<color=yellow>Burdens: {burdens}</color>\n";
        stats += $"<color=red>Wasteful: {wasteful}</color>";

        if (statsText != null) statsText.text = stats;

        // --- ANIMATION: Slide Stats In ---
        if (statsCanvasGroup != null)
        {
            statsCanvasGroup.alpha = 0f;
            statsText.transform.localPosition -= Vector3.right * 50f; // Start slightly left

            Sequence statSeq = DOTween.Sequence();
            statSeq.Append(statsCanvasGroup.DOFade(1, 0.5f));
            statSeq.Join(statsText.transform.DOLocalMoveX(statsText.transform.localPosition.x + 50f, 0.5f).SetEase(Ease.OutQuad));
        }
    }

    private void GenerateGrid()
    {
        foreach (Transform child in gridContentParent) Destroy(child.gameObject);

        List<InspectableItemData> items = Scene1Manager.Instance.collectedItemsList;

        // Force Rebuild FIRST so positions are calculated
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridContentParent);

        float delay = 0.2f; // Initial delay before items start popping

        for (int i = 0; i < items.Count; i++)
        {
            InspectableItemData item = items[i];

            GameObject newSlot = Instantiate(itemSlotPrefab, gridContentParent, false);

            // Hide initially for animation
            newSlot.transform.localScale = Vector3.zero;

            Image icon = newSlot.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = item.itemSprite;

            Image border = newSlot.GetComponent<Image>();
            if (border != null)
            {
                if (item.itemType == FloodItemType.Essential) border.color = Color.green;
                else if (item.itemType == FloodItemType.Burden) border.color = Color.yellow;
                else border.color = Color.red;
            }

            EndScreenSlot slotScript = newSlot.AddComponent<EndScreenSlot>();
            slotScript.Setup(item, this);

            // --- ANIMATION: Pop in Item ---
            newSlot.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(delay + (i * 0.1f));
        }

        // --- ANIMATION: Pop in Main Menu Button at the very end ---
        if (mainMenuButton != null)
        {
            float totalAnimationTime = delay + (items.Count * 0.1f) + 0.2f;
            mainMenuButton.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(totalAnimationTime);
        }
    }

    public void ShowItemDetails(InspectableItemData item)
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(true);

            // --- ANIMATION: Bounce Open ---
            detailPanel.transform.localScale = Vector3.one * 0.8f;
            detailPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        isDetailViewOpen = true;

        if (largeItemImage != null)
        {
            largeItemImage.sprite = item.itemSprite;
            largeItemImage.gameObject.SetActive(true);
        }

        if (itemNameText != null) itemNameText.text = item.itemName;
        if (reasonText != null) reasonText.text = item.educationalTip;

        if (sisterFaceImage != null)
        {
            sisterFaceImage.gameObject.SetActive(true);
            if (item.sisterFeedbackSprite != null)
                sisterFaceImage.sprite = item.sisterFeedbackSprite;
            else
                sisterFaceImage.sprite = defaultSisterSprite;
        }
    }

    public void ClearDetailView()
    {
        if (detailPanel != null) detailPanel.SetActive(false);
        isDetailViewOpen = false;

        if (largeItemImage != null) largeItemImage.gameObject.SetActive(false);
        if (itemNameText != null) itemNameText.text = "";
        if (reasonText != null) reasonText.text = "";
        if (sisterFaceImage != null) sisterFaceImage.sprite = defaultSisterSprite;
    }
}