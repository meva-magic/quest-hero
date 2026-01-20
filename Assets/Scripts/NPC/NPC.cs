using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class NPC : MonoBehaviour//, IInteractable
{
    [SerializeField] private NPCScriptableObject dialogueData;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText, nameText;
    [SerializeField] private Image portrait;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    private void Start()
    {
        //NPCID ??= GlobalHelper.GenerateUniqueID(gameObject);
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if(dialogueData == null || !isDialogueActive)
        {
            return;
        }

        if(isDialogueActive)
        {
            NextLine();
        }

        else
        {
            StartDialogue();
        }
    }


    public void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.nametag);
        portrait.sprite = dialogueData.portrait;

        dialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }


    private void NextLine()
    {
        if(isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }

        else if(++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }

        else
        {
            EndDialogue();
        }
    }


    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach(char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if(dialogueData.skipLines.Length > dialogueIndex && dialogueData.skipLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.skipDelay);

            NextLine();
        }
    }

    
    private void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
    }
}
