using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Asset")]
public class Dialogue : ScriptableObject
{
    public DialogueNode RootNode;
    public QuestNode questNode;
    public DialogueNode questSuccessNode;
    public DialogueNode questReminderNode;    
    public DialogueNode completedDialogueNode;
}
