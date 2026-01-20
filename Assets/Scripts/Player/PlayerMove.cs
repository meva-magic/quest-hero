using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public static PlayerMove _instance;

    [SerializeField] private float playerSpeed = 10;
    //[SerializeField] private float damping = 2f;

    [SerializeField] private CharacterController controller;

    private Vector3 inputVector;
    private Vector3 movementVector;

    //private bool isMoving;
    //public bool isTalking;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }

        else _instance = this;
    }


    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        GetInput();
        MovePlayer();
    }

    private void GetInput()
    {
        inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        inputVector.Normalize();
        inputVector = transform.TransformDirection(inputVector);

        movementVector = inputVector * playerSpeed;
    }

    private void MovePlayer()
    {
        controller.Move(movementVector * Time.deltaTime);
    }
}
