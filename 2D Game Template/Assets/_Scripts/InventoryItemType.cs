using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemType : MonoBehaviour
{
    [Tooltip("The type of the inventory item (defined in playercharacter) used by the Inventory.cs script to find object image cell with the same item in dictionary and have that object's quantity and gameobject show")]
    [SerializeField] private PlayerCharacter.InventoryItemTypes inventoryItemType;

    public PlayerCharacter.InventoryItemTypes GetInventoryItemType() => inventoryItemType;

}
