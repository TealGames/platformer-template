using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "ScriptableObjects/Weapon")]
public class WeaponSO : ScriptableObject
{
    public string weaponName;
    public int weaponDamage;
    public float attackRange;
    [Tooltip("The amount of time that the player can use this weapon again after using it")]public float cooldownTime;

    [Header("Sounds")]
    [Tooltip("The sound that this weapon makes when it does not hit anything")]public AudioClip[] swingSounds;
    [Tooltip("The sound that this weapon makes when it hits an enemy or boss")]public AudioClip[] hitSounds;
    [Tooltip("The sounds that this weapon makes when it hits an environmental object based on its material type")] public MaterialSounds[] materialSounds;
    [Tooltip("The sound that this weapon makes when it hits something that it is not supposed to hit (if null, then plays environment sound)")]public AudioClip deflectSound;

    

    

}
