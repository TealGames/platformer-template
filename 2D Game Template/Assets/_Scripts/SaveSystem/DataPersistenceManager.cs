
//Code from Trever Mock's video: https://www.youtube.com/watch?v=aUi9aijvpgs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the game's save and load system and sends game data to the file handler when saving and receives data from file handler when loading. 
/// Call SaveGame() in another script when you want to save the game and call LoadGame() in another script when you want to load game data
/// </summary>

//prevents OnEnable() GameManager event to be null
[DefaultExecutionOrder(10000)]
public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager Instance { get; private set; }
    private List<IDataPersistence> dataPersistenceObjects;

    private FileDataHandler dataHandler;

    private GameData gameData;

    private string selectedProfileId = "";

    [Header("File Storage Config")]
    [Tooltip("The name of the JSON file that game data gets stored to. To find the game's save files search %appdata% in window Search bar -> where it says {profileName} > AppData > Roaming SELECT AppData" +
        " -> LocalLow -> DefaultCompany -> {name of this Unity Project} and you should see file(s) with the name being the profile Id choosen in the saveSlot script attached to the saveSlots")]
    [SerializeField] private string fileName;


    [Header("Saving Config")]

    [Tooltip("If true, will save the game after the time specified in autoSaveTimeSeconds")][SerializeField] private bool doAudoSaving = false;
    [Tooltip("The amount of time that the game will autosave if true, IN SECONDS")][SerializeField] private float autoSaveTimeSeconds = 60f;
    private Coroutine autoSaveCoroutine;

    [Header("Debugging")]
    [Tooltip("A new save file is automatically created if true. This should only be used in debugging purposes and when not saving data the normal way")]
    [SerializeField] private bool initializeDataIfNull = false;

    [Tooltip("If true, this script will NOT save and load data when it is called to do so, such as loading during during new scene loads/selected profile changes and does not save during scene changes or when player leaves")]
    [SerializeField] private bool disableDataPersistence = false;

    [Tooltip("If true, will ignore the current profileId (meaning that the save file that corresponds with the CONTINUE button in Main Menu will be overriden) with the testSelectedProfileId")]
    [SerializeField] private bool overrideSelectedProfileId = false;

    [Tooltip("If overrideSelectedProfileId is true, will override it with this profileId")]
    [SerializeField] private string testSelectedProfileId = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        if (disableDataPersistence) UnityEngine.Debug.LogWarning("Data Persistence is currently disabled!");

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        InitializeSelectedProfileId();

        //if we return back to main menu, we save the game
        HUD.Instance.GetComponentInChildren<PauseMenu>().OnReturnToMainMenu += SaveGame;
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        //instead of calling save game when scene unloads, we subscribe to an event triggered BEFORE
        //scene load happens in LoadSceneAsync() in GameManager
        GameManager.Instance.OnSceneLoadStarted += SaveGame;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameManager.Instance.OnSceneLoadStarted -= SaveGame;
    }


    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();

        if (doAudoSaving)
        {
            //prevent multiple coroutines running
            if (autoSaveCoroutine != null) StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = StartCoroutine(AutoSave());
        }
        

    }

    public void ChangeSelectedProfileId(string newProfileId)
    {
        this.selectedProfileId = newProfileId;
        //load the game which will use that profile, updating our game data accordingly
        LoadGame();
    }

    public void DeleteProfileData(string profileId)
    {
        dataHandler.Delete(profileId);
        InitializeSelectedProfileId();

        //reload the game so that our data matches the newly selected profile id
        LoadGame();
    }

    private void InitializeSelectedProfileId()
    {
        this.selectedProfileId = dataHandler.GetMostRecentlyUpdatedProfileId();

        if (overrideSelectedProfileId)
        {
            if (testSelectedProfileId.Equals("")) selectedProfileId = testSelectedProfileId;
            else UnityEngine.Debug.LogWarning("Override selected Profile ID is selected but the profile ID to test is empty!");
        }
    }

    public void NewGame()=> this.gameData = new GameData();

    public void LoadGame()
    {
        //return right away if data persistence is disabled
        if (disableDataPersistence) return;

        //load any saved data from a file using the data handler
        this.gameData = dataHandler.Load(selectedProfileId);

        if (this.gameData == null && initializeDataIfNull) NewGame();

        //if no data can be loaded, don't continue
        if (this.gameData == null)
        {
            UnityEngine.Debug.Log("No data to load was found! A new game needs to be started before data can be loaded");
            return;
        }

        //push the loaded data to each script that needs it
        foreach (var dataPersistenceObj in dataPersistenceObjects) dataPersistenceObj.LoadData(gameData);
    }

    public void SaveGame()
    {
        //return right away if data persistence is disabled
        if (disableDataPersistence) return;

        if (this.gameData ==null)
        {
            UnityEngine.Debug.LogWarning("No data to load was found! A new game needs to be started before data can be saved");
            return;
        }
            
        //pass the data to other scripts so they can update it
        foreach (var dataPersistenceObj in dataPersistenceObjects) dataPersistenceObj.SaveData(gameData);

        //timestap the data so we know when it was last saved
        gameData.lastUpdated = System.DateTime.Now.ToBinary();

        //save the data to a file using the data handler
        dataHandler.Save(gameData, selectedProfileId);
    }

    //placeholder
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true).
            OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public bool HasGameData() => gameData != null;

    public Dictionary<string, GameData> GetAllProfilesGameData() => dataHandler.LoadAllProfiles();

    private IEnumerator AutoSave()
    {
        while(true)
        {
            yield return new WaitForSeconds(autoSaveTimeSeconds);
            SaveGame();
            UnityEngine.Debug.Log("Auto saved game");
        }
    }
}
