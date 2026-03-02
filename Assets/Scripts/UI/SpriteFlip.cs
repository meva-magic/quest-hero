using UnityEngine;

public class SpriteFlipper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spriteQuad; // The quad with the sprite
    
    [Header("Settings")]
    [SerializeField] private bool faceMovementDirection = true;
    
    // For Rigidbody-based movement (like player)
    private Rigidbody rb;
    
    // For NavMeshAgent-based movement (like NPCs)
    private UnityEngine.AI.NavMeshAgent agent;
    
    private Vector3 lastPosition;
    private Vector3 originalScale;
    
    void Start()
    {
        if (spriteQuad == null)
            spriteQuad = transform; // Use self if not specified
        
        // Store the original scale (should be positive)
        originalScale = spriteQuad.localScale;
        if (originalScale.x < 0) originalScale.x = Mathf.Abs(originalScale.x);
        
        lastPosition = transform.position;
        
        // Try to get movement components
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
    
    void Update()
    {
        Vector3 moveDirection = GetMovementDirection();
        
        if (faceMovementDirection && moveDirection.magnitude > 0.1f)
        {
            // Flip based on movement direction
            FlipSprite(moveDirection);
        }
    }
    
    Vector3 GetMovementDirection()
    {
        // Get current movement direction
        Vector3 movement = Vector3.zero;
        
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            movement = rb.velocity.normalized;
        }
        else if (agent != null && agent.velocity.magnitude > 0.1f)
        {
            movement = agent.velocity.normalized;
        }
        else
        {
            // Fallback: calculate from position change
            Vector3 currentPos = transform.position;
            movement = (currentPos - lastPosition).normalized;
            lastPosition = currentPos;
        }
        
        return movement;
    }
    
    void FlipSprite(Vector3 direction)
    {
        if (direction.magnitude < 0.1f) return;
        
        // Create a new scale based on original
        Vector3 newScale = originalScale;
        
        // Flip X based on horizontal movement direction
        if (direction.x > 0.1f)
        {
            // Moving right - normal orientation
            newScale.x = Mathf.Abs(originalScale.x);
        }
        else if (direction.x < -0.1f)
        {
            // Moving left - flip X
            newScale.x = -Mathf.Abs(originalScale.x);
        }
        // If moving mostly forward/back with little sideways, keep current flip
        
        // Apply the new scale
        spriteQuad.localScale = newScale;
    }
    
    // Optional: Call this if you need to reset the sprite orientation
    public void ResetFacing()
    {
        spriteQuad.localScale = new Vector3(
            Mathf.Abs(originalScale.x),
            originalScale.y,
            originalScale.z
        );
    }
}
