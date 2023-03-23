using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Manages, activates, disables and performs actions related to the pause menu
/// </summary>

public class PauseMenu : MonoBehaviour
{
    [Header("When Paused")]
    [SerializeField] [Tooltip("In order to avoid coroutines from stopping when pause menu disables, we need a separate container to hold gameobjects without disabling parent")] private GameObject pauseMenuContainer;
    [SerializeField] private AudioClip pauseMenuActivateSound;

    [Tooltip("The time it takes for the sounds to fade to musicVolumeChangeMultiplier when pause menu is enabled")][SerializeField] private float onPauseMusicFadeDuration;
    [SerializeField][Range(0f, 2f)][Tooltip("Multiplies current volume by this multiplier to get new volume when fading in/out. For example, a value of 0.25 would be reducing the volume to 25% of original")] 
    private float musicVolumeChangeMultiplier;

    private Dictionary<string, float> currentVolumes;

    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject settingsButton;

    [SerializeField] private GameObject quitToMenuButton;

    private AudioSource audioSource;
    private Animator animator;

    public event Action OnPauseEnabled;
    public event Action OnPauseDisabled;
    public event Action OnReturnToMainMenu;

    public event Action OnSettingsButtonPressed;
    // Start is called before the first frame update
    void Start()
    {
        PlayerCharacter.Instance.OnEscapeButtonPressed += EnablePauseMenu;
        AudioManager.Instance.OnAudioFadeIn += GetCurrentlyPlayingAudioVolumes;

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        currentVolumes = new Dictionary<string, float>();



        gameObject.SetActive(true);
        pauseMenuContainer.SetActive(false);
    }

    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //UnityEngine.Debug.Log(SceneManager.GetActiveScene().name);
    }

    public void EnablePauseMenu()
        //used when enabling pause menu in game
    {
        if (GameManager.Instance.GetCurrentGameState() == GameManager.GameState.Playing)
        {
            PlayerCharacter.Instance.OnEscapeButtonPressed -= EnablePauseMenu;
            PlayerCharacter.Instance.OnPlayerDamage += ReturnToGame;
            OnPauseEnabled?.Invoke();

            PlayerCharacter.Instance.FreezePlayer(true);
            if (animator != null) animator.SetBool("isActivated", true);
            else pauseMenuContainer.SetActive(true);
            StartCoroutine(SelectFirstChoice());

            audioSource?.PlayOneShot(pauseMenuActivateSound);
            StartCoroutine(AudioManager.Instance.FadeSounds(musicVolumeChangeMultiplier, onPauseMusicFadeDuration));
        }
        else UnityEngine.Debug.LogError("Escape button was pressed but since current game state is not GameManager.GameState.Playing, the pause menu cannot be brought up!");
    }

    public void ReturnToPauseMenu()
        //only used when going from controls back to pause menu
    {
        
        PlayerCharacter.Instance.OnEscapeButtonPressed -= EnablePauseMenu;
        PlayerCharacter.Instance.OnPlayerDamage += ReturnToGame;
        pauseMenuContainer.SetActive(true);
        StartCoroutine(SelectFirstChoice());
    }

    private void GetCurrentlyPlayingAudioVolumes() => currentVolumes = AudioManager.Instance.GetPlayingAudioVolumes();

    public void EnableSettingsMenu()
    {
        pauseMenuContainer.SetActive(false);
        OnSettingsButtonPressed?.Invoke();
    }

    public void ReturnToGame()
    {
        StartCoroutine(AudioManager.Instance.FadeSounds((1 / musicVolumeChangeMultiplier), onPauseMusicFadeDuration));

        PlayerCharacter.Instance.OnEscapeButtonPressed += EnablePauseMenu;
        PlayerCharacter.Instance.OnPlayerDamage -= ReturnToGame;

        OnPauseDisabled?.Invoke();

        PlayerCharacter.Instance.FreezePlayer(false);

        

        if (animator!=null) animator.SetBool("isActivated", false);
        else pauseMenuContainer.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        //To make sure that volumes get set back to non-faded values and so that default volume data is not lost when transitioning
        if (currentVolumes.Count > 0)
        {
            foreach (var pair in currentVolumes)
            {
                AudioManager.Instance.SetVolume(pair.Key, pair.Value);
            }
        }

        OnReturnToMainMenu?.Invoke();

        PlayerCharacter.Instance.FreezePlayer(false);
        PlayerCharacter.Instance.gameObject.SetActive(false);
        pauseMenuContainer.SetActive(false);
        StartCoroutine(GameManager.Instance.LoadSceneAsync("MainMenu", false, null));
        
    }

    private IEnumerator SelectFirstChoice()
    {
        // Event System requires we clear it first, then wait
        // for at least one frame before we set the current selected object.
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(continueButton);
    }
}