// DialoguePanel.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialoguePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject panel;
    
    private float hideTimer;
    private bool isShowing;
    
    void Start()
    {
        HideImmediate();
    }
    
    void Update()
    {
        if (isShowing)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                Hide();
            }
        }
    }
    
    public void Show(string text, float duration)
    {
        if (dialogueText != null)
            dialogueText.text = text;
            
        if (panel != null)
            panel.SetActive(true);
            
        hideTimer = duration;
        isShowing = true;
    }
    
    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
            
        isShowing = false;
    }
    
    public void HideImmediate()
    {
        if (panel != null)
            panel.SetActive(false);
            
        isShowing = false;
        hideTimer = 0;
    }
}
