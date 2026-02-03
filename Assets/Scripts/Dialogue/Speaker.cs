using UnityEngine;
using System.Collections;
 
public class Speaker : MonoBehaviour
{
    private GameObject player;
    public string Name;
    public Dialogue Dialogue;
    private bool inRange;
    
    // Флаг для предотвращения множественных запусков диалога
    private bool isDialogueActive = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inRange = false;
    }
 
    private void Update()
    {
        if (inRange && Input.GetKeyDown(KeyCode.Space) && !isDialogueActive)
        {
            SpeakTo();
        }
    }
 
    public void SpeakTo()
    {
        if (DialogueManager.Instance.IsDialogueActive()) return;
        
        isDialogueActive = true;
        
        if (QuestManager.instance.currentQuest != null && 
            Dialogue.questNode != null &&
            Dialogue.questNode == QuestManager.instance.currentQuest)
        {
            QuestManager.instance.CheckGoal();
            
            if (QuestManager.instance.goalAchieved)
            {
                DialogueManager.Instance.StartDialogue(Name, Dialogue.questSuccessNode);
                StartCoroutine(WaitForQuestCompletion());
            }
            else
            {
                DialogueManager.Instance.StartDialogue(Name, Dialogue.questReminderNode);
                StartCoroutine(WaitForDialogueEnd());
            }
        }
        else
        {
            DialogueManager.Instance.StartDialogue(Name, Dialogue.RootNode);
            StartCoroutine(WaitForDialogueEnd());
        }
    }
    
    IEnumerator WaitForDialogueEnd()
    {
        yield return new WaitWhile(() => DialogueManager.Instance.IsDialogueActive());
        isDialogueActive = false;
    }
    
    IEnumerator WaitForQuestCompletion()
    {
        yield return new WaitWhile(() => DialogueManager.Instance.IsDialogueActive());
        
        // После диалога успеха проверяем, нужно ли завершить квест
        if (QuestManager.instance.currentQuest != null && QuestManager.instance.goalAchieved)
        {
            // Ищем ответ, который завершает квест
            if (Dialogue.questSuccessNode != null && Dialogue.questSuccessNode.responses != null)
            {
                foreach (var response in Dialogue.questSuccessNode.responses)
                {
                    if (response.finishQuest)
                    {
                        QuestManager.instance.FinishQuest();
                        break;
                    }
                }
            }
        }
        
        isDialogueActive = false;
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            // Можно добавить визуальную подсказку (например, значок "E" над NPC)
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            
            if (DialogueManager.Instance.IsDialogueActive() && 
                DialogueManager.Instance.GetCurrentSpeakerName() == Name)
            {
                DialogueManager.Instance.HideDialogue();
                isDialogueActive = false;
            }
        }
    }
}
