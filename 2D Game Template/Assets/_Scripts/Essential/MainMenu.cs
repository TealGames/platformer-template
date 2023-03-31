using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class MainMenu : Menu
{
    [Header("Audio")]
    [Tooltip("The sound made when Start Game button is clicked")][SerializeField] private AudioClip startGameSound;
    [Tooltip("The sound made when return to main menu (when in file select menu) is clicked")][SerializeField] private AudioClip returnToMainMenuSound;
    [SerializeField] private string mainMenuSongName;

    [Header("GameObject References")]
    [SerializeField] private GameObject fileMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button loadGameButton;

    private AudioSource audioSource;
    private Animator animator;

    private bool inMainMenu;

    public event Action OnGameStart;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        AudioManager.Instance.Play(mainMenuSongName);
        inMainMenu = true;

        DisableButtonsDependingOnData();
    }

    private void DisableButtonsDependingOnData()
    {
        if (!DataPersistenceManager.Instance.HasGameData())
        {
            continueButton.interactable = false;
            loadGameButton.interactable = false;
        }
    }

    //if user wants to start a new game and select a file to save it to
    public void NewGame()
    {
        if (audioSource!=null && startGameSound!=null) audioSource.PlayOneShot(startGameSound);
        fileMenu.GetComponent<SaveSlotsMenu>().ActivateMenu(false);

        mainMenu.SetActive(false);
    }

    //if the user wants to return to a previous save file and not the most recent save file
    public void LoadGame()
    {
        if (audioSource != null && startGameSound != null) audioSource.PlayOneShot(startGameSound);
        fileMenu.GetComponent<SaveSlotsMenu>().ActivateMenu(true);

        mainMenu.SetActive(false);

    }


    //since a game data is loaded when a scene is loaded, by loading first scene, all data is auto loaded
    //NOTE: THIS DOES NOT IMPLEMENT SCENE SAVING SINCE FIRST SCENE IS ALWAYS CALLED RATHER THAN LAST SCENE PLAYER WAS IN
    public void ContinueGame()
    {
        //save game before loading scene
        DataPersistenceManager.Instance.SaveGame();
        GameManager.Instance.LoadFirstScene();
    }

    public void EnableSettings()
    {
        mainMenu.SetActive(false);
        SettingsMenu settingsScript = HUD.Instance.GetComponentInChildren<SettingsMenu>();
        settingsScript.EnableSettingsMenu();
        settingsScript.OnReturn += () => mainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        inMainMenu = false;
    }

    public void ReturnToMainMenu()
    {
        if (audioSource != null && returnToMainMenuSound != null) audioSource.PlayOneShot(returnToMainMenuSound);
        fileMenu.SetActive(false);
        mainMenu.SetActive(true);

        //refreshes main menu page if user clears data and cannot continue (no current save file) or load
        DisableButtonsDependingOnData();
    }

    

    private void PlayMainMenuSong()
    {

    }

}