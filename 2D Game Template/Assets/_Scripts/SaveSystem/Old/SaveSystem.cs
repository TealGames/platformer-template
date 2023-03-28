using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
//static classes only have one instance of it without creating a singleton
{
    public static void SavePlayer(PlayerCharacter playerData)
    {
        //create a path (location on computer) to store info and a BinaryFormatter which will convert it to binary so the data cannot easily be changed
        //BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.fun";

        //create new new file for to store the data
        FileStream stream = new FileStream(path, FileMode.Create);

        //create new instance of saved player data and add info to created file with binary
        SaveData saveData = new SaveData(playerData);

        //formatter.Serialize(stream, saveData);
        stream.Close();
    }

    public static SaveData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/player.fun";
        if (File.Exists(path))
        {
            //open the file with the specified path
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            //convert the binary into data and convert it into a SaveData type
            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            return data;
        }
        else
        {
            UnityEngine.Debug.LogError($"Save file not found in {path}");
            return null;
        }


    }

}