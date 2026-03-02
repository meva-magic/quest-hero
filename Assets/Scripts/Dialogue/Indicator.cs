using UnityEngine;

public class DialogueIndicator : MonoBehaviour
{
    [SerializeField] private GameObject indicatorObject; // Просто объект для включения/выключения

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (indicatorObject != null)
                indicatorObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (indicatorObject != null)
                indicatorObject.SetActive(false);
        }
    }
}
