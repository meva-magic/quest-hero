using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CamZone : MonoBehaviour
{
    #region Inspector

    [SerializeField]
    private CinemachineVirtualCamera mainCam = null;

    [SerializeField]
    private CinemachineVirtualCamera dialogueCam = null;

    #endregion

    #region MonoBehaviour

    private void Start()
    {
        if (mainCam != null) mainCam.enabled = true;
        if (dialogueCam != null) dialogueCam.enabled = false;
    }

    private void OnEnable()
    {
        // Подписываемся на события диалога
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStarted += EnableDialogueCam;
            DialogueManager.Instance.OnDialogueEnded += EnableMainCam;
        }
    }

    private void OnDisable()
    {
        // Отписываемся от событий
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

    #endregion
}
