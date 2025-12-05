using UnityEngine;
using Sirenix.OdinInspector;

// Define the 3 Flood Categories
public enum FloodItemType
{
    Essential, // Adds to Meter (Water, Light)
    Burden,    // Lowers Max Capacity (Guitar, Heavy Toys)
    Wasteful   // Damages Meter (Perishables)
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Game/Inspectable Item Data")]
public class InspectableItemData : ScriptableObject
{
    [Title("Item Information")]
    [Required]
    [PreviewField(100, ObjectFieldAlignment.Left)]
    public Sprite itemSprite;

    [Required]
    public string itemName = "New Item";

    [TextArea(5, 20)]
    public string itemDescription = "Enter item description here...";

    [Title("Flood Mechanics")]
    [EnumToggleButtons]
    public FloodItemType itemType = FloodItemType.Essential;

    [Tooltip("Essential: How much it fills the meter.\nBurden: How much it reduces MAX capacity.\nWasteful: How much it hurts the meter.")]
    [Range(0, 50)]
    public int impactValue = 10;

    [Title("Sister's Feedback")]
    [Tooltip("The specific dialogue node to play when this item is dropped in the cart.")]
    public DialogueNode sisterReaction;
}