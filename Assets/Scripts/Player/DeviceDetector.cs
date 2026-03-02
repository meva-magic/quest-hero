using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class DeviceDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BasicPlayer playerController; // Ваш существующий контроллер
    [SerializeField] private JoystickPlayer joystickController; // Новый джойстик контроллер
    [SerializeField] private GameObject joystickUI; // UI панель с джойстиком
    
    private bool isMobile = false;
    
    void Start()
    {
        // Определяем устройство
        #if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
            isMobile = true;
        #else
            // Проверяем через Input System (если есть геймпад, это все еще может быть ПК)
            isMobile = Application.isMobilePlatform || 
                      SystemInfo.deviceType == DeviceType.Handheld;
        #endif
        
        // Альтернативный метод: проверка сенсорного ввода
        if (!isMobile && Touchscreen.current != null)
        {
            isMobile = true;
        }
        
        // Включаем правильное управление
        if (isMobile)
        {
            SwitchToMobile();
        }
        else
        {
            SwitchToPC();
        }
    }
    
    void SwitchToMobile()
    {
        Debug.Log("Mobile device detected - switching to joystick controls");
        
        // Отключаем управление с клавиатуры
        if (playerController != null)
            playerController.enabled = false;
        
        // Включаем управление с джойстика
        if (joystickController != null)
            joystickController.enabled = true;
        
        // Показываем UI джойстика
        if (joystickUI != null)
            joystickUI.SetActive(true);
        
        // Активируем поддержку сенсорного ввода
        EnhancedTouchSupport.Enable();
    }
    
    void SwitchToPC()
    {
        Debug.Log("PC detected - switching to keyboard controls");
        
        // Включаем управление с клавиатуры
        if (playerController != null)
            playerController.enabled = true;
        
        // Отключаем управление с джойстика
        if (joystickController != null)
            joystickController.enabled = false;
        
        // Прячем UI джойстика
        if (joystickUI != null)
            joystickUI.SetActive(false);
    }
    
    // Метод для ручного переключения (для тестирования)
    public void ForceMobileMode()
    {
        isMobile = true;
        SwitchToMobile();
    }
    
    public void ForcePCMode()
    {
        isMobile = false;
        SwitchToPC();
    }
}
