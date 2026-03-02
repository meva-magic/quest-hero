using UnityEngine;
using System.Collections;
 
public class Speaker : MonoBehaviour
{
    private GameObject player;
    public string Name;
    public Dialogue Dialogue;
    private bool isPlayerInRange = false; // Флаг, что игрок В ЗОНЕ ЭТОГО NPC
    private bool isDialogueActive = false;
    private DialogueNode repeatingNode = null;
    
    [Header("Indicator")]
    [SerializeField] private GameObject dialogueIndicator;
    [SerializeField] private KeyCode dialogueKey = KeyCode.Space;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        isPlayerInRange = false;
        
        if (dialogueIndicator != null)
            dialogueIndicator.SetActive(false);
            
        FindRepeatingNode();
        
        Debug.Log($"Speaker {Name} started");
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
        // ГОВОРИТЬ МОЖЕТ ТОЛЬКО ТОТ, У КОГО PLAYER В ЗОНЕ
        if (isPlayerInRange && Input.GetKeyDown(dialogueKey) && !isDialogueActive && !DialogueManager.Instance.IsDialogueActive())
        {
            Debug.Log($"Player speaking with {Name}");
            SpeakTo();
        }
    }
 
    public void SpeakTo()
    {
        if (DialogueManager.Instance.IsDialogueActive()) return;
        
        isDialogueActive = true;
        
        if (dialogueIndicator != null)
            dialogueIndicator.SetActive(false);
        
        Debug.Log($"Speaker {Name} SpeakTo called");
        
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
            isPlayerInRange = true; // ТОЛЬКО ЭТОТ NPC ЗНАЕТ, ЧТО ИГРОК В ЕГО ЗОНЕ
            Debug.Log($"Player entered {Name} zone");
            
            if (dialogueIndicator != null && !isDialogueActive)
                dialogueIndicator.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false; // ИГРОК ВЫШЕЛ ИЗ ЗОНЫ ЭТОГО NPC
            Debug.Log($"Player exited {Name} zone");
            
            if (dialogueIndicator != null)
                dialogueIndicator.SetActive(false);
            
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
