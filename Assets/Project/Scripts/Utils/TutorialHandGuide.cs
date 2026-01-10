using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;

public class TutorialHandGuide : MonoBehaviour
{
    public enum HandMode { Dragging, Inspecting }

    [Title("Mode Settings")]
    public HandMode currentMode = HandMode.Dragging;

    [Title("UI References")]
    [Required] public RectTransform handIcon;

    // --- FIX: SEPARATE TARGETS FOR EACH MODE ---
    [Title("Drag Targets (Step 2)")]
    [Required] public RectTransform dragStartPoint;  // Assign the Water Bottle here
    [Required] public RectTransform cartPosition;    // Assign the Cart here

    [Title("Inspect Target (Step 3)")]
    [Required] public RectTransform inspectTarget;   // Assign a DIFFERENT item here
    // -------------------------------------------

    [Title("Drag Settings")]
    [ShowIf("currentMode", HandMode.Dragging)]
    public float dragSpeed = 2.0f;

    [Title("Inspect Settings")]
    [ShowIf("currentMode", HandMode.Inspecting)]
    public float pulseScale = 1.2f;
    [ShowIf("currentMode", HandMode.Inspecting)]
    public float pulseDuration = 0.5f;

    private Vector3 originalScale;

    private void Awake()
    {
        if (handIcon != null) originalScale = handIcon.localScale;
    }

    private void OnEnable()
    {
        // Kill any old animations first
        StopAllCoroutines();
        if (handIcon != null)
        {
            handIcon.DOKill();
            handIcon.localScale = originalScale;
        }

        switch (currentMode)
        {
            case HandMode.Dragging:
                // Check dragStartPoint, NOT itemPosition
                if (dragStartPoint != null && cartPosition != null)
                    StartCoroutine(PlayDragAnimation());
                break;

            case HandMode.Inspecting:
                // Check inspectTarget, NOT itemPosition
                if (inspectTarget != null)
                    PlayInspectAnimation();
                break;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (handIcon != null)
        {
            handIcon.DOKill();
            handIcon.localScale = originalScale;
        }
    }

    // --- MODE 1: DRAGGING ---
    private IEnumerator PlayDragAnimation()
    {
        // Safety check
        if (dragStartPoint == null) yield break;

        while (true)
        {
            float timer = 0f;

            // 1. Reset to Drag Start Item
            if (dragStartPoint != null) handIcon.position = dragStartPoint.position;

            // 2. Move to Cart
            while (timer < 1f)
            {
                timer += Time.unscaledDeltaTime * dragSpeed;
                if (dragStartPoint != null)
                    handIcon.position = Vector3.Lerp(dragStartPoint.position, cartPosition.position, timer);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    // --- MODE 2: INSPECTING ---
    private void PlayInspectAnimation()
    {
        // 1. Snap to the INSPECT TARGET (Safe item)
        if (inspectTarget != null)
        {
            handIcon.position = inspectTarget.position;

            // 2. Pulse effect
            handIcon.DOScale(originalScale * pulseScale, pulseDuration)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }
    }
}