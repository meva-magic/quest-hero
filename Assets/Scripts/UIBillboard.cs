using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    [SerializeField] private Transform cameraToFace;

    private void LateUpdate()
    {
        transform.LookAt(cameraToFace);
    }
}
