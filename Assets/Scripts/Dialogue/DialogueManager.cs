using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
 
    public GameObject DialogueParent;
    public TextMeshProUGUI DialogTitleText, DialogBodyText;
    public GameObject responseButtonPrefab;
    public GameObject questButtonPrefab;
    public Transform responseButtonContainer;
 
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
 
    public void StartDialogue(string title, DialogueNode node)
    {
        ShowDialogue();
 
        DialogTitleText.text = title;
        DialogBodyText.text = node.dialogueText;
 
        foreach (Transform child in responseButtonContainer)
        {
            Destroy(child.gameObject);
        }
 
        foreach (DialogueResponse response in node.responses)
        {
            GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = response.responseText;
 
            buttonObj.GetComponent<Button>().onClick.AddListener(() => SelectResponse(response, title));
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
    }
 
    private void ShowDialogue()
    {
        DialogueParent.SetActive(true);
    }
 
    public bool IsDialogueActive()
    {
        return DialogueParent.activeSelf;
    }
}
