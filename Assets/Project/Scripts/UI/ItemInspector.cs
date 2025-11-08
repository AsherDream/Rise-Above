using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

public class ItemInspector : MonoBehaviour
{
    [Title("Component References")]
    [SerializeField, Required]
    private Image inspectedItemImage;

    [SerializeField, Required]
    private CanvasGroup inspectorCanvasGroup;

    // NEW: Reference for the Title Text
    [SerializeField, Required]
    private TextMeshProUGUI titleText;

    [SerializeField, Required]
    private TextMeshProUGUI descriptionText;

    [Title("Settings")]
    [SerializeField, Range(0.01f, 0.1f), SuffixLabel("seconds per character")]
    private float typewriterSpeed = 0.05f;

    [Title("State (Read-Only)")]
    [ReadOnly]
    public static bool IsInspecting { get; private set; }

    private bool canClose = false;
    private Coroutine typingCoroutine;
    private HashSet<string> seenItems = new HashSet<string>();

    void Awake()
    {
        if (inspectorCanvasGroup == null) inspectorCanvasGroup = GetComponent<CanvasGroup>();
        CloseInspector();
    }

    void Update()
    {
        if (IsInspecting)
        {
            if (!canClose)
            {
                canClose = true;
                return;
            }
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[ItemInspector] Close input detected. Closing inspector.");
                CloseInspector();
            }
        }
    }

    public void InspectItem(InspectableItemData data)
    {
        if (data == null || data.itemSprite == null) return;

        Debug.Log($"[ItemInspector] Inspecting item: {data.itemName}");

        // Set image
        inspectedItemImage.sprite = data.itemSprite;
        inspectedItemImage.preserveAspect = true;

        // Set title
        if (titleText != null)
        {
            titleText.text = data.itemName;
        }

        // Show panel
        inspectorCanvasGroup.alpha = 1;
        inspectorCanvasGroup.interactable = true;
        inspectorCanvasGroup.blocksRaycasts = true;
        IsInspecting = true;
        canClose = false;

        // Handle description text
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        if (seenItems.Contains(data.itemName))
        {
            Debug.Log($"[ItemInspector] Item '{data.itemName}' seen before. Showing full text.");
            descriptionText.text = data.itemDescription;
        }
        else
        {
            Debug.Log($"[ItemInspector] First time seeing '{data.itemName}'. Starting typewriter effect.");
            typingCoroutine = StartCoroutine(TypeText(data.itemDescription));
            seenItems.Add(data.itemName);
        }
    }

    public void CloseInspector()
    {
        inspectorCanvasGroup.alpha = 0;
        inspectorCanvasGroup.interactable = false;
        inspectorCanvasGroup.blocksRaycasts = false;
        IsInspecting = false;
        canClose = false;

        if (inspectedItemImage != null) inspectedItemImage.sprite = null;
        if (titleText != null) titleText.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }

    IEnumerator TypeText(string textToType)
    {
        if (descriptionText == null) yield break;

        descriptionText.text = "";
        foreach (char letter in textToType.ToCharArray())
        {
            if (descriptionText == null) yield break;
            descriptionText.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
}