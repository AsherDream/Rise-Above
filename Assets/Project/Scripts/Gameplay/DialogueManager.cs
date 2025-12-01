using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Title("UI References")]
    [Required] public GameObject dialoguePanel;
    [Required] public TextMeshProUGUI speakerNameText;
    [Required] public TextMeshProUGUI dialogueText;
    [Required] public Transform choicesContainer;
    [Required] public GameObject choiceButtonPrefab;

    private DialogueNode currentNode;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueNode startNode)
    {
        if (startNode == null) return;
        dialoguePanel.SetActive(true);
        DisplayNode(startNode);
    }

    private void DisplayNode(DialogueNode node)
    {
        currentNode = node;
        speakerNameText.text = node.speakerName;
        dialogueText.text = node.dialogueText;

        // Clear old choices
        foreach (Transform child in choicesContainer) Destroy(child.gameObject);

        // Create new choices
        if (node.choices.Count > 0)
        {
            foreach (DialogueChoice choice in node.choices)
            {
                CreateChoiceButton(choice.choiceText, choice.nextNode);
            }
        }
        else if (node.nextNode != null)
        {
            CreateChoiceButton("Continue...", node.nextNode);
        }
        else if (node.isEndNode)
        {
            CreateChoiceButton("End", null);
        }
    }

    private void CreateChoiceButton(string text, DialogueNode nextNode)
    {
        GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
        buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = text;

        Button button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => OnChoiceSelected(nextNode));
    }

    private void OnChoiceSelected(DialogueNode nextNode)
    {
        if (nextNode != null) DisplayNode(nextNode);
        else EndDialogue();
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNode = null;
        Debug.Log("Dialogue Ended");
    }
}