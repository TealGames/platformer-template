using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SettingsMenu : MonoBehaviour
{
    [Header("General")]
    [SerializeField][Tooltip("To allow the settings menu to activate coroutines and functions (so we keeo parent object active) while settings menu gameObjects is not visible")] private GameObject settingsMenuContainer;
    [SerializeField] private PauseMenu PauseMenu;
    [SerializeField] private AudioClip settingsMenuActivateSound;

    [Header("Volume/Graphics Tab")]

    [SerializeField] private GameObject VolumeAndGraphicsTabContainer;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    [SerializeField] private TextMeshProUGUI masterValueText;
    [SerializeField] private TextMeshProUGUI musicValueText;


    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private TMPro.TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;



    [Header("Controls Tab")]
    [SerializeField] private GameObject controlTabContainer;
    [Tooltip("When a user picks new bindings, these keys will be ignored and will not be able to be choosen for any of the controls")][SerializeField] private InputAction ignoreKeys;

    private AudioSource audioSource;
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        PauseMenu.OnSettingsButtonPressed += EnableSettingsMenu;

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        settingsMenuContainer.SetActive(false);
        SetMasterVolume(0f);
        SetMusicVolume(0f);

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResoltionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        //iterate through all possible resolutions in resolutions[] and change it to a string and add it to a string list to add to the dropdown (can only be string lists)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResoltionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResoltionIndex;
        resolutionDropdown.RefreshShownValue();

        audioMixer.SetFloat("MIXER_MASTER_VOLUME", masterVolumeSlider.value);
        audioMixer.SetFloat("MIXER_MUSIC_VOLUME", musicVolumeSlider.value);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnableSettingsMenu()
    {
        ActivateControlsTab();
        if (animator != null) animator.SetBool("isActivated", true);
        else settingsMenuContainer.SetActive(true);
        audioSource?.PlayOneShot(settingsMenuActivateSound);
    }


    public void ReturnToPauseMenu()
    {
        if (animator != null) animator.SetBool("isActivated", false);
        else settingsMenuContainer.SetActive(false);
        PauseMenu.ReturnToPauseMenu();

    }

    public void SetMasterVolume(float volume)
    {
        masterValueText.text = ((int)(volume + 80f)).ToString();
        audioMixer.SetFloat("MIXER_MASTER_VOLUME", volume);
        GameManager.Instance.SetDefaultVolume(Mathf.Pow(10, volume / 20));
        //converts audioMixer volume into a volume from 0.0f to 1.0f
    }

    public void SetMusicVolume(float volume)
    {
        musicValueText.text = ((int)(volume + 80f)).ToString();
        audioMixer.SetFloat("MIXER_MUSIC_VOLUME", volume);
    }

    public void SetFullScreen(bool isFullScreen) => Screen.fullScreen = isFullScreen;

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        //create a new resolution from reoslutions[] with index from argument
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void ActivateControlsTab()
    {
        controlTabContainer.SetActive(true);
        VolumeAndGraphicsTabContainer.SetActive(false);
    }

    public void ActivateVolumeAndGraphicsTab()
    {
        VolumeAndGraphicsTabContainer.SetActive(true);
        controlTabContainer.SetActive(false);
    }

    public InputAction GetIgnoreKeysAction() => ignoreKeys;
}