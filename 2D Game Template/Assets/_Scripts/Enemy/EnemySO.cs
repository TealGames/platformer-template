using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "ScriptableObjects/Base Enemy Data")]
public class EnemySO : ScriptableObject
{
    [Header("General")]
    [Tooltip("The max amount of health when enemy spawns/activates")] public int maxEnemyHealth;
    [Tooltip("The min amount of health when enemy spawns/activates. Note: this value CANNOT be 0")] public int minEnemyHealth;
     public bool haveRandomHealthAmount;

    [Tooltip("The amount of time after the player has damaged the enemy that they can be damaged again")] public float invulnerableTime = 0.5f;
    [Tooltip("The amount of damage to do when the enemy collides with the player")] public int amountToDamage;
    [Tooltip("If true, contact damage will be the same as amountToDamage")] public bool doContactDamage;

    [Tooltip("The radius of the circle with center being alertCenter's position. When player enters this circle, the things that enemy does depends on the classes that inherit this class")]
     public float alertRange;

    [Tooltip("If true, will show the alert visual cue when player enters the alert circle")] public bool showAlertCue = false;
    [Tooltip("After this amount of time, the alert cue icon will disappear. If show alert cue is true and alert cue time is 0, it will always be shown when alerted")] public float alertCueTime;

     public LayerMask playerLayer;
}
