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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var v = context.ReadValue<Vector2>();
        moveInput.x = v.x;
        moveInput.z = v.y;
    }

    void FixedUpdate()
    {
        rb.velocity = moveSpeed * moveInput;
    }
}
