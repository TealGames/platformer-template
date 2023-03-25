using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Create another instance of this scriptable object when you want to create a predefined inventory setup for the player for testing. 
/// This should ONLY BE USED FOR TESTING PURPOSES. No other functionalities are applicable to this scriptable object.
/// </summary>

[System.Serializable]
public class InventoryItem
{
    [Tooltip("Note: NONE cannot be selected and do not create duplicate items with the same item type")] public PlayerCharacter.InventoryItemTypes itemType;
    public int itemQuantity;
}

[CreateAssetMenu(fileName = "InventorySetupSO", menuName = "ScriptableObjects/Inventory Setup")]
public class InventorySetupSO : ScriptableObject
{
    [Tooltip("Create predefined inventory setups with this array")] public InventoryItem[] setInventoryItems;
}



