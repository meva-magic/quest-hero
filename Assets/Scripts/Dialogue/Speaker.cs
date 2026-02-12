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
    
    // [NEW] Store the repeating node if found
    private DialogueNode repeatingNode = null;
    
    // Dialogue Indicator
    [Header("Dialogue Indicator")]
    [SerializeField] private GameObject dialogueIndicator;
    [SerializeField] private KeyCode dialogueKey = KeyCode.Space;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inRange = false;
        
        if (dialogueIndicator != null)
            dialogueIndicator.SetActive(false);
            
        // [NEW] Find the repeating node in this dialogue
        FindRepeatingNode();
    }
    
    // [NEW] Search through all nodes to find the one marked as repeating
    private void FindRepeatingNode()
    {
        if (Dialogue == null) return;
        
        // Check RootNode
        if (Dialogue.RootNode != null && Dialogue.RootNode.isRepeatingNode)
        {
            repeatingNode = Dialogue.RootNode;
            Debug.Log($"Found repeating node in RootNode: {Dialogue.RootNode.name}");
            return;
        }
        
        // Check quest nodes
        if (Dialogue.questSuccessNode != null && Dialogue.questSuccessNode.isRepeatingNode)
        {
            repeatingNode = Dialogue.questSuccessNode;
            Debug.Log($"Found repeating node in questSuccessNode: {Dialogue.questSuccessNode.name}");
            return;
        }
        
        if (Dialogue.questReminderNode != null && Dialogue.questReminderNode.isRepeatingNode)
        {
            repeatingNode = Dialogue.questReminderNode;
            Debug.Log($"Found repeating node in questReminderNode: {Dialogue.questReminderNode.name}");
            return;
        }
        
        // If no repeating node found, repeatingNode stays null
        // Dialogue will play normally from beginning each time
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
        
        // [NEW] If we have a repeating node, always show that instead of starting from beginning
        if (repeatingNode != null)
        {
            DialogueManager.Instance.StartDialogue(Name, repeatingNode);
            StartCoroutine(WaitForDialogueEnd());
            return;
        }
        
        // Normal dialogue flow - only happens if no repeating node was found
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
        
        // [NEW] Check if we just played a node that should become the repeating node
        if (repeatingNode == null)
        {
            DialogueNode lastNode = DialogueManager.Instance.GetLastNode();
            if (lastNode != null && lastNode.isRepeatingNode)
            {
                repeatingNode = lastNode;
                Debug.Log($"Set repeating node to: {lastNode.name}");
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
        
        // [NEW] Check if we should set a repeating node after quest completion
        if (repeatingNode == null)
        {
            DialogueNode lastNode = DialogueManager.Instance.GetLastNode();
            if (lastNode != null && lastNode.isRepeatingNode)
            {
                repeatingNode = lastNode;
                Debug.Log($"Set repeating node after quest to: {lastNode.name}");
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
    
    // [NEW] Public method to reset dialogue (if needed)
    public void ResetDialogue()
    {
        repeatingNode = null;
        FindRepeatingNode(); // Re-scan for any node marked isRepeatingNode
    }
}
