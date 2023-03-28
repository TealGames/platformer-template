using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class GameData
{
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
        inventoryItems= PlayerCharacter.Instance.DefaultInventory();
    }


}
