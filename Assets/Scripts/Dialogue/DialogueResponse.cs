using UnityEngine;

[System.Serializable]
public class DialogueResponse
{
    public string responseText;
    public DialogueNode nextNode;
    
    public bool activateQuest;
    public QuestNode questNode;
    public bool finishQuest;
    public bool giveReward;
}
