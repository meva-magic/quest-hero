using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ConfirmationDialog : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    
    [Header("Events")]
    public UnityEvent onYes;
    public UnityEvent onNo;
    
    private void Start()
    {
        // Скрыть диалог при старте
        Hide();
        
        // Настройка кнопок
        if (yesButton != null)
            yesButton.onClick.AddListener(OnYes);
            
        if (noButton != null)
            noButton.onClick.AddListener(OnNo);
    }
    
    public void Show(string message)
    {
        if (messageText != null)
            messageText.text = message;
            
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
    }
    
    public void Show(string message, UnityAction yesAction, UnityAction noAction = null)
    {
        if (messageText != null)
            messageText.text = message;
            
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
        
        // Очищаем предыдущие события
        onYes.RemoveAllListeners();
        onNo.RemoveAllListeners();
        
        // Добавляем новые события
        if (yesAction != null)
            onYes.AddListener(yesAction);
        
        if (noAction != null)
            onNo.AddListener(noAction);
    }
    
    private void OnYes()
    {
        onYes?.Invoke();
        Hide();
    }
    
    private void OnNo()
    {
        onNo?.Invoke();
        Hide();
    }
    
    public void Hide()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
        
        // Очищаем события
        onYes.RemoveAllListeners();
        onNo.RemoveAllListeners();
    }
    
    private void OnDestroy()
    {
        if (yesButton != null)
            yesButton.onClick.RemoveListener(OnYes);
            
        if (noButton != null)
            noButton.onClick.RemoveListener(OnNo);
    }
}
