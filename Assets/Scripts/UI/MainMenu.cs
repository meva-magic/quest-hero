using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    
    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
            
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmQuit);
            
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(OnCancelQuit);
            
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
            
        Time.timeScale = 1f;
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Stop("MainTheme");
            AudioManager.instance.Play("MenuMusic");
        }
    }
    
    private void OnStartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene();
        }
        else
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }
    
    private void OnQuitClicked()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(true);
    }
    
    private void OnConfirmQuit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
    
    private void OnCancelQuit()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }
    
    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartClicked);
            
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
            
        if (confirmYesButton != null)
            confirmYesButton.onClick.RemoveListener(OnConfirmQuit);
            
        if (confirmNoButton != null)
            confirmNoButton.onClick.RemoveListener(OnCancelQuit);
    }
}
