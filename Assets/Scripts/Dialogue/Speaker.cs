using UnityEngine;
 
public class Speaker : MonoBehaviour
{
    private GameObject player;
    public string Name;
    public Dialogue Dialogue;
    private bool inRange;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inRange = false;
    }
 
    private void Update()
    {
        if (inRange && Input.GetKeyDown(KeyCode.Space))
        {
            SpeakTo();
        }
    }
 
    public void SpeakTo()
    {
        // Check if player has an active quest from this NPC
        if (QuestManager.instance.currentQuest != null && 
            Dialogue.questNode != null &&
            Dialogue.questNode == QuestManager.instance.currentQuest)
        {
            QuestManager.instance.CheckGoal();
            
            if (QuestManager.instance.goalAchieved)
            {
                // Show success dialogue if quest item is found
                DialogueManager.Instance.StartDialogue(Name, Dialogue.questSuccessNode);
            }
            else
            {
                // Show reminder dialogue if quest is active but not completed
                DialogueManager.Instance.StartDialogue(Name, Dialogue.questReminderNode);
            }
        }
        else
        {
            // Show normal dialogue
            DialogueManager.Instance.StartDialogue(Name, Dialogue.RootNode);
        }
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
        }
    }
}
