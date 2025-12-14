using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Title("Settings")]
    [InfoBox("How long the dialogue stays on screen before auto-closing.")]
    [Range(1f, 10f)]
    public float dialogueDuration = 5f; // Default extended to 5 seconds

    [Title("UI References")]
    [Required] public GameObject dialoguePanel;
    [Required] public Image sisterPortraitImage;
    [Required] public TextMeshProUGUI speakerNameText;
    [Required] public TextMeshProUGUI dialogueText;

    // REMOVED: ChoicesContainer and ChoiceButtonPrefab

    [Title("Mood Configuration")]
    [InfoBox("Assign the sprite for each mood here")]
    public List<MoodMapping> moodSprites;

    private DialogueNode currentNode;
    private Coroutine autoCloseCoroutine; // To close dialogue automatically

    [System.Serializable]
    public struct MoodMapping
    {
        public SisterMood mood;
        public Sprite sprite;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void ForcePortrait(SisterMood mood)
    {
        UpdatePortrait(mood);
    }

    public void StartDialogue(DialogueNode startNode, float duration = -1f)
    {
        if (startNode == null) return;

        // Use the inspector value if no specific duration is passed (passed as -1)
        float activeDuration = duration > 0 ? duration : dialogueDuration;

        // Stop any pending close timer
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);

        dialoguePanel.SetActive(true);
        DisplayNode(startNode);

        // Automatically close after the set duration
        autoCloseCoroutine = StartCoroutine(AutoCloseDialogue(activeDuration));
    }

    private void DisplayNode(DialogueNode node)
    {
        currentNode = node;
        speakerNameText.text = node.speakerName;
        dialogueText.text = node.dialogueText;

        UpdatePortrait(node.mood);
    }

    private void UpdatePortrait(SisterMood mood)
    {
        MoodMapping mapping = moodSprites.Find(x => x.mood == mood);
        if (mapping.sprite != null)
        {
            sisterPortraitImage.sprite = mapping.sprite;
        }
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNode = null;

        // --- NEW: Reset Sister to Normal Mood ---
        UpdatePortrait(SisterMood.Normal);
    }

    private System.Collections.IEnumerator AutoCloseDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndDialogue();
    }
}