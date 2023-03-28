using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string dataDirectoryPath = "";
    private string dataFileName = "";

    public FileDataHandler(string dataDirectoryPath, string dataFileName)
    {
        this.dataDirectoryPath = dataDirectoryPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load()
    {
        //use Path.Combine() to account for different Operating Systems have different path separators
        string fullPath = Path.Combine(dataDirectoryPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader= new StreamReader(stream)) dataToLoad = reader.ReadToEnd();
                }

                //deserialize the JSON back into c# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError($"Error occured when trying to load data to file {fullPath}\n {e}");
            }
        
        }
        return loadedData;

    }

    public void Save(GameData gameData)
    {
        //use Path.Combine() to account for different Operating Systems have different path separators
        string fullPath = Path.Combine(dataDirectoryPath, dataFileName);
        try
        {
            //create the directory the file will be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            //Serialize C# game data object into JSON
            string dataToStore = JsonUtility.ToJson(gameData, true);

            //write the serialized data into file ("using" insures we close stream after usage is finished)
            using (FileStream stream= new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream)) writer.WriteLine(dataToStore);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error occured when trying to save data to file {fullPath}\n {e}");
        }
    }

}
