
//Code from Trever Mock's video: https://www.youtube.com/watch?v=aUi9aijvpgs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//delayed execution order prevents DefaultInventory() to be null
[DefaultExecutionOrder(10000)] [System.Serializable]
public class GameData
{
    //binary representation of when this instance of GameData was last updated
    public long lastUpdated;

    public int currencyQuantity;
    public Vector3 playerPosition;

    //SerializableDictionary used because Json cannot serialize dictionaries, so we separate keys and values into lists
    public SerializableDictionary<string, bool> collectiblesCollected; //stores all the coins as a string ID, and a bool if it was collected to determine if collectible should physically appear in game

    public SerializableDictionary<string, int> inventoryItems;  //stores the player's inventory items and quanity used in the inventory UI

    //values defined in this constructor are the default values that the game starts with
    //when there is no data to load
    public GameData()
    {
        this.currencyQuantity = 0;
        playerPosition= Vector3.zero;
        collectiblesCollected = new SerializableDictionary<string, bool>();

        //adds each inventory item type and sets its value to be 0
        inventoryItems = new SerializableDictionary<string, int>();

        foreach (var inventoryItem in Enum.GetNames(typeof(PlayerCharacter.InventoryItemTypes)))
        {
            if (inventoryItem == PlayerCharacter.InventoryItemTypes.None.ToString()) continue;
            inventoryItems.Add(inventoryItem.ToString(), 0);
        }
    }

    public int GetPrecentageComplete()
    {
        UnityEngine.Debug.Log("The precentage completed is currently not being calculated, so a placeholder value of 0 is returned");
        return 0;
    }
}
