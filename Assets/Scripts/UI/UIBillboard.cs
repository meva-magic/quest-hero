using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    [SerializeField] private Transform cameraToFace;
    [SerializeField] private bool flipX = false; // Optional: flip if needed

    private void Start()
    {
        if (cameraToFace == null)
            cameraToFace = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (cameraToFace == null) return;
        
        // Make the panel face the camera
        transform.LookAt(transform.position + cameraToFace.rotation * Vector3.forward,
                        cameraToFace.rotation * Vector3.up);
        
        // Optional: If text appears flipped, uncomment this
        // transform.Rotate(0, 180, 0);
    }
}
