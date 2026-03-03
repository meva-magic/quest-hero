using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }
    
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    
    [Header("Music")]
    [SerializeField] private string mainTheme = "MainTheme";
    [SerializeField] private string menuTheme = "MenuMusic";
    
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
    }
    
    private void Start()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseClicked);
            
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
            
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
        
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmYes);
            
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(OnConfirmNo);
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }
    
    private void OnPauseClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
        else
        {
            TogglePauseManual();
        }
    }
    
    private void OnResumeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
        else
        {
            TogglePauseManual();
        }
    }
    
    private void OnMenuClicked()
    {
        ShowConfirmationDialog("Вы уверены, что хотите вернуться в меню?");
    }
    
    private void TogglePauseManual()
    {
        bool isPaused = Time.timeScale == 0f;
        Time.timeScale = isPaused ? 1f : 0f;
        
        if (!isPaused)
        {
            Show();
            SwitchToMenuMusic();
        }
        else
        {
            Hide();
            SwitchToMainMusic();
        }
    }
    
    private void SwitchToMenuMusic()
    {
        if (AudioManager.instance != null)
        {
            if (AudioManager.instance.IsPlaying(mainTheme))
                AudioManager.instance.Stop(mainTheme);
            
            if (!AudioManager.instance.IsPlaying(menuTheme))
                AudioManager.instance.Play(menuTheme);
        }
    }
    
    private void SwitchToMainMusic()
    {
        if (AudioManager.instance != null)
        {
            if (AudioManager.instance.IsPlaying(menuTheme))
                AudioManager.instance.Stop(menuTheme);
            
            if (!AudioManager.instance.IsPlaying(mainTheme))
                AudioManager.instance.Play(mainTheme);
        }
    }
    
    private void ShowConfirmationDialog(string message)
    {
        if (confirmationDialog == null) return;
        
        if (confirmationText != null)
            confirmationText.text = message;
            
        confirmationDialog.SetActive(true);
    }
    
    private void OnConfirmYes()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
        else
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    
    private void OnConfirmNo()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }
    
    public void Show()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
        SwitchToMenuMusic();
    }
    
    public void Hide()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
            
        SwitchToMainMusic();
    }
    
    private void OnDestroy()
    {
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(OnPauseClicked);
            
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);
            
        if (menuButton != null)
            menuButton.onClick.RemoveListener(OnMenuClicked);
            
        if (confirmYesButton != null)
            confirmYesButton.onClick.RemoveListener(OnConfirmYes);
            
        if (confirmNoButton != null)
            confirmNoButton.onClick.RemoveListener(OnConfirmNo);
    }
}
