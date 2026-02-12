using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    [TextArea(3, 10)]
    public string dialogueText;
    public List<DialogueResponse> responses;

    public bool isRepeatingNode = false;

    public bool IsLastNode()
    {
        return responses == null || responses.Count <= 0;
    }
}