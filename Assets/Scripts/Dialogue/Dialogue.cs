using UnityEngine;
 
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Asset")]
public class Dialogue : ScriptableObject
{
    public DialogueNode RootNode;
    public QuestNode questNode; // The quest this NPC gives
    public DialogueNode questSuccessNode; // Dialogue when quest is completed
    public DialogueNode questReminderNode; // Dialogue when quest is active but not completed
}
