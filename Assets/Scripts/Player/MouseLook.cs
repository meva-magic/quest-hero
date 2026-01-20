using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    private float xMousePos;
    private float smoothMousePos;

    [SerializeField] private float mouseSensitivity = 1.5f;
    [SerializeField] private float smoothing = 1.5f;

    private float LookPos;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    private void Update()
    {
        GetInput();
        ModifyInput();
        MovePlayer();
    }

    private void GetInput()
    {
        xMousePos = Input.GetAxisRaw("Mouse X");
    }

    private void ModifyInput()
    {
        xMousePos *= mouseSensitivity * smoothing;
        smoothMousePos = Mathf.Lerp(smoothMousePos, xMousePos, 1/smoothing);
    }

    private void MovePlayer()
    {
        LookPos += smoothMousePos;
        transform.localRotation = Quaternion.AngleAxis(LookPos, transform.up);
    }
}