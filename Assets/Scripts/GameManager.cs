using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameScene = "Game";
    
    private bool isPaused = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        ResumeGame();
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Stop("MenuMusic");
            AudioManager.instance.Play("MainTheme");
        }
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Single);
        ResumeGame();
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Stop("MainTheme");
            AudioManager.instance.Play("MenuMusic");
        }
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        
        if (PauseMenuManager.Instance != null)
        {
            if (isPaused)
                PauseMenuManager.Instance.Show();
            else
                PauseMenuManager.Instance.Hide();
        }
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (PauseMenuManager.Instance != null)
            PauseMenuManager.Instance.Show();
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (PauseMenuManager.Instance != null)
            PauseMenuManager.Instance.Hide();
    }
    
    public bool IsPaused() => isPaused;
}
