using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] sounds;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string sound)
    {
        if (sounds == null)
        {
            Debug.LogWarning("AudioManager: Sounds array is null");
            return;
        }
        
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null || s.source == null)
        {
            Debug.LogWarning($"AudioManager: Sound '{sound}' not found or source is null");
            return;
        }
        s.source.Play();
    }
    
    public void Stop(string sound)
    {
        if (sounds == null) return;
        
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null || s.source == null) return;
        s.source.Stop();
    }
}
