using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;

public class Collectible : MonoBehaviour
{
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



    public event Action<string> OnCollectibleCollected;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        //DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
       
    }

    public void Collected()
    {
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
}