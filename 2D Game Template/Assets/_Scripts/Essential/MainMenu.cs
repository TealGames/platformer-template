using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Tooltip("The sound made when Start Game button is clicked")][SerializeField] private AudioClip startGameSound;
    [Tooltip("The sound made when return to main menu (when in file select menu) is clicked")][SerializeField] private AudioClip returnToMainMenuSound;
    [SerializeField] private string mainMenuSongName;

    [SerializeField] private GameObject fileMenuContainer;
    [SerializeField] private GameObject mainMenuContainer;

    private AudioSource audioSource;
    private Animator animator;

    private bool inMainMenu;

    public event Action OnGameStart;

    // Start is called before the first frame update
    void Start()
    {
        //since main menu is its own scene, we need to call another event in GameManager to be used by non-singleton scripts
        OnGameStart += GameManager.Instance.GameStarted;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        AudioManager.Instance.Play(mainMenuSongName);
        inMainMenu = true;
        PlayerCharacter.Instance.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        if (audioSource!=null && startGameSound!=null) audioSource.PlayOneShot(startGameSound);
        fileMenuContainer.SetActive(true);
        mainMenuContainer.SetActive(false);
    }

    public void EnableSettings()
    {
        HUD.Instance.GetComponentInChildren<SettingsMenu>().EnableSettingsMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
        inMainMenu = false;
    }

    public void ReturnToMainMenu()
    {
        if (audioSource != null && returnToMainMenuSound != null) audioSource.PlayOneShot(returnToMainMenuSound);
        fileMenuContainer.SetActive(false);
        mainMenuContainer.SetActive(true);
    }

    public void NewGame()
    {
        OnGameStart?.Invoke();
        DataPersistenceManager.Instance.NewGame();
        GameManager.Instance.HandleNewGame();
    }

    public void ContinueGame()
    {
        OnGameStart?.Invoke();
    }

    private void PlayMainMenuSong()
    {

    }

}