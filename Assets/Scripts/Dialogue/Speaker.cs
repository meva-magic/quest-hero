using UnityEngine;
using System.Collections;
 
public class Speaker : MonoBehaviour
{
    private GameObject player;
    public string Name;
    public Dialogue Dialogue;
    private bool inRange;
    private bool isDialogueActive = false;
    private DialogueNode repeatingNode = null;
    
    [SerializeField] private GameObject dialogueIndicator;
    [SerializeField] private KeyCode dialogueKey = KeyCode.Space;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inRange = false;
        
        if (dialogueIndicator != null)
            dialogueIndicator.SetActive(false);
            
        FindRepeatingNode();
    }
    
    private void FindRepeatingNode()
    {
        if (Dialogue == null) return;
        
        if (Dialogue.RootNode != null && Dialogue.RootNode.isRepeatingNode)
        {
            repeatingNode = Dialogue.RootNode;
            return;
        }
        
        if (Dialogue.questSuccessNode != null && Dialogue.questSuccessNode.isRepeatingNode)
        {
            repeatingNode = Dialogue.questSuccessNode;
            return;
        }
        
        if (Dialogue.questReminderNode != null && Dialogue.questReminderNode.isRepeatingNode)
        {
            repeatingNode = Dialogue.questReminderNode;
            return;
        }
    }
 
    private void Update()
    {
        if (inRange && Input.GetKeyDown(dialogueKey) && !isDialogueActive)
        {
            SpeakTo();
        }
        
        UpdateDialogueIndicator();
    }
    
    private void UpdateDialogueIndicator()
    {
        if (dialogueIndicator == null) return;
        
        bool shouldShow = inRange && 
                         !isDialogueActive && 
                         DialogueManager.Instance != null && 
                         !DialogueManager.Instance.IsDialogueActive();
        
        dialogueIndicator.SetActive(shouldShow);
    }
 
    public void SpeakTo()
    {
        if (DialogueManager.Instance.IsDialogueActive()) return;
        
        isDialogueActive = true;
        
        if (repeatingNode != null)
        {
            DialogueManager.Instance.StartDialogue(Name, repeatingNode);
            StartCoroutine(WaitForDialogueEnd());
            return;
        }
        
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
        
        if (repeatingNode == null)
        {
            DialogueNode lastNode = DialogueManager.Instance.GetLastNode();
            if (lastNode != null && lastNode.isRepeatingNode)
            {
                repeatingNode = lastNode;
            }
        }
        
        isDialogueActive = false;
    }
    
    IEnumerator WaitForQuestCompletion()
    {
        yield return new WaitWhile(() => DialogueManager.Instance.IsDialogueActive());
        
        if (QuestManager.instance.currentQuest != null && QuestManager.instance.goalAchieved)
        {
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
        
        if (repeatingNode == null)
        {
            DialogueNode lastNode = DialogueManager.Instance.GetLastNode();
            if (lastNode != null && lastNode.isRepeatingNode)
            {
                repeatingNode = lastNode;
            }
        }
        
        isDialogueActive = false;
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
            
            if (DialogueManager.Instance.IsDialogueActive() && 
                DialogueManager.Instance.GetCurrentSpeakerName() == Name)
            {
                DialogueManager.Instance.HideDialogue();
                isDialogueActive = false;
            }
        }
    }
    
    public void ResetDialogue()
    {
        repeatingNode = null;
        FindRepeatingNode();
    }
}
