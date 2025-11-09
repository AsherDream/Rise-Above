using UnityEngine;
using UnityEngine.EventSystems; // Required for clicking on objects

// Put this script ON your "Bad Item" and "Good Item" GameObjects
public class ClickableItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Item Details")]
    public bool isBadItem = true;
    public string itemName = "BadItem_Apple"; // Must match the name in your .ink file!

    [Header("System References")]
    // Drag your 'GameManagers' object here
    public SurvivalMeter survivalMeter;

    // Drag your 'GameManagers' object here
    public ItemDialogueManager dialogueManager;

    // This function runs when the GameObject is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isBadItem)
        {
            // --- DO BAD ITEM ACTIONS ---
            if (survivalMeter != null)
                survivalMeter.OnBadItemClick();

            if (dialogueManager != null)
                dialogueManager.ShowItemDialogue(itemName);
        }
        else
        {
            // --- DO GOOD ITEM ACTIONS ---
            if (survivalMeter != null)
                survivalMeter.OnGoodItemClick();

            if (dialogueManager != null)
                dialogueManager.ShowItemDialogue(itemName);
        }
    }
}