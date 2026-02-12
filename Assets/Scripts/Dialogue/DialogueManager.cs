using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    // События для диалога
    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;
 
    public GameObject DialogueParent;
    public TextMeshProUGUI DialogTitleText, DialogBodyText;
    public GameObject responseButtonPrefab;
    public GameObject questButtonPrefab;
    public Transform responseButtonContainer;
    
    private string currentSpeakerName;
    
    // Track the last node that was shown
    private DialogueNode lastNodeShown = null;
 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
 
        HideDialogue();
    }
    
    private void OnDestroy()
    {
        OnDialogueStarted = null;
        OnDialogueEnded = null;
    }
 
    public void StartDialogue(string title, DialogueNode node)
    {
        if (node == null)
        {
            Debug.LogWarning("DialogueManager: DialogueNode is null");
            return;
        }
        
        // Store this as the last node shown
        lastNodeShown = node;
        
        currentSpeakerName = title;
        ShowDialogue();
        
        // Invoke dialogue started event
        OnDialogueStarted?.Invoke();
 
        // Set title and dialogue text
        DialogTitleText.text = title;
        DialogBodyText.text = node.dialogueText;
 
        // Clear previous response buttons
        foreach (Transform child in responseButtonContainer)
        {
            Destroy(child.gameObject);
        }
 
        // Create response buttons
        if (node.responses != null && node.responses.Count > 0)
        {
            foreach (DialogueResponse response in node.responses)
            {
                GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
                
                // Set button text
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = response.responseText;
                
                // Add click listener
                Button button = buttonObj.GetComponent<Button>();
                DialogueResponse capturedResponse = response; // Prevent closure issue
                button.onClick.AddListener(() => SelectResponse(capturedResponse, title));
            }
        }
        else
        {
            // No responses - add a single "Continue" button that closes dialogue
            GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
            
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = "Продолжить";
            
            buttonObj.GetComponent<Button>().onClick.AddListener(() => HideDialogue());
        }
    }
 
    public void SelectResponse(DialogueResponse response, string title)
    {
        // Activate quest if this response triggers one
        if (response.activateQuest && response.questNode != null)
        {
            if (QuestManager.instance != null)
                QuestManager.instance.ActivateQuest(response.questNode);
        }

        // Finish quest if this response completes it
        if (response.finishQuest)
        {
            if (QuestManager.instance != null)
                QuestManager.instance.FinishQuest();
        }

        // Go to next node or close dialogue
        if (response.nextNode != null)
        {
            StartDialogue(title, response.nextNode);
        }
        else
        {
            HideDialogue();
        }
    }
 
    public void HideDialogue()
    {
        if (DialogueParent != null)
            DialogueParent.SetActive(false);
        
        // Invoke dialogue ended event
        OnDialogueEnded?.Invoke();
    }
 
    private void ShowDialogue()
    {
        if (DialogueParent != null)
            DialogueParent.SetActive(true);
    }
 
    public bool IsDialogueActive()
    {
        return DialogueParent != null && DialogueParent.activeSelf;
    }
    
    public string GetCurrentSpeakerName()
    {
        return currentSpeakerName;
    }
    
    // Get the last node that was displayed
    public DialogueNode GetLastNode()
    {
        return lastNodeShown;
    }
    
    // Force end dialogue (useful for when player walks away)
    public void ForceEndDialogue()
    {
        if (IsDialogueActive())
        {
            HideDialogue();
        }
    }
    
    // Clear all response buttons (useful for resetting)
    public void ClearResponseButtons()
    {
        if (responseButtonContainer != null)
        {
            foreach (Transform child in responseButtonContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    // Reset the dialogue manager state
    public void ResetDialogueState()
    {
        currentSpeakerName = null;
        lastNodeShown = null;
        ClearResponseButtons();
        HideDialogue();
    }
}
