using UnityEngine;
using UnityEngine.Rendering;
using System;

public class RealTimeSkybox : MonoBehaviour
{
    [System.Serializable]
    public struct TimePeriod
    {
        public string name;
        public int startHour;      // Начало периода (0-23)
        public int endHour;        // Конец периода (0-23)
        public Material skybox;    // Скайбокс для этого периода
        public VolumeProfile volume; // Global volume для этого периода
        public Color ambientLight;
    }
    
    [Header("Time Periods")]
    [SerializeField] private TimePeriod[] timePeriods;
    
    [Header("References")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Volume globalVolume;
    
    [Header("Settings")]
    [SerializeField] private bool useRealTime = true;
    [SerializeField] private float transitionSpeed = 1f; // Скорость перехода (если не используем реальное время)
    [SerializeField] private int debugHour = 12; // Для тестирования
    
    private int currentPeriodIndex = -1;
    private float virtualTimeOfDay = 12f; // Для ручного управления
    
    void Start()
    {
        // Применяем время с устройства
        UpdateTimeOfDay();
        
        // Обновляем каждую минуту
        InvokeRepeating(nameof(UpdateTimeOfDay), 60f, 60f);
    }
    
    void Update()
    {
        if (!useRealTime)
        {
            // Ручное управление временем (для тестирования)
            virtualTimeOfDay += Time.deltaTime * transitionSpeed / 3600f; // Медленно меняем
            if (virtualTimeOfDay >= 24f) virtualTimeOfDay = 0f;
            
            ApplyTimeOfDay(virtualTimeOfDay);
        }
    }
    
    void UpdateTimeOfDay()
    {
        if (!useRealTime) return;
        
        // Получаем текущее системное время
        DateTime now = DateTime.Now;
        float currentHour = now.Hour + (now.Minute / 60f);
        
        ApplyTimeOfDay(currentHour);
        
        Debug.Log($"System time: {now.Hour:00}:{now.Minute:00} - Applied hour: {currentHour:F1}");
    }
    
    void ApplyTimeOfDay(float hour)
    {
        // Находим подходящий период
        for (int i = 0; i < timePeriods.Length; i++)
        {
            var period = timePeriods[i];
            
            // Проверяем, входит ли час в период (учитываем переход через полночь)
            if (period.startHour <= period.endHour)
            {
                if (hour >= period.startHour && hour < period.endHour)
                {
                    SetPeriod(i);
                    break;
                }
            }
            else
            {
                // Период переходит через полночь (например, 22-4)
                if (hour >= period.startHour || hour < period.endHour)
                {
                    SetPeriod(i);
                    break;
                }
            }
        }
    }
    
    void SetPeriod(int index)
    {
        if (index == currentPeriodIndex) return;
        
        currentPeriodIndex = index;
        var period = timePeriods[index];
        
        // Меняем скайбокс
        if (period.skybox != null)
        {
            RenderSettings.skybox = period.skybox;
        }
        
        // Меняем global volume
        if (globalVolume != null && period.volume != null)
        {
            globalVolume.profile = period.volume;
        }
        
        // Меняем ambient light
        RenderSettings.ambientLight = period.ambientLight;
        
        // Поворачиваем солнце (примерное положение)
        if (sunLight != null)
        {
            float sunRotation = (period.startHour / 24f) * 360f;
            sunLight.transform.rotation = Quaternion.Euler(sunRotation, -30f, 0f);
        }
        
        Debug.Log($"Changed to period: {period.name} at hour {period.startHour}-{period.endHour}");
    }
    
    // Метод для тестирования в редакторе
    public void SetDebugHour(int hour)
    {
        useRealTime = false;
        virtualTimeOfDay = hour;
        ApplyTimeOfDay(hour);
    }
    
    public void EnableRealTime()
    {
        useRealTime = true;
        UpdateTimeOfDay();
    }
}
