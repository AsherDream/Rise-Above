using UnityEngine;
using Sirenix.OdinInspector;

public class DialogueTrigger : MonoBehaviour
{
    [Required]
    public DialogueNode conversationStartNode;

    [Button("Test Dialogue")]
    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(conversationStartNode);
    }
}