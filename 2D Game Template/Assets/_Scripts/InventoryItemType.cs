using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used by the inventory system to identify which cell item corresponds to which inventory item type from enum
/// </summary>

public class InventoryItemType : MonoBehaviour
{
    [Tooltip("The type of the inventory item (defined in playercharacter) used by the Inventory.cs script to find object image cell with the same item in dictionary and have that object's quantity and gameobject show")]
    [SerializeField] private PlayerCharacter.InventoryItemTypes inventoryItemType;
    [Tooltip("The item description data when this item is selected in the inventory")][SerializeField] private ObjectTextSO itemDescription;

    public PlayerCharacter.InventoryItemTypes GetInventoryItemType() => inventoryItemType;

    public ObjectTextSO GetItemDescription() => itemDescription;
}
