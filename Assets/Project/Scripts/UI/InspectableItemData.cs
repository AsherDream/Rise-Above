using UnityEngine;
using Sirenix.OdinInspector;

public enum FloodItemType
{
    Essential,
    Burden,
    Wasteful
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Game/Inspectable Item Data")]
public class InspectableItemData : ScriptableObject
{
    [Title("Item Information")]
    [Required, PreviewField(100)] public Sprite itemSprite;
    [Required] public string itemName = "New Item";
    [TextArea(5, 20)] public string itemDescription = "Enter item description here...";

    [Title("Flood Mechanics")]
    [EnumToggleButtons] public FloodItemType itemType = FloodItemType.Essential;
    [Range(0, 50)] public int impactValue = 10;

    [Title("Education")]
    [TextArea(3, 10)]
    [Tooltip("The lesson displayed on the end screen.")]
    public string educationalTip = "Why was this a good or bad choice?";

    // --- End Screen Visuals ---
    [Title("End Screen Visuals")]
    [Tooltip("The face the sister makes when reviewing this item.")]
    [PreviewField(50)]
    public Sprite sisterFeedbackSprite;
    // --------------------------

    [Title("Sister's Feedback (In-Game)")]
    public DialogueNode sisterReaction;
}