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

  private void Start ()
  {
    mainCam.enabled = true;
    dialogueCam.enabled = false;
  }

  private void OnTriggerEnter (Collider other)
  {
    if ( other.CompareTag("Player") )
      mainCam.enabled = false;
      dialogueCam.enabled = true;
  }

  private void OnTriggerExit (Collider other)
  {
    if ( other.CompareTag("Player") )
      mainCam.enabled = true;
      dialogueCam.enabled = false;
  }

  private void OnValidate ()
  {
    GetComponent<Collider>().isTrigger = true;
  }

  #endregion
}
