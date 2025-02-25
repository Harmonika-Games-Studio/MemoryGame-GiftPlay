using UnityEngine;
using UnityEngine.Audio;
using System;
using Harmonika.Tools;

public class SoundSystem : MonoBehaviour
{
    public Sound[] sounds;

    public static SoundSystem Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            return;

        foreach (Sound sound in sounds)
        {
            sound.audioSource = gameObject.AddComponent<AudioSource>();
            sound.audioSource.loop = sound.loop;
            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch= sound.pitch;
        }
    }

    private void Start()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.playOnAwake)
                Play(sound.name);
        }
    }

    public void Play(string name, bool playOneShot = false)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (!s.Equals(default(Sound)))
        {
            if (!playOneShot)
                s.audioSource.Play();
            else
                s.audioSource.PlayOneShot(s.audioSource.clip);
        }
        else
            Debug.LogError("Audio" + name + " not found");
    }

    public void ChangeAudioVolumeByType(Harmonika.Tools.AudioType type, float newVolume)
    {
        foreach (Sound sound in sounds)
        {
            if (sound.type == type)
            { 
                sound.audioSource.volume = (newVolume / 100) * sound.volume;
            }
        }
    }

}
