using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject confirmationDialog;
    
    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
            
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
            
        Time.timeScale = 1f;
        
        // Воспроизводим музыку меню
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
            // Запасной вариант
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
    
    private void OnQuitClicked()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(true);
    }
    
    public void OnConfirmQuit()
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
    
    public void OnCancelQuit()
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
    }
}