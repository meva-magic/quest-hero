using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class DayNightCycle : MonoBehaviour
{
    [System.Serializable]
    public class TimePreset
    {
        public string name = "Morning";
        public Material skybox;
        public VolumeProfile volumeProfile;
        public GameObject particleEffect; // Префаб эффекта (дождь, снег, туман и т.д.)
        public Color ambientLight = Color.white;
        public float lightIntensity = 1f;
    }
    
    [Header("Time Settings")]
    [SerializeField] private List<TimePreset> timePresets = new List<TimePreset>();
    [SerializeField] private float timeBetweenPresets = 60f; // Секунд между сменами
    
    [Header("References")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private Transform particleParent; // Куда создавать эффекты
    
    private int currentPresetIndex = 0;
    private float timer = 0f;
    private GameObject currentEffect = null;
    
    void Start()
    {
        if (timePresets.Count == 0)
        {
            Debug.LogWarning("No time presets assigned!");
            return;
        }
        
        // Применяем первый пресет
        ApplyPreset(currentPresetIndex);
    }
    
    void Update()
    {
        if (timePresets.Count == 0) return;
        
        timer += Time.deltaTime;
        
        if (timer >= timeBetweenPresets)
        {
            timer = 0f;
            NextPreset();
        }
    }
    
    void NextPreset()
    {
        currentPresetIndex++;
        if (currentPresetIndex >= timePresets.Count)
            currentPresetIndex = 0;
        
        ApplyPreset(currentPresetIndex);
        Debug.Log($"Time changed to: {timePresets[currentPresetIndex].name}");
    }
    
    void ApplyPreset(int index)
    {
        TimePreset preset = timePresets[index];
        
        // Смена скайбокса
        if (preset.skybox != null)
        {
            RenderSettings.skybox = preset.skybox;
        }
        
        // Смена ambient light
        RenderSettings.ambientLight = preset.ambientLight;
        
        // Смена Global Volume
        if (globalVolume != null && preset.volumeProfile != null)
        {
            globalVolume.profile = preset.volumeProfile;
        }
        
        // Смена освещения
        if (sunLight != null)
        {
            sunLight.intensity = preset.lightIntensity;
            
            // Простая смена угла солнца (по индексу)
            float angle = (index / (float)timePresets.Count) * 360f;
            sunLight.transform.rotation = Quaternion.Euler(angle, -30f, 0f);
        }
        
        // Смена партикл эффекта
        if (particleParent != null)
        {
            // Удаляем старый эффект
            if (currentEffect != null)
                Destroy(currentEffect);
            
            // Создаем новый
            if (preset.particleEffect != null)
            {
                currentEffect = Instantiate(preset.particleEffect, particleParent);
            }
        }
    }
    
    // Методы для ручного управления
    public void SetPreset(int index)
    {
        if (index >= 0 && index < timePresets.Count)
        {
            currentPresetIndex = index;
            ApplyPreset(index);
        }
    }
    
    public void NextPresetManual()
    {
        NextPreset();
    }
}
