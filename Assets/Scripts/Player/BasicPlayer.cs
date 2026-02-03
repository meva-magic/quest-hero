using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BasicPlayer : MonoBehaviour
{
    [Header("References")]
    Rigidbody rb;
    
    [Header("Settings")]
    [SerializeField]
    float moveSpeed = 10.0f;
    
    [Header("State")]
    Vector3 moveInput;
    bool canMove = true;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Подписываемся на события диалога
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted += DisableMovement;
            DialogueManager.Instance.OnDialogueEnded += EnableMovement;
        }
    }
    
    void Update()
    {
        // Проверяем паузу
        if (GameManager.Instance != null && GameManager.Instance.IsPaused())
        {
            if (canMove)
                DisableMovement();
            return;
        }
    }
    
    void OnDestroy()
    {
        // Отписываемся от событий
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted -= DisableMovement;
            DialogueManager.Instance.OnDialogueEnded -= EnableMovement;
        }
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!canMove || (GameManager.Instance != null && GameManager.Instance.IsPaused())) 
        {
            moveInput = Vector3.zero;
            return;
        }
        
        var v = context.ReadValue<Vector2>();
        moveInput.x = v.x;
        moveInput.z = v.y;
    }
    
    void FixedUpdate()
    {
        if (!canMove || (GameManager.Instance != null && GameManager.Instance.IsPaused()))
        {
            rb.velocity = Vector3.zero;
            return;
        }
        
        rb.velocity = moveSpeed * moveInput;
    }
    
    private void DisableMovement()
    {
        canMove = false;
        moveInput = Vector3.zero;
        rb.velocity = Vector3.zero;
    }
    
    private void EnableMovement()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPaused())
        {
            canMove = true;
        }
    }
    
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        if (!enabled) 
        {
            moveInput = Vector3.zero;
            rb.velocity = Vector3.zero;
        }
    }
}
