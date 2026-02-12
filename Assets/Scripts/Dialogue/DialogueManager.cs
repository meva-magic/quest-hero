using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;
 
    public GameObject DialogueParent;
    public TextMeshProUGUI DialogTitleText, DialogBodyText;
    public GameObject responseButtonPrefab;
    public GameObject questButtonPrefab;
    public Transform responseButtonContainer;
    
    private string currentSpeakerName;
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
        if (node == null) return;
        
        lastNodeShown = node;
        currentSpeakerName = title;
        ShowDialogue();
        OnDialogueStarted?.Invoke();
 
        DialogTitleText.text = title;
        DialogBodyText.text = node.dialogueText;
 
        foreach (Transform child in responseButtonContainer)
        {
            Destroy(child.gameObject);
        }
 
        if (node.responses != null && node.responses.Count > 0)
        {
            foreach (DialogueResponse response in node.responses)
            {
                GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
                
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = response.responseText;
                
                Button button = buttonObj.GetComponent<Button>();
                DialogueResponse capturedResponse = response;
                button.onClick.AddListener(() => SelectResponse(capturedResponse, title));
            }
        }
        else
        {
            GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
            
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = "Продолжить";
            
            buttonObj.GetComponent<Button>().onClick.AddListener(() => HideDialogue());
        }
    }
 
    public void SelectResponse(DialogueResponse response, string title)
    {
        if (response.activateQuest && response.questNode != null)
        {
            if (QuestManager.instance != null)
                QuestManager.instance.ActivateQuest(response.questNode);
        }

        if (response.finishQuest)
        {
            if (QuestManager.instance != null)
                QuestManager.instance.FinishQuest();
        }

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
    
    public DialogueNode GetLastNode()
    {
        return lastNodeShown;
    }
    
    public void ForceEndDialogue()
    {
        if (IsDialogueActive())
        {
            HideDialogue();
        }
    }
    
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
    
    public void ResetDialogueState()
    {
        currentSpeakerName = null;
        lastNodeShown = null;
        ClearResponseButtons();
        HideDialogue();
    }
}
