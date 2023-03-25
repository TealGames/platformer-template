using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;

    [Header("UI")]
    [Tooltip("Note: the purpose of the container objects is to have the main parent class script running (causes errors when running scripts with deactivated objects) " +
        "while the visuals are still enabled and disabled")]
    [SerializeField] private GameObject inventoryContainer;

    [Tooltip("Note: the purpose of the container objects is to have the main parent class script running (causes errors when running scripts with deactivated objects) " +
        "while the visuals are still enabled and disabled")]
    [SerializeField] private GameObject itemDescriptionContainer;

    [Tooltip("The gameObject that holds all of the different inventory item cells")][SerializeField] private GameObject itemCellsContainer;

    [Tooltip("The gameObject that appears on the object when it is the one that is selected in the inventory")]
    [SerializeField] private GameObject selectedItemIdentifier;

    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemLoreText;

    private GameObject selectedGameObject;
    private GameObject firstCell;

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
        PlayerCharacter.Instance.OnInventoryButtonPressed += EnableInventory;
        inventoryContainer.SetActive(false);
    }
        

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnableInventory()
    {
        if (GameManager.Instance.GetCurrentGameState() == GameManager.GameState.Playing)
        {
            if (itemCellsContainer!=null)
            {
                //loops through all items in the parent container with all cells
                for (int i = 0; i < itemCellsContainer.transform.childCount; i++) itemCellsContainer.transform.GetChild(i).gameObject.SetActive(false);

                Dictionary<string, int> inventoryItems = PlayerCharacter.Instance.GetCurrentInventoryItems();
                foreach (var item in inventoryItems)
                {
                    if (PlayerCharacter.Instance.GetInventoryItemQuantity(item.Key) > 0)
                    {
                        UnityEngine.Debug.Log($"Current testing {item.Key}, {item.Value}");
                        GameObject currentCell = GetCellByType(item.Key);
                        
                        //make a new list and store all textMeshProUGUIs in cell and change first text (quantity text) found
                        List<TextMeshProUGUI> textList = new List<TextMeshProUGUI>();
                        currentCell.GetComponentsInChildren<TextMeshProUGUI>(textList);
                        textList[0].text = item.Value.ToString();

                        currentCell.SetActive(true);
                    }
                }
                SelectFirstItem();

            }
            else UnityEngine.Debug.LogWarning("Note: the inventory item cells UI parent object is not set up but inventory items are being accessed!");

            //inventory button can disable inventory, not enable
            PlayerCharacter.Instance.OnInventoryButtonPressed -= EnableInventory;
            PlayerCharacter.Instance.OnInventoryButtonPressed += DisableInventory;

            //if player damaged or escape button is pressed, disable inventory
            PlayerCharacter.Instance.OnPlayerDamage += DisableInventory;
            PlayerCharacter.Instance.OnEscapeButtonPressed+= DisableInventory;

            OnInventoryEnabled?.Invoke();

            PlayerCharacter.Instance.FreezePlayer(true);
            if (animator != null) animator.SetBool("isActivated", true);
            else inventoryContainer.SetActive(true);

            if (inventoryActivateSound!=null) audioSource?.PlayOneShot(inventoryActivateSound);
            StartCoroutine(AudioManager.Instance.FadeSounds(musicVolumeChangeMultiplier, inventoryFadeDuration));

            
        }
        else UnityEngine.Debug.LogError("Inventory button was pressed but since current game state is not GameManager.GameState.Playing, the inventory cannot be brought up!");
    }

    public void DisableInventory()
    {
        StartCoroutine(AudioManager.Instance.FadeSounds((1 / musicVolumeChangeMultiplier), inventoryFadeDuration));

        //inventory button can enable inventory, not disable
        PlayerCharacter.Instance.OnInventoryButtonPressed -= DisableInventory;
        PlayerCharacter.Instance.OnInventoryButtonPressed += EnableInventory;

        //if player gets damaged or escape button is pressed, dont disable inventory again
        PlayerCharacter.Instance.OnPlayerDamage -= DisableInventory;
        PlayerCharacter.Instance.OnEscapeButtonPressed -= DisableInventory;

        OnInventoryDisabled?.Invoke();

        PlayerCharacter.Instance.FreezePlayer(false);

        if (animator != null) animator.SetBool("isActivated", false);
        else inventoryContainer.SetActive(false);
    }

    private void SetItemDescription(GameObject itemCell)
    {
        if (itemCell.TryGetComponent<InventoryItemType>(out InventoryItemType inventoryScript))
        {
            UnityEngine.Debug.Log("Item description is being set!");
            ObjectTextSO objectData= inventoryScript.GetItemDescription();
            itemDescriptionContainer.gameObject.SetActive(true);

            itemNameText.text = objectData.objectName;
            itemDescriptionText.text = objectData.objectDescription;
            itemLoreText.text = objectData.objectLore;
        }
        else UnityEngine.Debug.LogError($"SetItemDescription() was called with gameObject {itemCell} but it does not have the required InventoryItemType.cs script!");
    }

    private IEnumerator SetItemIdentifier(RectTransform transform)
    {
        yield return new WaitForEndOfFrame();
        selectedItemIdentifier.GetComponent<RectTransform>().position = transform.position;

    }
        
    private GameObject GetCellByType(string inventoryItemType)
    {
        
        for (int i=0; i<itemCellsContainer.transform.childCount; i++)
        {
            if (itemCellsContainer.transform.GetChild(i).gameObject.GetComponent<InventoryItemType>().GetInventoryItemType().ToString()
                == inventoryItemType) return itemCellsContainer.transform.GetChild(i).gameObject;
        }
        return null;
        UnityEngine.Debug.LogWarning($"Getting the inventory item with type {inventoryItemType} was not found from the cell gameObject images!");  
    }

    private void SelectFirstItem()
    {
        UnityEngine.Debug.Log("Select first item called!");
        for (int i = 0; i < itemCellsContainer.transform.childCount; i++)
        {
            if (itemCellsContainer.transform.GetChild(i).gameObject.activeSelf)
            {
                firstCell = itemCellsContainer.transform.GetChild(i).gameObject;
                UnityEngine.Debug.Log($"First cell is {firstCell.name}");

                //set the first cell to have identifier and item description
                StartCoroutine(SetItemIdentifier(firstCell.GetComponent<RectTransform>()));
                SetItemDescription(firstCell);
                break;
            }
            
        }
    }

    //when an inventory item is clicked on
    public void HandleItemClick(BaseEventData eventData)
    {
        if (eventData != null)
        {
            UnityEngine.Debug.Log("Item selected called!");
            selectedGameObject= eventData.selectedObject.gameObject;

            StartCoroutine(SetItemIdentifier(selectedGameObject.GetComponent<RectTransform>()));
            SetItemDescription(selectedGameObject);
        }
        else UnityEngine.Debug.LogWarning("Item that was clicked on has null event data!");
        
    }

    
}
