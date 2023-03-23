using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0.0f, 1.0f)]
    public float volume;

    [Tooltip("In seconds")] public float clipDelay;

    public bool loop;

    public bool doPitch;

    [Range(0.1f, 3.0f)]
    public float pitch;

    public enum AudioType
    {
        Music,
        Ambience,
        SFX,
        Voice,
    }

    [Tooltip("Note: music and ambience are both effected by Music Group in AudioMixer")]public AudioType audioType;

    [HideInInspector]
    public AudioSource source;

}
