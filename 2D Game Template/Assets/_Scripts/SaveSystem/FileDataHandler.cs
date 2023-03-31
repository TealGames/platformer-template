
//Code from Trever Mock's video: https://www.youtube.com/watch?v=aUi9aijvpgs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// A separate class that deals with saving specific profile's data into a JSON and storing into a folder. Likewise, it loads data from JSON. This class is used by the DataPersistenceManager and separates the data
/// storage type from the actual process of loading and saving data in game. This only works for downloadable games, and NOT for WebGL. For WebGL, replace this with a cloud storage script that stores data to the cloud
/// </summary>

public class FileDataHandler
{
    private string dataDirectoryPath = "";
    private string dataFileName = "";

    private readonly string backupExtension = ".bak";

    public FileDataHandler(string dataDirectoryPath, string dataFileName)
    {
        this.dataDirectoryPath = dataDirectoryPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load(string profileId, bool allowRestoreFromBackup= true)
    {
        //base case= if profileId is null return right away since null profile has no data anyways
        if (profileId == null) return null;

        //use Path.Combine() to account for different Operating Systems have different path separators
        string fullPath = Path.Combine(dataDirectoryPath, profileId, dataFileName);
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
                //since we are calling Load() recursively, we need to account for the case where the rollback succeeds,
                //but data is still failing to load for some other reason, which without this check may cause an infinite recursive loop
                if (allowRestoreFromBackup)
                {
                    UnityEngine.Debug.LogWarning($"Failed to load data. Attempting to roll back. \n {e}");
                    bool rollbackSuccess = AttemptRollback(fullPath);
                    if (rollbackSuccess)
                    {
                        //try to load again recursively (if failed before, should not fail again)
                        loadedData = Load(profileId, false);
                    }
                }
                //if we hit this else block one posibility is that backup file is also corrupted
                else UnityEngine.Debug.LogError($"Error occured when trying to load file at path: {fullPath} and backup did not work. \n {e}");
                
            }
        
        }
        return loadedData;

    }

    public void Save(GameData gameData, string profileId)
    {
        //base case= if profileId is null return right away since null profile has no data anyways
        if (profileId == null) return;

        //use Path.Combine() to account for different Operating Systems have different path separators
        string fullPath = Path.Combine(dataDirectoryPath, profileId, dataFileName);
        string backupFilePath= fullPath + backupExtension;
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

            //verify that the newly saved file can be loaded sucessfully (isn't corrupted)
            GameData verifiedGameData = Load(profileId);

            //if the data can be verified, back it up
            if (verifiedGameData != null) File.Copy(fullPath, backupFilePath, true);
            else throw new Exception("Save file could not be verified and backup could not be created");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error occured when trying to save data to file {fullPath}\n {e}");
        }
    }

    public void Delete(string profileId)
    {
        if (profileId == null) return;
        string fullPath = Path.Combine(dataDirectoryPath, profileId, dataFileName);

        try
        {
            //delete the profile folder if data file exists in the path
            if (File.Exists(fullPath)) Directory.Delete(Path.GetDirectoryName(fullPath), true);
            else UnityEngine.Debug.LogWarning($"Tried to delete profile data at path: {fullPath} but data was not found!");
        }
        catch(Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to delete profile data for profile id: {profileId} at path: {fullPath} \n {e}");
        }
    }

    public Dictionary<string, GameData> LoadAllProfiles()
    {
        Dictionary<string, GameData> profileDictionary = new Dictionary<string, GameData>();

        IEnumerable<DirectoryInfo> directoryInfos = new DirectoryInfo(dataDirectoryPath).EnumerateDirectories();
        foreach (var directoryInfo in directoryInfos)
        {
            string profileId = directoryInfo.Name;

            string fullPath= Path.Combine(dataDirectoryPath, profileId, dataFileName);
            if (!File.Exists(fullPath))
            {
                UnityEngine.Debug.LogWarning($"Skipped directory {profileId} when loading all profiles because it does not contain data");
                continue;
            }

            //load the data for this profile and put it into the directory
            GameData profileData = Load(profileId);

            if (profileData != null) profileDictionary.Add(profileId, profileData);
            else UnityEngine.Debug.LogError($"Tried to load profile but something went wrong. Profile id: {profileId}");
        }
        return profileDictionary;
    }

    public string GetMostRecentlyUpdatedProfileId()
    {

        string mostRecentProfileId = null;

        Dictionary<string, GameData> profilesGameData = LoadAllProfiles();
        foreach (KeyValuePair<string, GameData> pair in profilesGameData)
        {
            string profileId= pair.Key;
            GameData gameData= pair.Value;

            if (gameData == null) continue;

            //if this is the first data we've come across that exists, its the most recent so far
            if (mostRecentProfileId==null) mostRecentProfileId = profileId;
            else
            {
                //otherwise compare to see which date is the most recent
                DateTime mostRecentDateTime = DateTime.FromBinary(profilesGameData[mostRecentProfileId].lastUpdated);
                DateTime newDateTime = DateTime.FromBinary(gameData.lastUpdated);

                //the greatest DateTime value is the most recent
                if (newDateTime> mostRecentDateTime) mostRecentProfileId= profileId;
            }
        }

        return mostRecentProfileId;
    }

    //roll back means to return back to a previous state- so in this case, we are returning normal file to the backup's data
    private bool AttemptRollback(string fullPath)
    {
        bool isSucessful = false;
        string backupFilePath = fullPath + backupExtension;
        try
        {
            //if file exists, attempt to roll back to it by overwriting the original file
            if (File.Exists(backupFilePath))
            {
                File.Copy(backupFilePath, fullPath, true);
                isSucessful = true;
                UnityEngine.Debug.LogWarning($"Had to roll back to backup file at {backupFilePath}");
            }

            else throw new Exception("Tried to roll back original file to backup file, but no backup file exists to roll back to");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error occured when trying to roll back to backup file at {backupFilePath} \n {e}");
        }

        return isSucessful;
    }

}
