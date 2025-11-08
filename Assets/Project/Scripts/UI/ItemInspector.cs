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
    // HashSet to remember which items we've already seen
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
                CloseInspector();
            }
        }
    }

    public void InspectItem(InspectableItemData data)
    {
        if (data == null || data.itemSprite == null) return;

        inspectedItemImage.sprite = data.itemSprite;
        inspectedItemImage.preserveAspect = true;

        inspectorCanvasGroup.alpha = 1;
        inspectorCanvasGroup.interactable = true;
        inspectorCanvasGroup.blocksRaycasts = true;
        IsInspecting = true;
        canClose = false;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        // Check if we've seen this item before
        if (seenItems.Contains(data.itemName))
        {
            descriptionText.text = data.itemDescription;
        }
        else
        {
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
        if (descriptionText != null) descriptionText.text = "";
    }

    IEnumerator TypeText(string textToType)
    {
        descriptionText.text = "";
        foreach (char letter in textToType.ToCharArray())
        {
            descriptionText.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
}