using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    [System.Serializable]
    public class TimePreset
    {
        public string name = "Day";
        public Material skybox;
        public VolumeProfile volumeProfile;
        public GameObject particleEffectPrefab;
        public float duration = 60f;
    }

    [Header("Time Presets")]
    [SerializeField] private TimePreset dayPreset;
    [SerializeField] private TimePreset nightPreset;
    
    [Header("Skybox Rotation")]
    [SerializeField] private float skyboxRotationSpeed = 1f;
    
    [Header("Settings")]
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private bool startWithDay = true;
    
    [Header("References")]
    [SerializeField] private Transform particleParent;
    [SerializeField] private Volume globalVolume; // Ссылка на Global Volume в сцене
    
    private bool isDay = true;
    private float timer = 0f;
    private float skyboxRotation = 0f;
    private GameObject currentParticleEffect;
    private Material currentSkybox;

    void Start()
    {
        if (particleParent == null)
            particleParent = transform;
        
        if (startOnAwake)
        {
            isDay = startWithDay;
            ApplyPreset(isDay);
        }
    }

    void Update()
    {
        // Вращаем скайбокс
        if (currentSkybox != null)
        {
            skyboxRotation += Time.deltaTime * skyboxRotationSpeed;
            if (skyboxRotation > 360f) skyboxRotation -= 360f;
            
            currentSkybox.SetFloat("_Rotation", skyboxRotation);
        }
        
        timer += Time.deltaTime;
        
        float currentDuration = isDay ? dayPreset.duration : nightPreset.duration;
        
        if (timer >= currentDuration)
        {
            ToggleTime();
        }
    }

    public void ToggleTime()
    {
        isDay = !isDay;
        ApplyPreset(isDay);
    }

    public void SetDay()
    {
        if (!isDay)
        {
            isDay = true;
            ApplyPreset(true);
        }
    }

    public void SetNight()
    {
        if (isDay)
        {
            isDay = false;
            ApplyPreset(false);
        }
    }

    void ApplyPreset(bool day)
    {
        TimePreset preset = day ? dayPreset : nightPreset;
        
        // Меняем скайбокс
        if (preset.skybox != null)
        {
            currentSkybox = preset.skybox;
            RenderSettings.skybox = currentSkybox;
        }
        
        // Меняем Global Volume (используем ссылку из сцены)
        if (globalVolume != null && preset.volumeProfile != null)
            globalVolume.profile = preset.volumeProfile;
        else
            Debug.LogWarning("Global Volume reference is missing or volume profile is null");
        
        // Удаляем старый эффект
        if (currentParticleEffect != null)
            Destroy(currentParticleEffect);
        
        // Спавним новый эффект
        if (preset.particleEffectPrefab != null)
        {
            currentParticleEffect = Instantiate(preset.particleEffectPrefab, particleParent);
            currentParticleEffect.transform.localPosition = Vector3.zero;
        }
        
        timer = 0f;
        
        Debug.Log($"DayNightCycle: Switched to {(day ? "DAY" : "NIGHT")}");
    }

    private void OnValidate()
    {
        if (dayPreset == null)
            dayPreset = new TimePreset { name = "Day", duration = 60f };
        
        if (nightPreset == null)
            nightPreset = new TimePreset { name = "Night", duration = 60f };
    }
}
