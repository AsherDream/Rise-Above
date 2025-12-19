using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

public class GameOverEffects : MonoBehaviour
{
    [Title("UI Elements")]
    [Required] public TextMeshProUGUI titleText;
    [Required] public TextMeshProUGUI reasonText;
    [Required] public CanvasGroup buttonsGroup;

    [Title("Settings")]
    public float animationSpeed = 0.5f;
    public float buttonSlideDistance = 50f; // Made this a variable so it's easier to tune

    private Vector3 _initialButtonPos;

    private void Awake()
    {
        // Store the correct location of the buttons where they should end up.
        if (buttonsGroup != null)
        {
            _initialButtonPos = buttonsGroup.transform.localPosition;
        }
    }

    private void OnEnable()
    {
        // 0. Safety Check: Kill any running tweens on these objects if re-opened quickly
        // This prevents conflicts if the menu is toggled rapidly.
        titleText.transform.DOKill();
        reasonText.DOKill();
        buttonsGroup.DOKill();
        buttonsGroup.transform.DOKill();

        // 1. Reset State (Hide everything)
        titleText.transform.localScale = Vector3.zero;
        reasonText.alpha = 0f;
        buttonsGroup.alpha = 0f;
        buttonsGroup.interactable = false;

        // Snap buttons to the starting position (below their final resting place)
        buttonsGroup.transform.localPosition = _initialButtonPos - (Vector3.up * buttonSlideDistance);


        // 2. Animate Sequence
        // --- FIX #1: SetUpdate(true) allows this to run even if Time.timeScale is 0 ---
        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        // Step A: Title Slams In
        sequence.Append(titleText.transform.DOScale(1f, animationSpeed).SetEase(Ease.OutBack));

        // Step B: Reason Fades In
        sequence.Append(reasonText.DOFade(1f, 0.5f));

        // Step C: Buttons Fade In & Slide Up
        sequence.Append(buttonsGroup.DOFade(1f, 0.5f));
        // --- FIX #2: Animate to the stored initial position ---
        sequence.Join(buttonsGroup.transform.DOLocalMoveY(_initialButtonPos.y, 0.5f).SetEase(Ease.OutQuad));

        // Step D: Enable clicking
        sequence.OnComplete(() => buttonsGroup.interactable = true);
    }
}