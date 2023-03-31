using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.Events;
using System;

/// <summary>
/// Manages Scene switching, loading, remembering last loaded scene, transitions, game start and end and initial level loading and unloading
/// </summary>

public class GameManager : MonoBehaviour
{
    [Header("Game Start")]
    [SerializeField] [Tooltip("Whether or not to fade all playing sounds")] private bool fadeSoundsOnGameEnter;
    [SerializeField] private string gameStartMusicName;

    [Header("New Game")]
    [Tooltip("When new game is clicked, the first level data to load")][SerializeField] private LevelSO firstLevelData;
    [Tooltip("When new game is clicked, the location the player spawns")][SerializeField] private Transform newGameSpawn;

    [Header("Continue Game")]

    [Header("Levels")]
    [SerializeField] [Tooltip("The duration that a level title stays on screen before it gets deactivated")] private float titleDuration;
    [SerializeField][Tooltip("Once scene loads and transition fades out and NewLevelEnter() is called (starts song and sets title for level), the additional delay to add to the title showing")] private float levelTitleDelay = 0f;
    [SerializeField] [Tooltip("Used only for other scripts to be able to get the level data by name of level")]private LevelSO[] levels;

    private static bool sceneLoaded = false;
    private string currentLoadedScene;


    [Header("Transitions")]
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private GameObject blackScreen;
    [SerializeField] [Tooltip("The speed of either half of the transition animation(start or end)")] private float transitionSpeed = 1f;
    private bool sceneLoadingTriggered;

    [SerializeField][Tooltip("The duration that it takes for the Audio Manager to fade all playing sounds out")] private float soundFadeOutDuration;
    [SerializeField] [Tooltip("The duration that it takes for the Audio Manager to fade all playing sounds in")] private float soundFadeInDuration;
 
    [SerializeField][Tooltip("Delay of the level name and music when the transition ends")] private float levelEntryDelay;
    [SerializeField][Tooltip("If true, then after fade out transition trigger is called, will wait the animations duration before loading and unloading scenes")] private bool offsetLoadByAnimationDuration;

    [SerializeField] private AudioClip sceneChangeSound;
    [SerializeField][Tooltip("Duration of the fade out of master mixer volume (all audio)")] float masterVolumeFadeDuration;


    [Header("Other")]
    [SerializeField] private PauseMenu pauseMenuScript;


    [Header("Tools")]
    [SerializeField] private bool changeTimeScale;
    [SerializeField][Range(0, 2)] private float adjustedTimeScale;

    private float defaultVolume;
    private AudioSource audioSource;


    private bool mainMenuWillLoad;

    public event Action OnGameStart;
    public event Action OnGameEnd;
    public event Action OnSceneLoadStarted;
    public event Action OnSceneLoadEnded;
    public event Action OnNewLevelEntered;

    

    public enum GameState
    {
        MainMenu,
        Playing,
        Loading,
    }
    private GameState currentGameState;


