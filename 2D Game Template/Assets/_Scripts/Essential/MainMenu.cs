using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private LevelSO firstLevel;
    [SerializeField] private AudioClip gameStartSound;
    [SerializeField] private string mainMenuSongName;


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

        OnGameStart?.Invoke();

        if (audioSource!=null)audioSource.PlayOneShot(gameStartSound);
        StartCoroutine(GameManager.Instance.LoadSceneAsync(firstLevel, false, null));
    }

    public void QuitGame()
    {
        Application.Quit();
        inMainMenu = false;
    }

    private void PlayMainMenuSong()
    {
        
    }
}