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

    // --- NEW SECTION: CLEANUP MINIGAME SETTINGS ---
    [Title("Mess Settings")]
    [InfoBox("If this item causes a mess (like Milk/Eggs), assign the texture here. Leave null if no mess.")]
    public Texture2D messTexture; // The splatter image
    public AudioClip messSound;   // The sound it makes when splattering
    // ----------------------------------------------

    [Title("Education")]
    [TextArea(3, 10)]
    public string educationalTip = "Why was this a good or bad choice?";

    [Title("End Screen Visuals")]
    [PreviewField(50)]
    public Sprite sisterFeedbackSprite;

    [Title("Sister's Feedback (In-Game)")]
    public DialogueNode sisterReaction;
}