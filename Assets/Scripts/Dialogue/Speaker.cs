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
    
    // [NEW] Dialogue Indicator
    [Header("Dialogue Indicator")]
    [SerializeField] private GameObject dialogueIndicator; // Drag your UI indicator here
    [SerializeField] private KeyCode dialogueKey = KeyCode.Space; // Default Space

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inRange = false;
        
        // [NEW] Make sure indicator starts disabled
        if (dialogueIndicator != null)
            dialogueIndicator.SetActive(false);
    }
 
    private void Update()
    {
        if (inRange && Input.GetKeyDown(dialogueKey) && !isDialogueActive)
        {
            SpeakTo();
        }
        
        // [NEW] Update indicator state every frame
        UpdateDialogueIndicator();
    }
    
    // [NEW] Simple method to update indicator visibility
    private void UpdateDialogueIndicator()
    {
        if (dialogueIndicator == null) return;
        
        // Show indicator ONLY when:
        // 1. Player is in range
        // 2. Dialogue is NOT active
        // 3. No active dialogue in the manager
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
        
        // Indicator will be hidden automatically in UpdateDialogueIndicator()
        
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
        // Indicator will reappear in UpdateDialogueIndicator()
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
        
        isDialogueActive = false;
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            // Indicator will show in UpdateDialogueIndicator()
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
