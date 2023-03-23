using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script is the class that DialogueTrigger.cs uses to create Dialogue
 */


[System.Serializable]
public class Dialogue
{
    public string characterName;
    public AudioClip talkSound;

    [TextArea(3, 10)]
    public string[] sentences;
}

