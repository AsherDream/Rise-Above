using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/New Node")]
public class DialogueNode : ScriptableObject
{
    [Title("Dialogue Content")]
    [TextArea(3, 10)]
    public string dialogueText;

    [Tooltip("Name of the character speaking")]
    public string speakerName;

    [Title("Next Steps")]
    [Tooltip("If true, this is the end of the conversation.")]
    public bool isEndNode = false;

    [Tooltip("If not an end node, this is the next piece of dialogue to show automatically.")]
    [ShowIf("@this.choices.Count == 0 && !this.isEndNode")]
    public DialogueNode nextNode;

    [Title("Player Choices")]
    [Tooltip("Add choices here. If empty, it will use 'nextNode' or end.")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}

[System.Serializable]
public class DialogueChoice
{
    [Tooltip("The text the player clicks on")]
    public string choiceText;

    [Tooltip("The dialogue node this choice leads to")]
    public DialogueNode nextNode;
}