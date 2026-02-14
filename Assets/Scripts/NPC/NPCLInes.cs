using UnityEngine;
using TMPro;
using System.Collections;

public class NPCDialogueUI : MonoBehaviour
{
    public static NPCDialogueUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    
    [Header("Settings")]
    [SerializeField] private float displayTime = 3f; // Одно время для всех сообщений

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDialogue(string text)
    {
        ShowDialogue(text, displayTime);
    }

    public void ShowDialogue(string text, float customDisplayTime)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayDialogueRoutine(text, customDisplayTime));
    }

    private IEnumerator DisplayDialogueRoutine(string text, float duration)
    {
        // Устанавливаем текст
        if (dialogueText != null)
            dialogueText.text = text;
        
        // Показываем панель
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        // Ждем указанное время
        yield return new WaitForSeconds(duration);
        
        // Скрываем панель
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void HideDialogueImmediate()
    {
        StopAllCoroutines();
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}
