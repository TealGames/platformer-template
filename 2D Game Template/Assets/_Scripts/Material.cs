using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this script to any object that a weapon can collide with and the weapon will make a sound based on the type of material specified here
/// when it collides with this object. Note: this object must have a collider on it that is NOT a trigger
/// </summary>

public class Material: MonoBehaviour
{
    public enum MaterialType
    {
        None,
        Wood,
        Stone,
        Glass,
        Metal,
        Cloth,
    }

    [SerializeField] private MaterialType materialType;

    public MaterialType GetMaterialType() => materialType;
}
