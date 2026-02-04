using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ДОБАВИТЬ ЭТУ СТРОКУ

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject confirmationDialog;
    
    [Header("Confirmation Buttons")]
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    
    private void Start()
    {
        // Настройка кнопок главного меню
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
            
        // Настройка кнопок подтверждения
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmQuit);
            
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(OnCancelQuit);
            
        // Скрываем диалог подтверждения при старте
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
            
        // Восстанавливаем нормальную скорость времени
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
            Debug.LogWarning("GameManager не найден, загружаем сцену напрямую");
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
        // Отписываемся от событий
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
