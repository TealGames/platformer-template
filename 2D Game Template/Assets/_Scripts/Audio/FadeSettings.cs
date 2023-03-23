using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] 
public class FadeSettings
{
    public string mixerGroupName;
    [Tooltip("Time it takes to fade the volume to the target volume")] public float fadeDuration;
    public float targetEndVolume;
    [Tooltip("If true, will return to default volume after done fading")] public bool returnToDefaultVolume;
    [Tooltip("After done fading, how long it takes for it to return to normal volume")] public float returnToDefaultDuration;

    
    FadeSettings(string mixerGroupName, float fadeDuration, float targetEndVolume)
    {
        this.mixerGroupName = mixerGroupName;
        this.fadeDuration = fadeDuration;
        this.targetEndVolume = targetEndVolume;
    }

    FadeSettings(string mixerGroupName, float fadeDuration, float targetEndVolume, float returnToDefaultDuration)
    {
        this.mixerGroupName = mixerGroupName;
        this.fadeDuration = fadeDuration;
        this.targetEndVolume = targetEndVolume;
        this.returnToDefaultDuration= returnToDefaultDuration;
    }

}

