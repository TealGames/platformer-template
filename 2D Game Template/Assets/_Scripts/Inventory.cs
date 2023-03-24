using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Inventory : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;

    [Header("UI")]
    [SerializeField] private GameObject itemDescriptionContainer;
    [SerializeField] private Animator itemDescriptionAnimator;

    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemLoreText;

    [SerializeField] private GameObject[] itemCells;

    [Header("Sounds")]

    [Tooltip("The time it takes for the sounds to fade to musicVolumeChangeMultiplier when inventory is enabled")][SerializeField] private float inventoryFadeDuration;
    [Tooltip("Multiplies current volume by this multiplier to get new volume when fading in/out. For example, a value of 0.25 would be reducing the volume to 25% of original")]
    [Range(0f, 2f)] [SerializeField] private float musicVolumeChangeMultiplier;
    [SerializeField] private AudioClip inventoryActivateSound;

    [SerializeField] private AudioClip itemDescriptionActivateSound;

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
            Dictionary<string, int> inventoryItems = PlayerCharacter.Instance.GetCurrentInventoryItems();
            if (itemCells != null)
            {
                foreach (var item in inventoryItems)
                {
                    if (PlayerCharacter.Instance.GetInventoryItemQuantity(item.Key) >0)
                    {
                        int itemIndex= GetCellIndexByType(item.Key);
                        itemCells[itemIndex].SetActive(true);
                        //quantity of the item should be set here
                    }
                }
            }
            else UnityEngine.Debug.LogWarning("Note: the inventory item cells UI objects are not set up but inventory items are being accessed!");
            
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

    public void EnableItemDescription(ObjectTextSO objectData)
    {
        itemDescriptionContainer.gameObject.SetActive(true);
        if (itemDescriptionAnimator != null) itemDescriptionAnimator?.SetTrigger("activated_trig");

        itemNameText.text= objectData.objectName;
        itemDescriptionText.text = objectData.objectDescription;
        itemLoreText.text = objectData.objectLore;
    }

    public void DisableItemDescription()
    {
        itemDescriptionContainer.gameObject.SetActive(false);
        if (itemDescriptionAnimator != null) itemDescriptionAnimator?.SetTrigger("deactivated_trig");

        itemNameText.text = "";
        itemDescriptionText.text = "";
        itemLoreText.text = "";
    }

    private int GetCellIndexByType(string inventoryItemType)
    {
        int index = 0;
        foreach(var item in itemCells)
        {
            if (item.GetComponent<InventoryItemType>().GetInventoryItemType().ToString() == inventoryItemType) return index;
            index++;
        }
        return 0;
        UnityEngine.Debug.LogWarning($"Getting the inventory item with type {inventoryItemType} was not found from the cell gameObject images!");
    }
}
