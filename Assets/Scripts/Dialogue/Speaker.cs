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
        if (QuestManager.instance.currentQuest != null && 
            Dialogue.questNode != null &&
            Dialogue.questNode == QuestManager.instance.currentQuest)
        {
            QuestManager.instance.CheckGoal();
            
            if (QuestManager.instance.goalAchieved)
            {
                DialogueManager.Instance.StartDialogue(Name, Dialogue.questSuccessNode);
            }
            else
            {
                DialogueManager.Instance.StartDialogue(Name, Dialogue.questReminderNode);
            }
        }
        else
        {
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

            DialogueManager.Instance.HideDialogue();
        }
    }
}
