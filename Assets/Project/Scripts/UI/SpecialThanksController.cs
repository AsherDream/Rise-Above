using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using Sirenix.OdinInspector;
using DG.Tweening;

public class SpecialThanksController : MonoBehaviour, IPointerClickHandler
{
    [Title("Animation Settings")]
    public float scrollDuration = 8.0f;
    public Ease scrollEase = Ease.OutQuart; // Smooth slow-down at the end

    [Title("References")]
    [Required] public RectTransform creditsContainer;
    [Required] public TextMeshProUGUI continuePromptText; // Drag the text here even if it's inside the container

    private bool canClick = false;
    private float screenHeight;

    private void Awake()
    {
        screenHeight = Screen.height;
    }

    private void OnEnable()
    {
        canClick = false;

        // Hide the prompt initially (Fade out)
        if (continuePromptText != null)
        {
            continuePromptText.canvasRenderer.SetAlpha(0f);
            continuePromptText.gameObject.SetActive(true);
        }

        StartCoroutine(CreditSequence());
    }

    private IEnumerator CreditSequence()
    {
        if (creditsContainer != null)
        {
            // 1. Force Unity to calculate the new height (after Width fix)
            LayoutRebuilder.ForceRebuildLayoutImmediate(creditsContainer);

            // 2. Start Position: Just below the screen
            // We place the Top of the container just below the Bottom of the screen
            float containerHeight = creditsContainer.rect.height;
            float startY = -(screenHeight / 2) - (containerHeight / 2) - 50f; // Extra 50 buffer

            creditsContainer.anchoredPosition = new Vector2(0, startY);

            // 3. End Position: DEAD CENTER (0,0)
            // Since we fixed the size, moving to 0 will center the whole block perfectly.
            float endY = 0f;

            // 4. Animate
            creditsContainer.DOAnchorPosY(endY, scrollDuration)
                .SetEase(scrollEase)
                .SetUpdate(true);
        }

        // Wait for animation to finish
        yield return new WaitForSeconds(scrollDuration);

        // Fade in the text
        canClick = true;
        if (continuePromptText != null)
        {
            continuePromptText.CrossFadeAlpha(1f, 1.0f, true);
            continuePromptText.DOFade(0.5f, 1f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canClick)
        {
            DOTween.KillAll(); // Stop text pulsing

            // 1. Hide the credits panel so it doesn't stay on screen
            if (PanelManager.Instance != null)
            {
                PanelManager.Instance.HideAll();
            }

            // 2. Globally load the Main Menu with the Iris transition
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene("MainMenu");
            }
            else
            {
                // Fallback
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
    }
}