using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName= "LevelSO", menuName= "ScriptableObjects/Level")]
public class LevelSO: ScriptableObject
{
    public string levelName;
    public TMP_ColorGradient textColor;
    public string sceneName;

    public string levelMusic;
    public string levelAmbience;
    public string[] levelSFX;

    [Tooltip("When transitioning to this level, whether or not fade all currently playing sounds in Audio Manager")] public bool fadeSounds = true;

    [TextAreaAttribute] public string levelDescription;
}
