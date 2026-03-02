using UnityEngine;

public class ForceCameraManager : MonoBehaviour
{
    [SerializeField] private GameObject mainCam;
    [SerializeField] private GameObject ambCam;
    [SerializeField] private GameObject loveCam;
    [SerializeField] private GameObject thiefCam;
    
    private Camera mainCameraComponent;
    private Camera ambCameraComponent;
    private Camera loveCameraComponent;
    private Camera thiefCameraComponent;
    
    private void Start()
    {
        // Получаем компоненты Camera
        if (mainCam != null) mainCameraComponent = mainCam.GetComponent<Camera>();
        if (ambCam != null) ambCameraComponent = ambCam.GetComponent<Camera>();
        if (loveCam != null) loveCameraComponent = loveCam.GetComponent<Camera>();
        if (thiefCam != null) thiefCameraComponent = thiefCam.GetComponent<Camera>();
        
        // Включаем только главную камеру
        EnableOnlyMain();
    }
    
    private void OnEnable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted += OnDialogueStarted;
            DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
        }
    }
    
    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted -= OnDialogueStarted;
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        }
    }
    
    private void OnDialogueStarted()
    {
        string speaker = DialogueManager.Instance.GetCurrentSpeakerName();
        Debug.Log($"Dialogue with: {speaker}");
        
        // Выключаем все камеры
        DisableAllCameras();
        
        // Включаем нужную
        switch (speaker)
        {
            case "Любимая":
                if (loveCameraComponent != null) loveCameraComponent.enabled = true;
                Debug.Log($"LoveCam enabled: {loveCameraComponent?.enabled}");
                break;
            case "Amb":
                if (ambCameraComponent != null) ambCameraComponent.enabled = true;
                Debug.Log($"AmbCam enabled: {ambCameraComponent?.enabled}");
                break;
            case "Thief":
                if (thiefCameraComponent != null) thiefCameraComponent.enabled = true;
                Debug.Log($"ThiefCam enabled: {thiefCameraComponent?.enabled}");
                break;
        }
    }
    
    private void OnDialogueEnded()
    {
        Debug.Log("Dialogue ended");
        EnableOnlyMain();
    }
    
    private void EnableOnlyMain()
    {
        DisableAllCameras();
        if (mainCameraComponent != null) mainCameraComponent.enabled = true;
    }
    
    private void DisableAllCameras()
    {
        if (mainCameraComponent != null) mainCameraComponent.enabled = false;
        if (ambCameraComponent != null) ambCameraComponent.enabled = false;
        if (loveCameraComponent != null) loveCameraComponent.enabled = false;
        if (thiefCameraComponent != null) thiefCameraComponent.enabled = false;
    }
    
    // Тест клавишами
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DisableAllCameras();
            if (ambCameraComponent != null) ambCameraComponent.enabled = true;
            Debug.Log("1 - AmbCam");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DisableAllCameras();
            if (loveCameraComponent != null) loveCameraComponent.enabled = true;
            Debug.Log("2 - LoveCam");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            DisableAllCameras();
            if (thiefCameraComponent != null) thiefCameraComponent.enabled = true;
            Debug.Log("3 - ThiefCam");
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            EnableOnlyMain();
            Debug.Log("0 - MainCam");
        }
    }
}
