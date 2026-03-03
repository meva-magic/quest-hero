using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CamZone_Location : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera mainCam = null;
    [SerializeField]
    private CinemachineVirtualCamera zoneCam = null;

    private bool isPlayerInZone = false;

    private void Start()
    {
        if (mainCam != null) mainCam.enabled = true;
        if (zoneCam != null) zoneCam.enabled = false;
    }

    private void OnEnable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted += OnDialogueStarted;
            DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
        }
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted -= OnDialogueStarted;
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            
            if (!DialogueManager.Instance.IsDialogueActive())
            {
                EnableZoneCam();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            
            if (!DialogueManager.Instance.IsDialogueActive())
            {
                EnableMainCam();
            }
        }
    }

    private void OnDialogueStarted()
    {
        if (zoneCam != null) zoneCam.enabled = false;
    }

    private void OnDialogueEnded()
    {
        if (isPlayerInZone && !DialogueManager.Instance.IsDialogueActive())
        {
            EnableZoneCam();
        }
        else
        {
            EnableMainCam();
        }
    }

    private void EnableZoneCam()
    {
        if (mainCam != null) mainCam.enabled = false;
        if (zoneCam != null) zoneCam.enabled = true;
    }

    private void EnableMainCam()
    {
        if (mainCam != null) mainCam.enabled = true;
        if (zoneCam != null) zoneCam.enabled = false;
    }

    private void OnValidate()
    {
        GetComponent<Collider>().isTrigger = true;
    }
}
