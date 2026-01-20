using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Range(0.001f, 0.05f)]
    [SerializeField] private float amount = 0.03f;

    [Range(10f, 100f)]
    [SerializeField] private float smooth = 69f;

    [Range(1f, 30f)]
    [SerializeField] private float frequency = 10f;

    private float inputMagnitude;
    private Vector3 startPos;
    private float timer = 0f;

    private void Start()
    {
        startPos = transform.localPosition;
    }

    private void Update()
    {
        CheckForMovement();
    }

    private void CheckForMovement()
    {
        inputMagnitude = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).magnitude;

        if (inputMagnitude > 0)
        {
            timer += Time.deltaTime * frequency;
            Vector3 targetPos = CalculateBobPosition(timer);
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos + targetPos, smooth * Time.deltaTime);
        }
        else
        {
            timer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, smooth * Time.deltaTime);
        }
    }

    private Vector3 CalculateBobPosition(float timer)
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(timer) * amount * 1.4f;
        pos.x = Mathf.Cos(timer / 2f) * amount * 1.6f;
        return pos;
    }
}
