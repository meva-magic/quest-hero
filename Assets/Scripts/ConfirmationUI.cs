using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ConfirmationDialog : MonoBehaviour
{
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    
    public UnityEvent onYes;
    public UnityEvent onNo;
    
    private void Start()
    {
        Hide();
        
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
        
        onYes.RemoveAllListeners();
        onNo.RemoveAllListeners();
        
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
