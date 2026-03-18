using UnityEngine;
using Sirenix.OdinInspector;

public class TutorialManager : MonoBehaviour
{
    public static bool IsDragAllowed = false;

    public enum TutorialStep { Intro, Drag, Inspect, Move, Completed }

    [Title("Game State")]
    [ReadOnly] public TutorialStep currentStep = TutorialStep.Intro;

    [Title("Dialogue Nodes")]
    [Required] public DialogueNode introNode;
    [Required] public DialogueNode dragNode;
    [Required] public DialogueNode inspectNode;
    [Required] public DialogueNode moveNode;
    [Required] public DialogueNode completedNode;

    [Title("UI References")]
    public GameObject nextButton;
    public GameObject darkOverlay;

    [Title("Tutorial Visuals")]
    [Required] public GameObject ghostHandObject;
    public TutorialMovementGuide movementGuideScript;
    public GameObject mouseVisual;

    [Title("Game Control")]
    public MonoBehaviour bgScroller;

    // --- NEW REFERENCE ---
    private TutorialHandGuide handGuideScript;

    void Start()
    {
        if (ghostHandObject != null)
        {
            handGuideScript = ghostHandObject.GetComponent<TutorialHandGuide>();
        }

        // --- NEW: CHECK SAVE DATA ---
        // If PlayerPrefs has a "1" saved here, they already beat it.
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            SkipTutorialSilently();
        }
        else
        {
            // First time playing! Start normally.
            IsDragAllowed = false;
            SetStep(TutorialStep.Intro);
        }
    }

    void Update()
    {
        if (ItemInspector.IsInspecting) return;

        if (currentStep == TutorialStep.Move)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                CompleteTutorial();
            }
        }
    }

    public void SetStep(TutorialStep step)
    {
        currentStep = step;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Reset Visuals
        // IMPORTANT: We must disable the hand object first so that when we enable it later,
        // its OnEnable() function runs again with the new mode.
        ghostHandObject.SetActive(false);
        if (mouseVisual) mouseVisual.SetActive(false);
        if (movementGuideScript) movementGuideScript.gameObject.SetActive(false);
        nextButton.SetActive(false);

        if (currentStep != TutorialStep.Completed)
        {
            darkOverlay.SetActive(true);
        }

        switch (currentStep)
        {
            case TutorialStep.Intro:
                if (bgScroller) bgScroller.enabled = false;
                IsDragAllowed = false;
                DialogueManager.Instance.StartDialogue(introNode, true);
                nextButton.SetActive(true);
                break;

            case TutorialStep.Drag:
                DialogueManager.Instance.StartDialogue(dragNode, true);

                // --- SETUP HAND FOR DRAGGING ---
                if (handGuideScript) handGuideScript.currentMode = TutorialHandGuide.HandMode.Dragging;
                ghostHandObject.SetActive(true);

                IsDragAllowed = true;
                break;

            case TutorialStep.Inspect:
                DialogueManager.Instance.StartDialogue(inspectNode, true);
                if (mouseVisual) mouseVisual.SetActive(true);

                // --- SETUP HAND FOR INSPECTING ---
                // We reuse the same hand object, but change the mode
                if (handGuideScript) handGuideScript.currentMode = TutorialHandGuide.HandMode.Inspecting;
                ghostHandObject.SetActive(true);
                break;

            case TutorialStep.Move:
                DialogueManager.Instance.StartDialogue(moveNode, true);
                if (movementGuideScript) movementGuideScript.gameObject.SetActive(true);
                break;

            case TutorialStep.Completed:
                CompleteTutorial();
                break;
        }
    }

    // --- PUBLIC METHODS ---

    public void OnNextButtonClicked()
    {
        if (currentStep == TutorialStep.Intro) SetStep(TutorialStep.Drag);
    }

    public void OnItemDragged()
    {
        if (currentStep == TutorialStep.Drag) SetStep(TutorialStep.Inspect);
    }

    public void OnInspectionOpened()
    {
        if (currentStep == TutorialStep.Inspect) SetStep(TutorialStep.Move);
    }

    private void CompleteTutorial()
    {
        currentStep = TutorialStep.Completed;
        IsDragAllowed = true;

        DialogueManager.Instance.StartDialogue(completedNode, false);

        darkOverlay.SetActive(false);
        ghostHandObject.SetActive(false);
        if (movementGuideScript) movementGuideScript.gameObject.SetActive(false);
        if (mouseVisual) mouseVisual.SetActive(false);

        if (bgScroller) bgScroller.enabled = true;

        // --- NEW: SAVE TO BROWSER ---
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        Debug.Log("Tutorial Finished and Saved!");
    }

    private void SkipTutorialSilently()
    {
        currentStep = TutorialStep.Completed;
        IsDragAllowed = true; // Let them grab items immediately

        // Hide all tutorial UI
        if (darkOverlay) darkOverlay.SetActive(false);
        if (ghostHandObject) ghostHandObject.SetActive(false);
        if (movementGuideScript) movementGuideScript.gameObject.SetActive(false);
        if (mouseVisual) mouseVisual.SetActive(false);
        if (nextButton) nextButton.SetActive(false);

        // Start the game normally
        if (bgScroller) bgScroller.enabled = true;

        Debug.Log("[TutorialManager] Returning player detected. Tutorial skipped.");
    }
}