using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
using System.Collections.Generic;

public class UILayoutController : MonoBehaviour
{
    [Title("Target Tracking")]
    [Required] public RectTransform backgroundRect;
    public float swapThresholdX = -1200f;

    [Title("UI Elements")]
    [Required] public RectTransform sisterPanel;
    [Required] public RectTransform timerPanel;

    [Title("Text Correction")]
    public List<RectTransform> textObjectsToUnflip = new List<RectTransform>();

    // --- MANUAL COORDINATES SECTION ---
    [Title("Manual Position Settings")]
    [InfoBox("Set where the UI elements should move to when swapped.")]

    [LabelText("Sister Target Pos (Left)")]
    public Vector2 sisterSwappedPos; // Where Sister goes when swapped

    [LabelText("Timer Target Pos (Right)")]
    public Vector2 timerSwappedPos;  // Where Timer goes when swapped

    // Internal state to remember original positions
    private Vector2 sisterDefaultPos;
    private Vector2 timerDefaultPos;
    private bool isSwapped = false;

    private void Start()
    {
        // 1. Remember where they started automatically
        if (sisterPanel != null) sisterDefaultPos = sisterPanel.anchoredPosition;
        if (timerPanel != null) timerDefaultPos = timerPanel.anchoredPosition;
    }

    private void Update()
    {
        if (backgroundRect == null) return;

        bool atFarRight = backgroundRect.anchoredPosition.x < swapThresholdX;

        if (atFarRight && !isSwapped)
        {
            SwapToLeft();
        }
        else if (!atFarRight && isSwapped)
        {
            SwapToRight();
        }
    }

    [Button("1. Trigger Swap (Move to Manual Pos)")]
    private void SwapToLeft()
    {
        isSwapped = true;

        // Move to the MANUALLY defined coordinates
        sisterPanel.DOAnchorPos(sisterSwappedPos, 0.5f).SetEase(Ease.OutBack);
        timerPanel.DOAnchorPos(timerSwappedPos, 0.5f).SetEase(Ease.OutBack);

        // Flip Visuals
        sisterPanel.DOScaleX(-1f, 0.3f);

        // Un-Flip Text
        foreach (var textRect in textObjectsToUnflip)
        {
            if (textRect != null) textRect.DOScaleX(-1f, 0f);
        }
    }

    [Button("2. Reset (Move to Start Pos)")]
    private void SwapToRight()
    {
        isSwapped = false;

        // Move back to the AUTOMATIC start coordinates
        sisterPanel.DOAnchorPos(sisterDefaultPos, 0.5f).SetEase(Ease.OutBack);
        timerPanel.DOAnchorPos(timerDefaultPos, 0.5f).SetEase(Ease.OutBack);

        // Reset Visuals
        sisterPanel.DOScaleX(1f, 0.3f);

        foreach (var textRect in textObjectsToUnflip)
        {
            if (textRect != null) textRect.DOScaleX(1f, 0f);
        }
    }

    // --- HELPER TOOL ---
    [Button("Capture Current Positions as Swapped Targets", ButtonSizes.Large)]
    [GUIColor(0.6f, 1f, 0.6f)]
    private void CaptureCurrentPositions()
    {
        if (sisterPanel != null) sisterSwappedPos = sisterPanel.anchoredPosition;
        if (timerPanel != null) timerSwappedPos = timerPanel.anchoredPosition;
        Debug.Log($"Saved! Sister Target: {sisterSwappedPos} | Timer Target: {timerSwappedPos}");
    }
}