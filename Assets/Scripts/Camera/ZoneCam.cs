using Cinemachine;
using UnityEngine;
using System.Collections.Generic;

public class CamZoneManager : MonoBehaviour
{
    [Header("Main Cameras")]
    [SerializeField] private CinemachineVirtualCamera mainCam;
    [SerializeField] private CinemachineVirtualCamera dialogueCam;
    
    [Header("Zone Cameras")]
    [SerializeField] private List<ZoneCamera> zoneCameras = new List<ZoneCamera>();
    
    [System.Serializable]
    public class ZoneCamera
    {
        public string zoneID;
        public CinemachineVirtualCamera camera;
        public bool isActive = false;
    }
    
    private string currentZoneID = "";
    private bool inDialogue = false;
    
    private void Start()
    {
        // Включаем основную камеру
        EnableMainCam();
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
    
    public void SwitchToZone(string zoneID, CinemachineVirtualCamera zoneCam)
    {
        if (inDialogue) return; // Диалог важнее
        
        // Выключаем предыдущую зону
        if (!string.IsNullOrEmpty(currentZoneID))
        {
            foreach (var zc in zoneCameras)
            {
                if (zc.zoneID == currentZoneID && zc.camera != null)
                {
                    zc.camera.enabled = false;
                    zc.isActive = false;
                }
            }
        }
        
        // Включаем новую зону
        currentZoneID = zoneID;
        
        // Находим и включаем камеру
        bool found = false;
        foreach (var zc in zoneCameras)
        {
            if (zc.zoneID == zoneID)
            {
                if (zc.camera != null)
                {
                    zc.camera.enabled = true;
                    zc.isActive = true;
                    found = true;
                }
                break;
            }
        }
        
        // Если не нашли в списке, используем переданную камеру
        if (!found && zoneCam != null)
        {
            zoneCam.enabled = true;
            zoneCameras.Add(new ZoneCamera { zoneID = zoneID, camera = zoneCam, isActive = true });
        }
        
        // Выключаем основную камеру
        if (mainCam != null) mainCam.enabled = false;
    }
    
    public void ExitZone(string zoneID)
    {
        if (currentZoneID != zoneID) return;
        if (inDialogue) return;
        
        // Выключаем камеру зоны
        foreach (var zc in zoneCameras)
        {
            if (zc.zoneID == zoneID && zc.camera != null)
            {
                zc.camera.enabled = false;
                zc.isActive = false;
                break;
            }
        }
        
        currentZoneID = "";
        
        // Включаем основную камеру
        EnableMainCam();
    }
    
    private void OnDialogueStarted()
    {
        inDialogue = true;
        
        // Выключаем все зоны
        foreach (var zc in zoneCameras)
        {
            if (zc.camera != null)
                zc.camera.enabled = false;
        }
        
        // Включаем диалоговую камеру
        if (mainCam != null) mainCam.enabled = false;
        if (dialogueCam != null) dialogueCam.enabled = true;
    }
    
    private void OnDialogueEnded()
    {
        inDialogue = false;
        
        // Выключаем диалоговую камеру
        if (dialogueCam != null) dialogueCam.enabled = false;
        
        // Возвращаемся к зоне, если игрок все еще в ней
        if (!string.IsNullOrEmpty(currentZoneID))
        {
            foreach (var zc in zoneCameras)
            {
                if (zc.zoneID == currentZoneID && zc.camera != null)
                {
                    zc.camera.enabled = true;
                    if (mainCam != null) mainCam.enabled = false;
                    return;
                }
            }
        }
        
        // Иначе включаем основную камеру
        EnableMainCam();
    }
    
    private void EnableMainCam()
    {
        if (mainCam != null) mainCam.enabled = true;
        if (dialogueCam != null) dialogueCam.enabled = false;
    }
}
