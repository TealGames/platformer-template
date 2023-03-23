using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serialized Class for WeaponSO scriptable object to use to store an array of the corresponding sounds of each type of material
/// </summary>

[System.Serializable]
public class MaterialSounds
{
    public Material.MaterialType materialType;
    [Tooltip("When this weapon collides with the object with a material of the type specified in materialType, it makes this sound")] public AudioClip materialSound;
}
