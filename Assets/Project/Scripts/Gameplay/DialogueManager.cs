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
    public float dialogueDuration = 5f;

    [Title("UI References")]
    [Required] public GameObject dialoguePanel;
    [Required] public Image sisterPortraitImage;
    [Required] public TextMeshProUGUI speakerNameText;
    [Required] public TextMeshProUGUI dialogueText;

    [Title("Mood Configuration")]
    public List<MoodMapping> moodSprites;

    private DialogueNode currentNode;
    private Coroutine autoCloseCoroutine;

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

    // --- RESTORED MISSING METHOD ---
    public void ForcePortrait(SisterMood mood)
    {
        UpdatePortrait(mood);
    }
    // -------------------------------

    public void StartDialogue(DialogueNode startNode, bool isTutorial = false)
    {
        if (startNode == null) return;

        // 1. Stop any previous closing timer so it doesn't disappear randomly
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);

        dialoguePanel.SetActive(true);

        // Force Alpha to 1 in case it was stuck fading
        CanvasGroup group = dialoguePanel.GetComponent<CanvasGroup>();
        if (group != null) { group.alpha = 1f; group.blocksRaycasts = true; }

        DisplayNode(startNode);

        // 2. Only start Auto-Close if it is NOT a tutorial
        if (!isTutorial)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseDialogue(dialogueDuration));
        }
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
        if (moodSprites == null) return;
        MoodMapping mapping = moodSprites.Find(x => x.mood == mood);

        // Ensure we found a mapping and the sprite is valid
        if (mapping.sprite != null)
        {
            sisterPortraitImage.sprite = mapping.sprite;
        }
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNode = null;
        UpdatePortrait(SisterMood.Normal);
    }

    private System.Collections.IEnumerator AutoCloseDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndDialogue();
    }
}