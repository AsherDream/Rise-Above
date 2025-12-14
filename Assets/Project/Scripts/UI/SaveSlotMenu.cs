using UnityEngine;
using UnityEngine.UI;
using TMPro; // IMPORTANT: If you use TextMeshPro, keep this. If legacy Text, remove.

public class SaveSlotMenu : MonoBehaviour
{
    [Header("Settings")]
    public bool isSaveMode = true; // Check this ON if this menu is for Saving

    [Header("References")]
    // Drag your 4 "Save Slot" GameObjects here
    public Button[] slotButtons;

    // Drag the Text objects that are CHILDREN of the buttons here
    public TextMeshProUGUI[] slotTexts;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            bool isEmpty = SaveManager.Instance.IsSlotEmpty(i);

            if (isEmpty)
            {
                slotTexts[i].text = "Empty Slot";
            }
            else
            {
                slotTexts[i].text = $"Chapter {i + 1}\n<size=60%>Saved Game</size>";
            }
        }
    }

    // LINK THIS FUNCTION TO THE BUTTONS
    public void OnSlotClicked(int slotIndex)
    {
        if (isSaveMode)
        {
            // We are saving: Overwrite or create new
            SaveManager.Instance.SaveGame(slotIndex);
            RefreshUI(); // Update text immediately
        }
        else
        {
            // We are loading: Only load if not empty
            if (!SaveManager.Instance.IsSlotEmpty(slotIndex))
            {
                SaveManager.Instance.LoadGame(slotIndex);
            }
        }
    }
}