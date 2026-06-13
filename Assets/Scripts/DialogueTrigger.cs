using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueBox dialogueBox;
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            dialogueBox.StartDialoguePublic();
        }
    }
}