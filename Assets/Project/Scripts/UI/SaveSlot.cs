using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    public int slotNumber = 1;

    [Header("UI References")]
    public TextMeshProUGUI slotText;
    public Button deleteButton;

    private SaveManager saveManager;

    private void Start()
    {
        saveManager = FindFirstObjectByType<SaveManager>();
        RefreshSlotUI();
    }

    public void RefreshSlotUI()
    {
        if (saveManager == null) return;

        bool isEmpty = saveManager.IsSlotEmpty(slotNumber);

        if (isEmpty)
        {
            slotText.text = "Empty Slot";
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);
        }
        else
        {
            slotText.text = "Save File " + slotNumber;
            if (deleteButton != null) deleteButton.gameObject.SetActive(true);
        }
    }

    public void OnClickSlot()
    {
        if (saveManager != null)
        {
            saveManager.SaveGame(slotNumber);
            RefreshSlotUI();
        }
    }

    // --- THIS IS THE PART YOU WERE MISSING ---
    public void OnClickDelete()
    {
        if (saveManager != null)
        {
            saveManager.DeleteSaveFile(slotNumber);
            RefreshSlotUI();
        }
    }
}