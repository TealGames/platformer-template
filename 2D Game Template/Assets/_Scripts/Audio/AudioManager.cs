using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Diagnostics;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] public AudioMixerGroup[] audioMixerGroups { get; private set; }

    [SerializeField] [Tooltip("Writes a debug log to console when a sound is playing")] private bool displayDebugsWhenPlays;
    //[SerializeField] [Tooltip("When a mixer group volume is faded (whether lower or higher) the time after it fades when it returns back to normal")] private float fadeBackToNormalVolumeDuration = 1.0f;

    [SerializeField] public Sound[] sounds;
    private List<string> musicNames;
    private List<string> ambienceList;
    private List<string> SFXNames;
    private List<string> voiceNames;

    public Action OnAudioFadeIn;
    public Action OnAudioFadeOut;
    
    public static AudioManager Instance;


    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        audioMixerGroups = audioMixer.FindMatchingGroups("Master");
        //this takes all the child groups of "Master" and places them in a Audio Mixer Group array

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;

            if (s.doPitch)
            {
                s.source.pitch = s.pitch;
            }

            s.source.loop = s.loop;

            if (s.audioType == Sound.AudioType.Music)
            {
                s.source.outputAudioMixerGroup = audioMixerGroups[1];
                //starts with 1 because the the "Master" group is included as  mixer group 1 (index 0)
            }

            else if (s.audioType == Sound.AudioType.Ambience)
                //Ambience is considered the same as Music since just background looping noise
            {
                s.source.outputAudioMixerGroup = audioMixerGroups[1];

            }

            else if (s.audioType== Sound.AudioType.SFX)
            {
                s.source.outputAudioMixerGroup = audioMixerGroups[2];

            }

            else if (s.audioType == Sound.AudioType.Voice)
            {
                s.source.outputAudioMixerGroup = audioMixerGroups[3];

            }

            
        }
    }

    void Update()
    {
        foreach (var sound in sounds)
        {
            //if (sound.source.isPlaying && displayDebugsWhenPlays) UnityEngine.Debug.Log($"{sound.name}'s audio source is playing!");
            if (sound.volume!= sound.source.volume) sound.source.volume= sound.volume;
        }
    }


    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            UnityEngine.Debug.Log("Play() Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void Play(string name, float delay)
    //an overloaded play method with the delay parameter
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            UnityEngine.Debug.Log("Play() Sound (delayed version): " + name + " not found!");
            return;
        }
        s.source.PlayDelayed(delay);
    }

    public void PlayOneShot(string name)
        //plays from sound[]
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            UnityEngine.Debug.Log("PlayOneShot() Sound: " + name + " not found!");
            return;
        }
        s.source.PlayOneShot(s.clip);
    }

    public void SetVolume(string name, float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            UnityEngine.Debug.Log("PlayOneShot() Sound: " + name + " not found!");
            return;
        }
        s.volume = volume;
    }

    public void StopPlaying(string sound, float fadeDuration, float endVolume)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            UnityEngine.Debug.Log("StopPlaying() Sound: " + name + " not found!");
            return;
        }
        if (s.audioType== Sound.AudioType.Music || s.audioType== Sound.AudioType.Voice) StartCoroutine(FadeMixerGroupVolume("Music", fadeDuration, endVolume));
        s.source.Stop();

        StartCoroutine(FadeMixerGroupVolume("Music", fadeDuration, 1.0f));
    }

    public void StopPlaying(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            UnityEngine.Debug.Log("StopPlaying() Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    public void StopPlayingAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
    }
    public Dictionary<string, float> GetPlayingAudioVolumes()
    {
        Dictionary<string, float> currentPlayingAudio = new Dictionary<string, float>();
        foreach (var s in sounds)
        {
            if (s.source.isPlaying) currentPlayingAudio.Add(s.name, s.volume);
        }
        if (currentPlayingAudio.Count > 0) return currentPlayingAudio;
        else return null;
    }

    public IEnumerator FadeSounds(float targetVolumeMultiplier, float fadeDuration)
        //since different sounds can have different default volumes, we multiply by the multiplier to determine new volume
    {
        foreach (Sound s in sounds)
        {
            if (s.source.isPlaying)
            {
                float currentTime = 0;
                float currentVolume = s.volume;
                float targetValue = Mathf.Clamp(targetVolumeMultiplier*currentVolume, 0f, 1f);
                UnityEngine.Debug.Log(targetValue);

                while (currentTime < fadeDuration)
                {
                    currentTime += Time.deltaTime;
                    float newVolume = Mathf.Lerp(currentVolume, targetValue, currentTime / fadeDuration);
                    //Mathf.Lerp() takes first paramter (min) and second paramter (max) and finds point on a linear graph based on the third paramter
                    //Note: third paramter is between 0 and 1 and is a precentage to take of the mix and max

                    s.volume = newVolume;
                    if (s.volume==0f) s.source.Stop();
                    yield return null;
                }
            }
        }
        yield break;
    }

    public IEnumerator FadeSounds(float targetVolumeMultiplier, float fadeDuration, float returnNormalDuration)
        //after done fading, will immediately begin to transition to fading back to normal volume
    {
        int index = 0;
        foreach (Sound s in sounds)
        {
            if (s.source.isPlaying)
            {
                float currentTime = 0;
                float currentVolume = s.volume;
                float targetValue = Mathf.Clamp(targetVolumeMultiplier*currentVolume, 0f, 1f);
                UnityEngine.Debug.Log(targetValue);

                while (currentTime < fadeDuration)
                {
                    currentTime += Time.deltaTime;
                    float newVolume = Mathf.Lerp(currentVolume, targetValue, currentTime / fadeDuration);
                    //Mathf.Lerp() takes first paramter (min) and second paramter (max) and finds point on a linear graph based on the third paramter
                    //Note: third paramter is between 0 and 1 and is a precentage to take of the mix and max

                    s.volume = newVolume;
                    
                    yield return null;
                }
                if (s.volume==0f) s.source.Stop();
                FadeSound(s.name, currentVolume, returnNormalDuration);
            }
        }
        yield break;
    }

    public IEnumerator FadeSound(string name, float targetEndVolume, float fadeDuration)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null)
        {
            float currentTime = 0;
            float currentVolume = s.volume;
            float targetValue = Mathf.Clamp(targetEndVolume, 0f, 1f);

            while (currentTime < fadeDuration)
            {
                currentTime += Time.deltaTime;
                float newVolume = Mathf.Lerp(currentVolume, targetValue, currentTime / fadeDuration);
                //Mathf.Lerp() takes first paramter (min) and second paramter (max) and finds point on a linear graph based on the third paramter
                //Note: third paramter is between 0 and 1 and is a precentage to take of the mix and max

                s.volume = newVolume;
                if (Mathf.Approximately(s.volume, 0f)) s.source.Stop();
                yield return null;
            }
            yield break;
        }
        else UnityEngine.Debug.Log($"Fade sound argument by the name {name} was not found!");
    }

    public IEnumerator FadeIn(float fadeDuration)
    {
        int index = 0;
        foreach (Sound s in sounds)
        {
            if (s.source.isPlaying)
            {
                float targetValue = s.volume;
                s.volume = 0f;
                float currentTime = 0;

                while (currentTime < fadeDuration)
                {
                    currentTime += Time.deltaTime;
                    float newVolume = Mathf.Lerp(0f, targetValue, currentTime / fadeDuration);
                    //Mathf.Lerp() takes first paramter (min) and second paramter (max) and finds point on a linear graph based on the third paramter
                    //Note: third paramter is between 0 and 1 and is a precentage to take of the mix and max

                    s.volume = newVolume;

                    yield return null;
                }
            }
        }
        OnAudioFadeIn?.Invoke();
        yield break;
    }

    public IEnumerator FadeOut(float fadeDuration)
    {
        int index = 0;
        foreach (Sound s in sounds)
        {
            if (s.source.isPlaying)
            {
                float startVolume= s.volume;
                float currentTime = 0;

                while (currentTime < fadeDuration)
                {    
                    currentTime += Time.deltaTime;
                    float newVolume = Mathf.Lerp(startVolume, 0f, currentTime / fadeDuration);
                    //Mathf.Lerp() takes first paramter (min) and second paramter (max) and finds point on a linear graph based on the third paramter
                    //Note: third paramter is between 0 and 1 and is a precentage to take of the mix and max

                    s.volume = newVolume;

                    yield return null;
                }
                s.source.Stop();
                s.volume = startVolume;
            }
        }
        OnAudioFadeOut?.Invoke();
        yield break;
    }


    public IEnumerator FadeMixerGroupVolume(string mixerGroupName, float fadeDuration, float targetEndVolume)
    //fadeDuration: time it takes to fade the volume to the target volume, returnToDefaultDuration: after faded, how long it takes for it to return to normal
    #region Declaration
    {
        string groupName = "MIXER_" + mixerGroupName.ToUpper() + "_VOLUME";

        float currentTime = 0;
        audioMixer.GetFloat(groupName, out float currentVolume);

        currentVolume = Mathf.Pow(10, currentVolume / 20);
        float targetValue = Mathf.Clamp(targetEndVolume, 0.0001f, 1f);
        //note: second parameter is 0.0001 because if its 0, then audioMixer breaks because it works logarithmically.

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(currentVolume, targetValue, currentTime / fadeDuration);
            //Mathf.Lerp() takes first paramter (min) and second paramter (max) and finds point on a linear graph based on the third paramter
            //Note: third paramter is between 0 and 1 and is a precentage to take of the mix and max

            audioMixer.SetFloat(groupName, Mathf.Log10(newVolume) * 20);
            //audioMixer works logarithmically so the volume needs to be converted appropriately from a linear graph to a logarithm

            //Db -> 0-1 volume (Mathf.Pow)
            //0-1 volume -> Db (Mathf.Log)

            yield return null;
        }
        yield break;
    }
    #endregion

    public IEnumerator FadeMixerGroupVolume(string mixerGroupName, float fadeDuration, float targetEndVolume, float returnToDefaultDuration)
    //fadeDuration: time it takes to fade the volume to the target volume, returnToDefaultDuration: after faded, how long it takes for it to return to normal
    #region Declaration
    {
        string groupName = "MIXER_" + mixerGroupName.ToUpper() + "_VOLUME";

        float currentTime = 0;
        audioMixer.GetFloat(groupName, out float currentVolume);

        currentVolume = Mathf.Pow(10, currentVolume / 20);
        float targetValue = Mathf.Clamp(targetEndVolume, 0.0001f, 1f);
        //note: second parameter is 0.0001 because if its 0, then audioMixer breaks because it works logarithmically.

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(currentVolume, targetValue, currentTime / fadeDuration);
            //Mathf.Lerp() takes first paramter (min) and second paramter (max) and finds point on a linear graph based on the third paramter
            //Note: third paramter is between 0 and 1 and is a precentage to take of the mix and max

            audioMixer.SetFloat(groupName, Mathf.Log10(newVolume) * 20);
            //audioMixer works logarithmically so the volume needs to be converted appropriately from a linear graph to a logarithm

            //Db -> 0-1 volume (Mathf.Pow)
            //0-1 volume -> Db (Mathf.Log)

            yield return null;
        }
        if (Mathf.Approximately(targetEndVolume, 0.0f)) StopPlayingAllSounds();
        StartCoroutine(FadeMixerGroupVolume(mixerGroupName, returnToDefaultDuration, Mathf.Pow(10, currentVolume / 20)));
        yield break;
    }
    #endregion

    public IEnumerator FadeMixerGroupVolume(FadeSettings fadeSettings)
    //fadeDuration: time it takes to fade the volume to the target volume, returnToDefaultDuration: after faded, how long it takes for it to return to normal
    #region Declaration
    {
        string groupName = "MIXER_" + fadeSettings.mixerGroupName.ToUpper() + "_VOLUME";

        float currentTime = 0;
        audioMixer.GetFloat(groupName, out float currentVolume);

        currentVolume = Mathf.Pow(10, currentVolume / 20);
        float targetValue = Mathf.Clamp(fadeSettings.targetEndVolume, 0.0001f, 1f);
        //note: second parameter is 0.0001 because if its 0, then audioMixer breaks because it works logarithmically.

        while (currentTime < fadeSettings.fadeDuration)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(currentVolume, targetValue, currentTime / fadeSettings.fadeDuration);
            //Mathf.Lerp() takes first paramter (min) and second paramter (max) and finds point on a linear graph based on the third paramter
            //Note: third paramter is between 0 and 1 and is a precentage to take of the mix and max

            audioMixer.SetFloat(groupName, Mathf.Log10(newVolume) * 20);
            //audioMixer works logarithmically so the volume needs to be converted appropriately from a linear graph to a logarithm

            //Db -> 0-1 volume (Mathf.Pow)
            //0-1 volume -> Db (Mathf.Log)

            yield return null;
        }
        if (Mathf.Approximately(fadeSettings.targetEndVolume, 0.0f) && fadeSettings.returnToDefaultVolume!=null) StopPlayingAllSounds();
        if (fadeSettings.returnToDefaultVolume != null&& fadeSettings.returnToDefaultDuration!=null) 
            StartCoroutine(FadeMixerGroupVolume(fadeSettings.mixerGroupName, fadeSettings.returnToDefaultDuration, Mathf.Pow(10, currentVolume / 20)));
        yield break;
    }
    #endregion

    public IEnumerator SetMixerGroupVolume(string mixerGroupName, float volume, float duration, bool fadeBackToNormalVolume)
    #region Declaration
    {
        string groupName = "MIXER_" + mixerGroupName.ToUpper() + "_VOLUME";

        float clampedVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat(groupName, Mathf.Log10(clampedVolume) * 20);
        yield return new WaitForSeconds(duration);

        if (fadeBackToNormalVolume)
        {
            StartCoroutine(FadeMixerGroupVolume(mixerGroupName, 0.5f, 0.5f));
        }
    }
    #endregion



}
