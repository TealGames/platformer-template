using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;

/// <summary>
/// Add this script to any item that physically appears in the world and can be isCollected when the player enters the collider. If its an item that would be classified in the enum CollectibleType then its a collectible
/// </summary>



public class Collectible : MonoBehaviour, IDataPersistence
{
    [Header("Object ID")]
    [Tooltip("In order for the save system to remember what object was isCollected or not, it saves the isCollected object's id. " +
        "To generate a random id, right click on script and select 'Generate guid for id'. A guid is a string of 32 random characters with low chance of repeating ids. ")]
    private string id;

    [ContextMenu("Generate guid for id")]
    private void GenerateGuid() => id= System.Guid.NewGuid().ToString();

    private AudioSource audioSource;
    private Animator animator;

    [SerializeField] private AudioClip collectSound;
    [SerializeField] private CollectibleType collectibleType;

    public enum CollectibleType
    {
        InventoryItem,
        Health,
        Currency,
    }
    [SerializeField] private PlayerCharacter.InventoryItemTypes inventoryItemType;

    private bool isCollected;



    public event Action<string> OnCollectibleCollected;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        if (id == "") GenerateGuid();

        //DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
       
    }

    public void Collected()
    {
        isCollected = true;
        audioSource.PlayOneShot(collectSound);
        if (animator != null) animator.SetTrigger("collected_trig");
        else gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
        {
            if (collectibleType == CollectibleType.InventoryItem)
            {
                if (inventoryItemType != PlayerCharacter.InventoryItemTypes.None)
                {
                    string inventoryItemName = inventoryItemType.ToString();
                    PlayerCharacter.Instance.SetInventoryItemQuantity(inventoryItemName, PlayerCharacter.Instance.GetInventoryItemQuantity(inventoryItemName + 1));
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"{gameObject.name} is an inventory type collectible but the type of inventory item is not specified!");
                    return;
                }
            }
            else if (collectibleType != CollectibleType.InventoryItem)
            {
                if (inventoryItemType != PlayerCharacter.InventoryItemTypes.None)
                {
                    UnityEngine.Debug.LogWarning($"{gameObject.name} is not an inventory type collectible but a non-inventory type collectible is specified!");
                    return;
                }

                if (collectibleType == CollectibleType.Currency) PlayerCharacter.Instance.SetCurrency(PlayerCharacter.Instance.GetCurrency() + 1);
                else if (collectibleType == CollectibleType.Health) PlayerCharacter.Instance.SetHealth(PlayerCharacter.Instance.GetHealth() + 1);
            }
            Collected();
        }
        else UnityEngine.Debug.Log($"A non player object has entered {gameObject.name}'s collider!");
    }

    public void LoadData(GameData data)
    {
        data.collectiblesCollected.TryGetValue(id, out isCollected);
        if (isCollected) gameObject.SetActive(false);
    }

    public void SaveData(GameData data)
    {
        if (data.collectiblesCollected.ContainsKey(id)) data.collectiblesCollected.Remove(id);
        data.collectiblesCollected.Add(id, isCollected);
    }
}