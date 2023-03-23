using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;


    [Tooltip("The time it takes for the sounds to fade to musicVolumeChangeMultiplier when inventory is enabled")][SerializeField] private float inventoryFadeDuration;
    [Tooltip("Multiplies current volume by this multiplier to get new volume when fading in/out. For example, a value of 0.25 would be reducing the volume to 25% of original")]
    [Range(0f, 2f)] [SerializeField] private float musicVolumeChangeMultiplier;
    [SerializeField] private AudioClip inventoryActivateSound;

    public event Action OnInventoryEnabled;
    public event Action OnInventoryDisabled;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnableInventory()
    {
        if (GameManager.Instance.GetCurrentGameState() == GameManager.GameState.Playing)
        {
            PlayerCharacter.Instance.OnInventoryButtonPressed -= EnableInventory;
            PlayerCharacter.Instance.OnPlayerDamage += DisableInventory;
            OnInventoryEnabled?.Invoke();

            PlayerCharacter.Instance.FreezePlayer(true);
            if (animator != null) animator.SetBool("isActivated", true);
            else gameObject.SetActive(true);

            audioSource?.PlayOneShot(inventoryActivateSound);
            StartCoroutine(AudioManager.Instance.FadeSounds(musicVolumeChangeMultiplier, inventoryFadeDuration));
        }
        else UnityEngine.Debug.LogError("Inventory button was pressed but since current game state is not GameManager.GameState.Playing, the inventory cannot be brought up!");
    }

    public void DisableInventory()
    {
        StartCoroutine(AudioManager.Instance.FadeSounds((1 / musicVolumeChangeMultiplier), inventoryFadeDuration));

        PlayerCharacter.Instance.OnEscapeButtonPressed += EnableInventory;
        PlayerCharacter.Instance.OnPlayerDamage -= DisableInventory;

        OnInventoryDisabled?.Invoke();

        PlayerCharacter.Instance.FreezePlayer(false);

        if (animator != null) animator.SetBool("isActivated", false);
        else gameObject.SetActive(false);
    }
}
