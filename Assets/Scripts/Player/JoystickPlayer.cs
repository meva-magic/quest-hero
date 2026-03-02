using UnityEngine;

public class JoystickPlayer : MonoBehaviour
{
    [Header("References")]
    public VariableJoystick joystick; // Ссылка на ассет джойстика
    public DialogueManager dialogueManager; // Ссылка на ваш DialogueManager
    
    [Header("Settings")]
    public float moveSpeed = 10f;
    
    private Vector2 moveInput;
    private bool canMove = true;
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Подписываемся на события диалога для блокировки движения
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted += DisableMovement;
            DialogueManager.Instance.OnDialogueEnded += EnableMovement;
        }
    }
    
    void Update()
    {
        if (!canMove || (GameManager.Instance != null && GameManager.Instance.IsPaused()))
        {
            moveInput = Vector2.zero;
            return;
        }
        
        // Получаем ввод с джойстика
        moveInput = joystick.Direction;
        
        // Поворачиваем персонажа в направлении движения (для 2D)
        if (moveInput.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, -targetAngle);
        }
    }
    
    void FixedUpdate()
    {
        if (!canMove || (GameManager.Instance != null && GameManager.Instance.IsPaused()))
        {
            rb.velocity = Vector3.zero;
            return;
        }
        
        // Применяем движение
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed;
        rb.velocity = movement;
    }
    
    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted -= DisableMovement;
            DialogueManager.Instance.OnDialogueEnded -= EnableMovement;
        }
    }
    
    public void DisableMovement()
    {
        canMove = false;
        moveInput = Vector2.zero;
        rb.velocity = Vector3.zero;
    }
    
    public void EnableMovement()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPaused())
        {
            canMove = true;
            moveInput = Vector2.zero;
            
            // Сбрасываем джойстик
            if (joystick != null)
                joystick.OnPointerUp(null);
        }
    }
}
