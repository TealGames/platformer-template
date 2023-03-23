using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Base class of the types of fields to store when saving the game. Each save is a new instance of this class using the constructor
/// </summary>

[System.Serializable]
public class SaveData
{
    public string levelName;
    public int health;
    public float[] position;

    public int currency;




    public SaveData(PlayerCharacter playerData)
    {
        levelName = SceneManager.GetActiveScene().name;
        //health = PlayerCharacter.instance.maxHealth;

        position[0] = playerData.transform.position.x;
        position[1] = playerData.transform.position.y;
        position[2] = playerData.transform.position.z;

        //currency = playerData.currentCurrency;
    }

}