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
 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
 
        HideDialogue();
    }
    
    private void OnDestroy()
    {
        // Отписываемся от всех событий при уничтожении
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
        
        currentSpeakerName = title;
        ShowDialogue();
        
        // Вызываем событие начала диалога
        OnDialogueStarted?.Invoke();
 
        DialogTitleText.text = title;
        DialogBodyText.text = node.dialogueText;
 
        // Очищаем предыдущие кнопки
        foreach (Transform child in responseButtonContainer)
        {
            Destroy(child.gameObject);
        }
 
        // Проверяем есть ли ответы
        if (node.responses != null && node.responses.Count > 0)
        {
            foreach (DialogueResponse response in node.responses)
            {
                GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
                
                // Находим TextMeshProUGUI компонент
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = response.responseText;
                
                buttonObj.GetComponent<Button>().onClick.AddListener(() => SelectResponse(response, title));
            }
        }
        else
        {
            // Если нет ответов, добавляем кнопку "Продолжить"
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
            QuestManager.instance.ActivateQuest(response.questNode);
        }

        if (response.finishQuest)
        {
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
        DialogueParent.SetActive(false);
        
        // Вызываем событие окончания диалога
        OnDialogueEnded?.Invoke();
    }
 
    private void ShowDialogue()
    {
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
}
