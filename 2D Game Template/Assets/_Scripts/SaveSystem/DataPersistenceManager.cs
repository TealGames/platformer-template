using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the game's save and load system and sends game data to the file handler when saving and receives data from file handler when loading. 
/// Call SaveGame() in another script when you want to save the game and call LoadGame() in another script when you want to load game data
/// </summary>

public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager Instance { get; private set; }
    private List<IDataPersistence> dataPersistenceObjects;

    private FileDataHandler dataHandler;

    private GameData gameData;

    [Header("File Storage Config")]
    [Tooltip("The name of the JSON file that game data gets stored to")][SerializeField] private string fileName;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
    }

    //subscribe to scene loading/unloading events before Start()
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }


    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();

        //placeholder 
        LoadGame();
    }

    public void OnSceneUnloaded(Scene scene)
    {
        SaveGame();
    }


    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        //load any saved data from a file using the data handler
        this.gameData = dataHandler.Load();

        //if no data can be loaded, initialize to new game
        if (this.gameData == null)
        {
            UnityEngine.Debug.Log("No data to load was found! Initializing to default values.");
            NewGame();
        }

        //push the loaded data to each script that needs it
        foreach (var dataPersistenceObj in dataPersistenceObjects) dataPersistenceObj.LoadData(gameData);
    }

    public void SaveGame()
    {
        //pass the data to other scripts so they can update it
        foreach (var dataPersistenceObj in dataPersistenceObjects) dataPersistenceObj.SaveData(gameData);

        //save the data to a file using the data handler
        dataHandler.Save(gameData);
    }

    //placeholder
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().
            OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
