using UnityEngine;
using Ink.Runtime;
using TMPro;

public class ItemDialogueManager : MonoBehaviour
{
    [Header("Ink Story Asset (compiled .json)")]
    public TextAsset inkJSONAsset; // drag item_dialogue.ink.json here

    [Header("UI References")]
    public GameObject dialoguePanel; // panel to show/hide
    public TMP_Text dialogueText;    // TextMeshPro UI text
    public GameObject continueButton; // optional close/continue button

    private Story story;

    void Awake()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (inkJSONAsset != null) story = new Story(inkJSONAsset.text);
    }

    /// <summary>
    /// Public method: show dialogue for a given itemName (call from other scripts)
    /// </summary>
    public void ShowItemDialogue(string itemName)
    {
        if (inkJSONAsset == null)
        {
            Debug.LogWarning("ItemDialogueManager: inkJSONAsset not assigned.");
            return;
        }

        // Reset story each time to avoid leftover state
        story = new Story(inkJSONAsset.text);

        // Set Ink variable (not strictly necessary if we pass to function, but good to keep)
        story.variablesState["item"] = itemName;

        // Evaluate the Ink function that returns the string
        var resultObj = story.EvaluateFunction("GetItemDialogue", itemName);

        string resultText = (resultObj != null) ? resultObj.ToString() : "…";

        // Show on UI
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueText != null) dialogueText.text = resultText;

        // Optionally hide continue button if null
        if (continueButton != null) continueButton.SetActive(true);
    }

    // Hook this to the Close/Continue UI button
    public void CloseDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }
}
