using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private GameObject loadingPanel;
    
    private AsyncOperation loadingOperation;
    
    public void LoadScene(string sceneName)
    {
        loadingPanel.SetActive(true);
        loadingOperation = SceneManager.LoadSceneAsync(sceneName);
        loadingOperation.allowSceneActivation = false;
    }
    
    private void Update()
    {
        if (loadingOperation != null)
        {
            float progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);
            
            if (loadingSlider != null)
                loadingSlider.value = progress;
            
            if (progress >= 1f)
            {
                loadingOperation.allowSceneActivation = true;
                loadingOperation = null;
                
                if (loadingPanel != null)
                    loadingPanel.SetActive(false);
            }
        }
    }
}
