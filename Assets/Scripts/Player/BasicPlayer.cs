using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BasicPlayer : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] float moveSpeed = 10.0f;
    Vector3 moveInput;
    bool canMove = true;
    
    [Header("Footsteps")]
    [SerializeField] string footstepSoundName = "Footsteps";
    bool isMoving = false;
    
    [Header("Visual")]
    [SerializeField] private GameObject playerModel; // Перетащите модель игрока сюда
    [SerializeField] private bool hideModelDuringDialogue = true;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted += DisableMovement;
            DialogueManager.Instance.OnDialogueEnded += EnableMovement;
            
            // Добавляем скрытие модели
            if (hideModelDuringDialogue)
            {
                DialogueManager.Instance.OnDialogueStarted += HideModel;
                DialogueManager.Instance.OnDialogueEnded += ShowModel;
            }
        }
        
        // Если модель не назначена, используем дочерний объект с MeshRenderer
        if (playerModel == null)
        {
            // Ищем MeshRenderer в дочерних объектах
            MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
                playerModel = renderer.gameObject;
            else
                // Ищем SkinnedMeshRenderer (для анимированных моделей)
                playerModel = GetComponentInChildren<SkinnedMeshRenderer>()?.gameObject;
        }
    }
    
    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused())
        {
            if (canMove)
                DisableMovement();
            return;
        }
        
        // Check if player is moving
        bool wasMoving = isMoving;
        isMoving = moveInput.magnitude > 0.1f && canMove;
        
        // Handle footsteps sound
        if (isMoving && !wasMoving)
        {
            if (AudioManager.instance != null)
                AudioManager.instance.Play(footstepSoundName);
        }
        else if (!isMoving && wasMoving)
        {
            if (AudioManager.instance != null)
                AudioManager.instance.Stop(footstepSoundName);
        }
    }
    
    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted -= DisableMovement;
            DialogueManager.Instance.OnDialogueEnded -= EnableMovement;
            
            if (hideModelDuringDialogue)
            {
                DialogueManager.Instance.OnDialogueStarted -= HideModel;
                DialogueManager.Instance.OnDialogueEnded -= ShowModel;
            }
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
        
        if (AudioManager.instance != null)
            AudioManager.instance.Stop(footstepSoundName);
    }
    
    private void EnableMovement()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPaused())
        {
            canMove = true;
        }
    }
    
    // Новые методы для скрытия/показа модели
    private void HideModel()
    {
        if (playerModel != null)
            playerModel.SetActive(false);
    }
    
    private void ShowModel()
    {
        if (playerModel != null)
            playerModel.SetActive(true);
    }
    
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        if (!enabled) 
        {
            moveInput = Vector3.zero;
            rb.velocity = Vector3.zero;
            
            if (AudioManager.instance != null)
                AudioManager.instance.Stop(footstepSoundName);
        }
    }
}
