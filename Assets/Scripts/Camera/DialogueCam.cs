using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CamZone_Dialogue : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera mainCam = null;
    [SerializeField]
    private CinemachineVirtualCamera dialogueCam = null;

    private void Start()
    {
        if (mainCam != null) mainCam.enabled = true;
        if (dialogueCam != null) dialogueCam.enabled = false;
    }

    private void OnEnable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted += EnableDialogueCam;
            DialogueManager.Instance.OnDialogueEnded += EnableMainCam;
        }
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted -= EnableDialogueCam;
            DialogueManager.Instance.OnDialogueEnded -= EnableMainCam;
        }
    }

    private void EnableDialogueCam()
    {
        if (mainCam != null) mainCam.enabled = false;
        if (dialogueCam != null) dialogueCam.enabled = true;
    }

    private void EnableMainCam()
    {
        if (mainCam != null) mainCam.enabled = true;
        if (dialogueCam != null) dialogueCam.enabled = false;
    }

    private void OnValidate()
    {
        GetComponent<Collider>().isTrigger = true;
    }
}
