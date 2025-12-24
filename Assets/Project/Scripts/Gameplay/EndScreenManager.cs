using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class EndScreenManager : MonoBehaviour
{
    [Title("References")]
    [Required] public GameObject itemSlotPrefab;

    [Title("Top Section")]
    [Required] public TextMeshProUGUI titleText;

    [Title("Left Panel (Stats)")]
    [Required] public TextMeshProUGUI statsText;
    [Required] public CanvasGroup statsCanvasGroup;

    [Title("Rank Stamps")] // <--- NEW SECTION
    [Required] public Image rankStampImage;
    public Sprite rankASprite;
    public Sprite rankBSprite;
    public Sprite rankCSprite;

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
    [Required] public GameObject mainMenuButton;

    [Title("Audio Juice")]
    public AudioSource sfxSource;
    public AudioClip popSound;
    public AudioClip paperOpenSound;
    public AudioClip victoryChime;
    public AudioClip stampThudSound; // <--- NEW SOUND SLOT (Optional)

    [Title("Defaults")]
    public Sprite defaultSisterSprite;

    private bool isDetailViewOpen = false;

    private void Awake()
    {
        if (detailPanel != null) detailPanel.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.SetActive(false);
        if (rankStampImage != null) rankStampImage.gameObject.SetActive(false); // Hide stamp initially
    }

    private void Start()
    {
        ClearDetailView();

        if (mainMenuButton != null)
        {
            Button btn = mainMenuButton.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnMainMenuClicked);
            mainMenuButton.transform.localScale = Vector3.zero;
        }

        if (Scene1Manager.Instance == null) return;

        if (sfxSource && victoryChime) sfxSource.PlayOneShot(victoryChime);

        if (PanelManager.Instance != null)
            PanelManager.Instance.enabled = false;

        GenerateStats(); // This now triggers the stamp animation too!
        GenerateGrid();
    }

    private void OnDestroy()
    {
        if (PanelManager.Instance != null)
            PanelManager.Instance.enabled = true;
    }

    private void GenerateStats()
    {
        List<InspectableItemData> items = Scene1Manager.Instance.collectedItemsList;

        // --- MATH LOGIC ---
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

        int hp = (SurvivalMeter.Instance != null) ? SurvivalMeter.Instance.currentHP : 100;
        int days = (hp / 20) + 1;

        // --- TEXT FORMATTING ---
        string stats = $"<b>SURVIVAL PROBABILITY:</b>\n<size=150%>{hp}%</size>\n\n";
        stats += $"<b>EST. SURVIVAL:</b>\n{days} Days\n\n";
        stats += $"<b>PACKING REPORT:</b>\n";
        stats += $"Items Packed: {totalItems}\n";
        stats += $"<color=green>Essential: {essentials}</color>\n";
        stats += $"<color=yellow>Burden: {burdens}</color>\n";
        stats += $"<color=red>Useless: {wasteful}</color>";

        if (statsText != null) statsText.text = stats;

        // --- ANIMATION: SLIDE TEXT ---
        if (statsCanvasGroup != null)
        {
            statsCanvasGroup.alpha = 0f;
            statsText.transform.localPosition -= Vector3.right * 50f;

            Sequence statSeq = DOTween.Sequence();
            statSeq.Append(statsCanvasGroup.DOFade(1, 0.5f));
            statSeq.Join(statsText.transform.DOLocalMoveX(statsText.transform.localPosition.x + 50f, 0.5f).SetEase(Ease.OutQuad));
        }

        // --- NEW: TRIGGER RANK STAMP ---
        ShowRankStamp(hp);
    }

    private void ShowRankStamp(int score)
    {
        if (rankStampImage == null) return;

        rankStampImage.gameObject.SetActive(true);

        // 1. Determine Rank
        Sprite selectedStamp = rankCSprite; // Default to C
        if (score >= 90) selectedStamp = rankASprite;
        else if (score >= 70) selectedStamp = rankBSprite;

        rankStampImage.sprite = selectedStamp;

        // 2. Setup "Slam" Animation State
        rankStampImage.color = new Color(1, 1, 1, 0); // Invisible
        rankStampImage.transform.localScale = Vector3.one * 3f; // Big (Zoomed in)

        // 3. The Animation Sequence
        Sequence stampSeq = DOTween.Sequence();

        // Wait 1.5 seconds (let the grid populate first)
        stampSeq.AppendInterval(1.5f);

        // Fade in instantly
        stampSeq.Append(rankStampImage.DOFade(1, 0.05f));

        // SLAM down (Scale from 3 -> 1 quickly with a bounce)
        stampSeq.Join(rankStampImage.transform.DOScale(1f, 0.25f).SetEase(Ease.InBack)); // InBack feels like a heavy impact

        // Shake screen and play sound when it hits
        stampSeq.AppendCallback(() => {
            if (sfxSource && stampThudSound) sfxSource.PlayOneShot(stampThudSound);

            // Optional: Slight camera shake or UI shake if you want extra impact
            rankStampImage.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.3f);
        });
    }

    private void GenerateGrid()
    {
        // ... (Keep your existing Grid code exactly as it was) ...
        // Ensure you paste the grid generation logic back here!
        foreach (Transform child in gridContentParent) Destroy(child.gameObject);
        List<InspectableItemData> items = Scene1Manager.Instance.collectedItemsList;
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridContentParent);
        float delay = 0.3f;
        for (int i = 0; i < items.Count; i++)
        {
            InspectableItemData item = items[i];
            GameObject newSlot = Instantiate(itemSlotPrefab, gridContentParent, false);
            newSlot.transform.localScale = Vector3.zero;
            Image icon = newSlot.transform.Find("Icon").GetComponent<Image>();
            icon.sprite = item.itemSprite;
            Image border = newSlot.GetComponent<Image>();
            if (border != null)
            {
                if (item.itemType == FloodItemType.Essential) border.color = new Color(0f, 1f, 0f, 0.5f);
                else if (item.itemType == FloodItemType.Burden) border.color = new Color(1f, 0.92f, 0.016f, 0.5f);
                else border.color = new Color(1f, 0f, 0f, 0.5f);
            }
            EndScreenSlot slotScript = newSlot.AddComponent<EndScreenSlot>();
            slotScript.Setup(item, this);
            int idx = i;
            float popDelay = delay + (idx * 0.1f);
            newSlot.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(popDelay)
                .OnStart(() => {
                    if (sfxSource && popSound)
                    {
                        sfxSource.pitch = 0.8f + (idx * 0.05f);
                        sfxSource.PlayOneShot(popSound);
                    }
                });
        }
        if (mainMenuButton != null)
        {
            float totalTime = delay + (items.Count * 0.1f) + 1.0f; // Delayed after stamp
            mainMenuButton.SetActive(true);
            mainMenuButton.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(totalTime);
        }
    }

    // ... (Keep ShowItemDetails, ClearDetailView, OnMainMenuClicked) ...
    public void ShowItemDetails(InspectableItemData item)
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
            if (sfxSource) { sfxSource.pitch = 1.0f; if (paperOpenSound) sfxSource.PlayOneShot(paperOpenSound); }
            detailPanel.transform.localScale = Vector3.one * 0.8f;
            detailPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
        isDetailViewOpen = true;
        if (largeItemImage != null) { largeItemImage.sprite = item.itemSprite; largeItemImage.gameObject.SetActive(true); }
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (reasonText != null) reasonText.text = item.educationalTip;
        if (sisterFaceImage != null) { sisterFaceImage.gameObject.SetActive(true); sisterFaceImage.sprite = (item.sisterFeedbackSprite != null) ? item.sisterFeedbackSprite : defaultSisterSprite; }
    }
    public void ClearDetailView() { if (detailPanel != null) detailPanel.SetActive(false); isDetailViewOpen = false; }
    public void OnMainMenuClicked() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
}