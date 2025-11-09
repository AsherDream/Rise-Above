using UnityEngine;
using Sirenix.OdinInspector;

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
}