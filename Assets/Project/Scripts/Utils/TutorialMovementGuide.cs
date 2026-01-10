using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;

public class TutorialMovementGuide : MonoBehaviour
{
    [Title("Resources")]
    [Required] public Sprite arrowSprite; // Drag your blue arrow image here

    [Title("Containers")]
    [InfoBox("Empty GameObjects in your Canvas where the arrows will appear.")]
    [Required] public RectTransform leftArrowHolder;
    [Required] public RectTransform rightArrowHolder;

    [Title("Animation Settings")]
    public float moveDistance = 30f;
    public float animDuration = 0.6f;

    private GameObject leftArrowGO;
    private GameObject rightArrowGO;

    private void OnEnable()
    {
        // Create arrows if they don't exist
        if (leftArrowGO == null) CreateArrows();

        // Reset and start animation
        ResetArrows();
        AnimateArrows();
    }

    private void OnDisable()
    {
        // Stop animations when tutorial step ends
        if (leftArrowGO != null) leftArrowGO.transform.DOKill();
        if (rightArrowGO != null) rightArrowGO.transform.DOKill();
    }

    [Button("Test Create Arrows")]
    private void CreateArrows()
    {
        // Clear old ones if any
        if (leftArrowHolder.childCount > 0) DestroyImmediate(leftArrowHolder.GetChild(0).gameObject);
        if (rightArrowHolder.childCount > 0) DestroyImmediate(rightArrowHolder.GetChild(0).gameObject);

        // --- LEFT ARROW ---
        leftArrowGO = CreateArrowObject("LeftArrow_Img", leftArrowHolder);
        // No rotation needed, sprite already points left

        // --- RIGHT ARROW ---
        rightArrowGO = CreateArrowObject("RightArrow_Img", rightArrowHolder);
        // Rotate 180 degrees to point right
        rightArrowGO.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    private GameObject CreateArrowObject(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.sprite = arrowSprite;
        img.SetNativeSize(); // Resize to match the sprite's original size

        return go;
    }

    private void ResetArrows()
    {
        if (leftArrowGO != null) leftArrowGO.transform.localPosition = Vector3.zero;
        if (rightArrowGO != null) rightArrowGO.transform.localPosition = Vector3.zero;
    }

    private void AnimateArrows()
    {
        // Move Left Arrow further left
        leftArrowGO.transform.DOLocalMoveX(-moveDistance, animDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true); // Ignore time scale

        // Move Right Arrow further right (positive X because it's rotated)
        rightArrowGO.transform.DOLocalMoveX(moveDistance, animDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }
}