    public static GameManager Instance;

    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null) Instance = this; 
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (changeTimeScale) changeTimeScale = false;
        //pauseMenuScript.OnMainMenuLoaded += OnMainMenuLoad;
        SceneManager.sceneLoaded += OnSceneLoaded;
        //pauseMenuScript.OnReturnToMainMenu += () => currentGameState = GameState.MainMenu;
         
        audioSource = GetComponent<AudioSource>();
        transitionAnimator.SetFloat("transitionMultiplier_f", transitionSpeed);

        //defaultVolume = Mathf.Pow(10, (settingsMenu.GetComponent<SettingsMenu>().masterVolumeSlider.value) / 20);

        
    }

    // Update is called once per frame
    void Update()
    {
        //UnityEngine.Debug.Log($"Scene loaded is {sceneLoaded}");
        if (changeTimeScale) Time.timeScale = adjustedTimeScale;
        UnityEngine.Debug.Log($"Game state is {currentGameState}");

        //first scene in build settings should always be main menu
        if (SceneManager.GetActiveScene()== SceneManager.GetSceneByBuildIndex(0))
        {
            //currentGameState = GameState.MainMenu;
            UnityEngine.Debug.Log("Game state is menu for some reason!");
        }
            
        UnityEngine.Debug.Log("Current active scene is " + SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadFirstScene()
    {
        UnityEngine.Debug.Log("LoadFirstScene() is called!");
        OnGameStart?.Invoke(); 

        
        StartCoroutine(LoadSceneAsync(firstLevelData, true, newGameSpawn));
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoaded = true;
        UnityEngine.Debug.Log("The scene has loaded");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene.name));
        //currentLoadedScene = scene.name;
        
    }

    private IEnumerator NewLevelEnter(LevelSO levelData)
    {

        
        //whenever a new level is called, it must be during play mode (and not during loading or main menu)
        currentGameState = GameState.Playing;

        yield return new WaitForSeconds(levelEntryDelay);
        OnNewLevelEntered?.Invoke();



        UnityEngine.Debug.Log("New Level Enter called!");
        if (!string.IsNullOrEmpty(levelData.levelName))
        {
            yield return new WaitForSeconds(levelTitleDelay);
            if (levelData.textColor!=null) HUD.Instance.ReturnText(HUD.TextType.Title).colorGradientPreset = levelData.textColor;
            HUD.Instance.EnableText(HUD.TextType.Title, levelData.levelName);
            //StartCoroutine(LevelTitleCooldown());
        }
        
        if (sceneChangeSound != null) audioSource.PlayOneShot(sceneChangeSound);
        if (!string.IsNullOrEmpty(levelData.levelMusic))
        {
            AudioManager.Instance.Play(levelData.levelMusic);
            if(levelData.fadeSounds)StartCoroutine(AudioManager.Instance.FadeIn(soundFadeInDuration));
        }
        if (!string.IsNullOrEmpty(levelData.levelAmbience)) AudioManager.Instance.Play(levelData.levelAmbience);

        yield return new WaitForSeconds(titleDuration);
        HUD.Instance.DisableText(HUD.TextType.Title);

        
    } 

    public IEnumerator LoadSceneAsync(LevelSO levelData, bool teleportPlayer, Transform teleportPoint)
    {
        UnityEngine.Debug.Log("Scene to unload is " + SceneManager.GetActiveScene().name);
        OnSceneLoadStarted?.Invoke();
        currentGameState = GameState.Loading;
        

        Scene sceneToUnload = SceneManager.GetActiveScene();
        string scenePath = $"Assets/Scenes/{levelData.sceneName}.unity";

        if(levelData.fadeSounds) StartCoroutine(AudioManager.Instance.FadeOut(soundFadeOutDuration));
        transitionAnimator.SetTrigger("startTransition_trig");

        if (offsetLoadByAnimationDuration)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneUtility.GetBuildIndexByScenePath(scenePath));
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneToUnload.buildIndex);
        yield return new WaitForEndOfFrame();

        if (teleportPlayer)
        {

            PlayerCharacter.Instance.SetPlayerPosition(teleportPoint);
            UnityEngine.Debug.Log("The player should teleport");
        }
        transitionAnimator.SetTrigger("endTransition_trig");
        
        yield return new WaitForEndOfFrame();
        StartCoroutine(NewLevelEnter(levelData));

        OnSceneLoadEnded?.Invoke();
    }

    //alternative LoadSceneAsync() where instead of level data, we can specify scene name
    public IEnumerator LoadSceneAsync(string sceneName, bool teleportPlayer, Transform teleportPoint)
    {
        OnSceneLoadStarted?.Invoke();
        currentGameState = GameState.Loading;

        UnityEngine.Debug.Log("Scene to unload is " + SceneManager.GetActiveScene().name);
        Scene sceneToUnload = SceneManager.GetActiveScene();

        StartCoroutine(AudioManager.Instance.FadeOut(soundFadeOutDuration));
        transitionAnimator.SetTrigger("startTransition_trig");

        if (offsetLoadByAnimationDuration)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneToUnload.buildIndex);
        yield return new WaitForEndOfFrame();

        if (teleportPlayer)
        {
            PlayerCharacter.Instance.SetPlayerPosition(teleportPoint);
            UnityEngine.Debug.Log("The player should teleport");
        }
        transitionAnimator.SetTrigger("endTransition_trig");
        yield return new WaitForEndOfFrame();
    }

    /*
    public void LoadSceneAsync(int buildSettingsSceneIndex, bool showTitle, bool teleportPlayer, Transform teleportPoint)
    {
        int sceneToUnload = lastLoadedSceneIndex;
        //last loaded scene refers to the scene that is being unloaded/changed into the new scene
        if (buildSettingsSceneIndex == lastLoadedSceneIndex) sceneToUnload = 1;
        if (buildSettingsSceneIndex == 4)
        {
            buildSettingsSceneIndex = 4;
            sceneToUnload = 3;
        }

        StartCoroutine(AudioManager.Instance.FadeMixerGroupVolume("Music", masterVolumeFadeDuration, 0.0f));

        transitionAnimator.SetTrigger("StartTransition_trig");
        UnityEngine.Debug.Log(sceneToUnload);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildSettingsSceneIndex, LoadSceneMode.Additive);
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneToUnload);


        if (teleportPlayer)
        {

            PlayerCharacter.Instance.SetPlayerPosition(teleportPoint);
            UnityEngine.Debug.Log("The player should teleport");
        }

        transitionAnimator.SetTrigger("EndTransition_trig");
        this.showTitle = showTitle;



        UnityEngine.Debug.Log($"Default Volume is {defaultVolume}");
        StartCoroutine(AudioManager.Instance.FadeMixerGroupVolume("Master", masterVolumeFadeDuration, defaultVolume));
        StartCoroutine(AudioManager.Instance.FadeMixerGroupVolume("Music", masterVolumeFadeDuration, defaultVolume));
        Invoke("LevelEntry", levelEntryDelay);
        OnSceneLoadStarted?.Invoke();

        UnityEngine.Debug.Log("Scene has loaded!");

        if (!mainMenuWillLoad) lastLoadedSceneIndex = buildSettingsSceneIndex;
        else mainMenuWillLoad = false;
        //if not going back to main menu change last scene index
    }
    */



    public void SetDefaultVolume(float volume)
    {
        defaultVolume = volume;
        UnityEngine.Debug.Log($"Default volume is {defaultVolume}");
    }

    public void GameEnd()
    {
        OnGameEnd?.Invoke();
        //PlayerCharacter.Instance.gameObject.SetActive(false);

        DialogueManager.Instance.gameObject.transform.Find("Player HUD").gameObject.SetActive(false);

        audioSource.PlayOneShot(sceneChangeSound);

        UnityEngine.Debug.Log("LoadSceneAsync has been called!");

        StartCoroutine(AudioManager.Instance.FadeMixerGroupVolume("Master", masterVolumeFadeDuration, 0.0f));

        transitionAnimator.SetTrigger("StartTransition_trig");

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(5, LoadSceneMode.Additive);



        transitionAnimator.SetTrigger("EndTransition_trig");
        Invoke("LevelEntry", levelEntryDelay);

        OnSceneLoadStarted?.Invoke();

        UnityEngine.Debug.Log("Scene has loaded!");

    }
    

    public void OnMainMenuLoad() => mainMenuWillLoad = true;

    private IEnumerator SceneChangeCooldown()
    {
        yield return new WaitForSeconds(1.0f);
        sceneLoadingTriggered = false;
    }

    public LevelSO GetLevelData(string levelName)
    {
        foreach(var level in levels)
        {
            if (level.levelName== levelName) return level;
        }
        UnityEngine.Debug.Log($"Level data (scriptable object) with the name {levelName} does not exist! Make sure it is spelled correctly!");
        return null;
    }

    public void SetBlackScreen(bool showScreen)
    {
        if (showScreen) blackScreen.gameObject.SetActive(true);
        else blackScreen.gameObject.SetActive(false);
    }

    public GameState GetCurrentGameState() => currentGameState;

}