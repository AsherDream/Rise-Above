using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector; // UPDATED: Added Odin

public class ItemInspector : MonoBehaviour
{
    [Title("Component References")] // UPDATED
    [SerializeField, Required] // UPDATED: Added Odin attributes
    private Image inspectedItemImage;

    [SerializeField, Required] // UPDATED
    private CanvasGroup inspectorCanvasGroup;

    [Title("State (Read-Only)")] // UPDATED
    [ReadOnly] // UPDATED
    public static bool IsInspecting { get; private set; }

    // UPDATED: This boolean prevents the inspector from closing on the same frame it opens
    private bool canClose = false;

    void Awake()
    {
        if (inspectorCanvasGroup == null)
            inspectorCanvasGroup = GetComponent<CanvasGroup>();

        if (inspectedItemImage == null)
            Debug.LogError("[ItemInspector] Inspected Item Image is not assigned!", this);

        // Ensure it's hidden at the start
        CloseInspector();
    }

    void Update()
    {
        // Check for 'close' input (Right-click or Escape)
        if (IsInspecting)
        {
            // UPDATED: This logic delays the check by one frame
            if (!canClose)
            {
                // This is the first frame the inspector is open.
                // We set canClose to true, but 'return' so we don't check for input.
                canClose = true;
                return;
            }

            // On the second frame and after, canClose is true, so this code runs
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[ItemInspector] Close input detected. Closing inspector.");
                CloseInspector();
            }
        }
    }

    // This is called by the item when it's right-clicked
    public void InspectItem(Sprite itemSprite)
    {
        if (itemSprite == null)
        {
            Debug.LogWarning("[ItemInspector] InspectItem called with a null sprite.", this);
            return;
        }

        Debug.Log($"[ItemInspector] Inspecting item: {itemSprite.name}", this);

        // Set the sprite
        inspectedItemImage.sprite = itemSprite;
        inspectedItemImage.preserveAspect = true;

        // Show the panel
        inspectorCanvasGroup.alpha = 1;
        inspectorCanvasGroup.interactable = true;
        inspectorCanvasGroup.blocksRaycasts = true;

        IsInspecting = true;

        // UPDATED: We just opened, so we set canClose to false.
        // This will trigger the one-frame delay in Update().
        canClose = false;
    }

    // This is called to hide the panel
    public void CloseInspector()
    {
        // Hide the panel
        inspectorCanvasGroup.alpha = 0;
        inspectorCanvasGroup.interactable = false;
        inspectorCanvasGroup.blocksRaycasts = false;

        IsInspecting = false;
        canClose = false; // Reset the flag

        if (inspectedItemImage != null)
            inspectedItemImage.sprite = null;
    }
}

