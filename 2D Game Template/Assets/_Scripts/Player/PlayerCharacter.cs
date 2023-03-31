using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using AYellowpaper.SerializedCollections;

/// <summary>
/// Manages the player and what the player does, input control, events, damage, weapons, inventory, etc.
/// </summary>

public class PlayerCharacter : MonoBehaviour, IDamageable, IDataPersistence
{
    public static PlayerCharacter Instance;


    [Header("References")]

    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private ParticleSystem moveEffect;
    [SerializeField] private ParticleSystem damageEffect;
    [SerializeField] private ParticleSystem killEffect;

    [SerializeField] private Transform groundCheck;
    //[SerializeField] private Transform attackPoint;
    [SerializeField] private Transform weaponEdgePoint;


    //[SerializeField] private Transform respawnPosition;

    //[SerializeField] private HUD hudScript;
    //[SerializeField] public CameraEffects cameraEffectsScript;
    //[SerializeField] public CinemachineVCamStates cameraStateScript;

    [SerializeField] private GameObject visuals;
    [Tooltip("The container to hold the player's weapons. On start, all weapon gameObjects that are active will be party of the player's weapons. If GameObject weapon is deactivated then not included. " +
        "Sprite renderer is disabled when that weapon is not being used.")]
    [SerializeField] private GameObject weaponContainer;
    [SerializeField] private GameObject TransformContainer;

    [Header("Fields")]

    [SerializeField] public int maxHealth = 5;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invulnerableTime = 2.0f;
    private bool isDamageable;

    [SerializeField] private bool doMasterVolumeFadeOnHurt;
    [SerializeField] private float changeVolumeToOnHurt;
    [SerializeField] private float volumeChangeDurationOnHurt;
    private float defaultGravity;


    private float verticalInput;
    private float horizontalInput;
    private bool isFrozen;
    private Vector2 move;
    private Vector2 moveAmount;

    private bool isGrounded;
    private bool spawnDust;
    [Space(10)]
    [SerializeField] private bool doCameraShakeOnJump = false;
    [SerializeField][Range(50, 150)] private float LensPrecentageOnCameraJump = 100f;
    [SerializeField] private float cameraShakeDuration;

    [Space(10)]
    [SerializeField] private float attackRange = 0.5f;

    [Tooltip("If true, when a player swaps weapons and there are more than 1 weapon, then the previous weapon's cooldown timer stops")]
    [SerializeField] private bool disableAttackCooldownOnSwap = true;

    private int attackDamage;
    private bool canAttack;

    [SerializeField] private LayerMask enemyLayers;


    [Header("Inventory")]

    [SerializeField] private int currentCurrency = 0;
    [SerializeField] private float currencyMultiplierOnKilled;
    public int keyAmount { get; set; }

    private Dictionary<string, int> inventoryItems = new Dictionary<string, int>();
    private Dictionary<string, GameObject> availableWeapons = new Dictionary<string, GameObject>();


    private GameObject currentWeapon;
    private IEnumerator currentAttackCooldownCoroutine;

    public enum InventoryItemTypes
    {
        None,
        Key,
    }


    [Header("Audio")]
    [SerializeField] private AudioClip landingSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip jumpGrunt;

    [SerializeField] private float jumpGruntChancePercent;

    [SerializeField] private AudioClip hurtSound;

    [SerializeField] private float randomYellRepeatRate;

    [Tooltip("Sounds that are played at random intervals based on the random yell repeat rate")]
    [SerializeField] private AudioClip[] randomSounds;

    [SerializeField] private AudioClip respawnSound;


    [Header("Events")]

    public UnityEvent OnPlayerHeal;
    public event Action OnPlayerDamage;
    public event Action OnPlayerAttack;

    public event Action<int> OnPlayerHealthChanged;
    public event Action<int> OnPlayerCurrencyChanged;

    public event Action OnInteractableButtonPressed;
    public event Action OnTalkButtonPressed;
    public event Action<bool> OnSubmitButtonPressed;
    public event Action OnEscapeButtonPressed;
    public event Action OnInventoryButtonPressed;

    public event Action<GameObject, Component[]> OnTriggerEnter;
    public event Action<GameObject, Component[]> OnTriggerExit;

    [Header("Tools")]
    [SerializeField] private bool hasInfiniteHealth;
    [SerializeField] private bool doNoRespawning;
    [SerializeField] private bool dontLoseCurrencyOnKilled;
    [Tooltip("If true, provide the setup scriptable object with the new inventory item quantities to change the current inventory to be that of the amounts and items specified")]
    [SerializeField] private bool setInventoryItems;

    //[Tooltip("If true, will have the serialized dictionaries (available weapons and inventory) update their keys and values " +
    //  "(note: it may be costly over time as a loop is run everytime a dictionary value is added)")]
    //[SerializeField] private bool displayDictionaryData;

    [SerializeField][Tooltip("If true and player starts a quest, then dialogue that only gets triggered with certain dialogue or choices is triggered from first NPC's interaction")]
    private bool dontRequireCertainDialogueToTriggerQuest;

    [Tooltip("The defined inventory item data that overrides the curent inventory items on Start()")][SerializeField] private InventorySetupSO inventorySetupData;

    [Space(15)]
    [SerializedDictionary("Item Name", "Item Quantity")] public SerializedDictionary<string, int> inventoryItemsDisplay;
    [SerializedDictionary("Weapon Name", "Weapon GameObject")] public SerializedDictionary<string, GameObject> availableWeaponsDisplay;

    private bool result;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        inventoryItems = new Dictionary<string, int>();
        availableWeapons = new Dictionary<string, GameObject>();

        isDamageable = true;
        canAttack = true;
        isFrozen = false;
        defaultGravity = playerRb.gravityScale;

        //placeholder for save system (remove when system is completed)
        SetDefaultInventory();
        //placeholder for save system (remove when system is completed)
        SetBeginningWeapons();

        SetHealth(maxHealth);
        if (setInventoryItems && inventorySetupData != null) SetCurrentInventoryItems(inventorySetupData);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GetCurrentGameState() == GameManager.GameState.MainMenu || GameManager.Instance.GetCurrentGameState() == GameManager.GameState.Loading) SetPlayerVisibility(false);
        else SetPlayerVisibility(true);

        if (!isFrozen)
        {
            if (currentHealth <= 0 && !doNoRespawning && !hasInfiniteHealth) Killed();
            //HandleRespawn();

            if (isGrounded)
            {
                if (spawnDust)
                {
                    if (moveEffect != null) Instantiate(moveEffect, groundCheck.transform.position, Quaternion.identity);
                    spawnDust = false;
                }
            }

            else spawnDust = true;

            if (Input.GetButtonDown("Jump") && !isFrozen)
            {

                if (moveEffect != null) Instantiate(moveEffect, groundCheck.transform.position, Quaternion.identity);
                animator.SetBool("isJumping_b", true);
                isGrounded = false;
                PlaySound(jumpSound, 0.5f, 1.2f, 0.8f);
                float randomChance = UnityEngine.Random.Range(0, 100);
                if (randomChance <= jumpGruntChancePercent) PlaySound(jumpGrunt, 0.95f, 1.05f, 1.0f);

            }

            if (Input.GetButtonUp("Jump") || isGrounded && !isFrozen)
            {
                animator.SetBool("isJumping_b", false);
                isGrounded = false;
            }

            //if (Input.GetKeyDown(KeyCode.Escape)) UnityEngine.Debug.Log("Player has pressed escape!");

            CheckMoveSpeed();
        }
    }

    private void FixedUpdate()
    {
        if (!isFrozen)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            move = new Vector2(horizontalInput, verticalInput);
        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {

    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider != null)
        {
            Component[] objectComponents = collider.gameObject.GetComponents(typeof(Component));
            OnTriggerEnter?.Invoke(collider.gameObject, objectComponents);
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider != null)
        {
            Component[] objectComponents = collider.gameObject.GetComponents(typeof(Component));
            OnTriggerExit?.Invoke(collider.gameObject, objectComponents);
        }
    }


    public void InteractPressed(InputAction.CallbackContext context)
    {
        if (context.performed) OnInteractableButtonPressed?.Invoke();
    }

    public void TalkPressed(InputAction.CallbackContext context)
    {
        if (context.performed) OnTalkButtonPressed?.Invoke();
    }

    public void SubmitPressed(InputAction.CallbackContext context)
    {
        result = false;
        if (context.performed) result = true;
        else if (context.canceled) result = false;
        OnSubmitButtonPressed?.Invoke(result);
    }

    public void EscapePressed(InputAction.CallbackContext context)
    {
        if (context.performed) OnEscapeButtonPressed?.Invoke();
    }

    public void InventoryButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed) OnInventoryButtonPressed?.Invoke();
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && canAttack)
        {
            canAttack = false;
            OnPlayerAttack?.Invoke();

            WeaponSO currentWeaponData = currentWeapon.GetComponent<Weapon>().GetWeaponData();
            int currentDamage = currentWeaponData.weaponDamage;

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(currentWeapon.GetComponent<Weapon>().GetAttackPoint().position, currentWeaponData.attackRange, enemyLayers);
            currentAttackCooldownCoroutine = AttackCooldown(currentWeaponData.cooldownTime);

            UnityEngine.Debug.Log($"Player has done {attackDamage} damage");

            if (hitEnemies.Length > 0)
            {
                foreach (Collider2D enemy in hitEnemies)
                {
                    UnityEngine.Debug.Log("Found enemies to attack!");
                    if (enemy.gameObject.TryGetComponent<Enemy>(out Enemy enemyScript)) enemyScript.TakeDamage(currentDamage);
                    //PlaySound(currentWeaponScript.hitSound[UnityEngine.Random.Range(0, currentWeaponScript.hitSound.Length)], 0.9f, 1.1f, 0.5f);
                    if (damageEffect != null) Instantiate(damageEffect, weaponEdgePoint.position, Quaternion.identity);
                }
            }
            if (currentAttackCooldownCoroutine != null) StartCoroutine(currentAttackCooldownCoroutine);
            else UnityEngine.Debug.Log("The current attack cooldown coroutine is null!");
        }
    }


    private IEnumerator AttackCooldown(float attackCooldown)
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void SwapWeapons(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //UnityEngine.Debug.Log("Swap weapons triggered!");
            if (availableWeapons.Count > 1 && disableAttackCooldownOnSwap)
            {
                canAttack = true;
                if (currentAttackCooldownCoroutine != null) StopCoroutine(currentAttackCooldownCoroutine);
            }
            if (currentWeapon.TryGetComponent<SpriteRenderer>(out SpriteRenderer oldRenderer)) oldRenderer.enabled = false;

            //starting from the current weapon index, keep checking next weapon if its active (if active, then available)
            //for the amount of children in the parent weapon container

            int index = currentWeapon.transform.GetSiblingIndex() + 1;
            for (int i = 0; i < weaponContainer.transform.childCount; i++)
            {
                if (index > weaponContainer.transform.childCount - 1) index = 0;
                if (weaponContainer.transform.GetChild(index).gameObject.activeSelf)
                {
                    currentWeapon = weaponContainer.transform.GetChild(index).gameObject;
                    if (currentWeapon.TryGetComponent<SpriteRenderer>(out SpriteRenderer newRenderer)) newRenderer.enabled = true;
                    break;
                }
                else index++;
            }
        }
    }

    private void SetBeginningWeapons()
    {
        //each weapon gameobject that is active under weapon container is added to weapon dictionary and
        //first one is set as current weapon and other weapon's sprite renderers are disabled
        for (int i = 0; i < weaponContainer.transform.childCount; i++)
        {
            GameObject weapon = weaponContainer.transform.GetChild(i).gameObject;
            if (weapon.activeSelf && weapon.TryGetComponent<Weapon>(out Weapon weaponScript))
            {
                availableWeapons.Add(weaponScript.GetWeaponData().weaponName, weapon);
                if (i == 0)
                {
                    currentWeapon = weapon;
                    currentAttackCooldownCoroutine = AttackCooldown(weaponScript.GetWeaponData().cooldownTime);
                }
                else if (weapon.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer)) spriteRenderer.enabled = false;
            }
        }
    }


    public void PlaySound(AudioClip sound, float minPitch, float maxPitch, float volume)
    {
        audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(sound, volume);
    }

    private void PlayRandomSounds()
    {
        int chanceOfYell = UnityEngine.Random.Range(0, 100);
        if (chanceOfYell > 90)
        {
            PlaySound(randomSounds[UnityEngine.Random.Range(0, randomSounds.Length)], 0.95f, 1.05f, 0.5f);
        }
    }


    private void CheckMoveSpeed()
    {
        if (!Mathf.Approximately(move.x, 0.0f)) animator.SetFloat("speed_f", 10.0f);
        if (Mathf.Approximately(move.x, 0.0f)) animator.SetFloat("speed_f", 0.0f);
    }


    public void TakeDamage(int damage)
    {
        UnityEngine.Debug.Log($"Player was hit for {damage}");
        if (isDamageable)
        {
            isDamageable = false;
            UnityEngine.Debug.Log("Player was hit without iframes!");

            OnPlayerDamage?.Invoke();
            if (damageEffect != null) Instantiate(damageEffect, new Vector2(transform.position.x, transform.position.y + 2.0f), Quaternion.identity);

            //if (doMasterVolumeFadeOnHurt) StartCoroutine(AudioManager.Instance.SetMixerGroupVolume("Music", changeVolumeToOnHurt, volumeChangeDurationOnHurt, true));
            if (animator != null) animator.SetTrigger("hurt_trig");
            if (hurtSound != null) PlaySound(hurtSound, 0.9f, 1.1f, 1.0f);

            SetHealth(currentHealth - damage);
            UnityEngine.Debug.Log($"Player Health: {currentHealth}");

            StartCoroutine(IFrames());
        }
        else return;
    }

    private IEnumerator IFrames()
    {
        UnityEngine.Debug.Log("IFrames started");
        yield return new WaitForSeconds(invulnerableTime);
        isDamageable = true;
        UnityEngine.Debug.Log("IFrames ended!");
    }

    public void Killed()
    {

    }

    public void SetHealth(int health)
    {
        if (!hasInfiniteHealth)
        {
            int clampedHealth = Mathf.Clamp(health, 0, maxHealth);

            UnityEngine.Debug.Log($"Health is set to {health}");

            currentHealth = clampedHealth;
            OnPlayerHealthChanged?.Invoke(clampedHealth);
        }
    }

    public int GetHealth() => currentHealth;

    public void HandleRespawn()
    {
        if (killEffect != null) Instantiate(killEffect, new Vector2(transform.position.x, transform.position.y + 2.0f), Quaternion.identity);

        if (!dontLoseCurrencyOnKilled) currentCurrency = Mathf.RoundToInt(currentCurrency * currencyMultiplierOnKilled);

        PlaySound(respawnSound, 0.95f, 1.05f, 1.0f);
        SetHealth(maxHealth);

        //transform.position = respawnPosition.position;
    }


    public void FreezePlayer(bool freezePlayer)
    {
        if (freezePlayer)
        {
            animator.SetFloat("speed_f", 0.0f);
            isFrozen = true;
        }
        else isFrozen = false;
    }

    //allows script to continue running while player is not seen and is not moving
    private void SetPlayerVisibility(bool visiblity)
    {
        UnityEngine.Debug.Log("Player visiblity is set to " + visiblity);
        FreezePlayer(!visiblity);

        visuals.SetActive(visiblity);
        TransformContainer.SetActive(visiblity);
        weaponContainer.SetActive(visiblity);

        if (visiblity)
        {
            playerRb.constraints = RigidbodyConstraints2D.None;
            playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void SetPlayerPosition(Transform teleportTransform) => transform.position = new Vector2(teleportTransform.position.x, teleportTransform.position.y);


    public void SetCurrency(int newCurrency)
    {
        OnPlayerCurrencyChanged?.Invoke(newCurrency);
        currentCurrency = newCurrency;
    }

    public int GetCurrency() => currentCurrency;

    public void SetInventoryItemQuantity(string inventoryItemName, int newInventoryItemAmount)
    {
        if (inventoryItems.ContainsKey(inventoryItemName)) inventoryItems[inventoryItemName] = newInventoryItemAmount;
        else UnityEngine.Debug.LogWarning($"The argument {inventoryItemName} does not match any keys in the inventory items dictionary!");
    }

    public int GetInventoryItemQuantity(string inventoryItemName)
    {
        if (inventoryItems.ContainsKey(inventoryItemName)) return inventoryItems[inventoryItemName];
        else
        {
            UnityEngine.Debug.LogWarning($"The argument {inventoryItemName} does not match any keys in the inventory items dictionary!");
            return 0;
        }
    }

    public Dictionary<string, int> GetCurrentInventoryItems() => inventoryItems;

    private void SetCurrentInventoryItems(InventorySetupSO inventorySetup)
    {
        foreach (var item in inventorySetup.setInventoryItems) SetInventoryItemQuantity(item.itemType.ToString(), item.itemQuantity);
    }

    //can be called with a button on this script to reset inventory data if new one is provided
    public void UpdateInventoryData() => SetCurrentInventoryItems(inventorySetupData);

    //sets each inventory item type (from enum) to 0
    public void SetDefaultInventory()
    {
        foreach (var inventoryItem in Enum.GetNames(typeof(InventoryItemTypes)))
        {
            if (inventoryItem == InventoryItemTypes.None.ToString()) continue;
            inventoryItems.Add(inventoryItem.ToString(), 0);
            //UnityEngine.Debug.Log($"Added {inventoryItem} to dictionary");
        }
    }

    public void AddWeapon(string weaponName, GameObject gameObject) => availableWeapons.Add(weaponName, gameObject);

    public GameObject GetCurrentWeapon() => currentWeapon;

    public void LoadData(GameData data)
    {
        this.currentCurrency = data.currencyQuantity;
        this.transform.position = data.playerPosition;
        this.inventoryItems= data.inventoryItems;
    }

    public void SaveData(GameData data)
    {
        data.currencyQuantity = this.currentCurrency;
        data.playerPosition = this.transform.position;

        //to convert Json-saved SerializableDictionary type to regular dictionary, we cast it and set data
        Dictionary<string, int> nonSerializedDictionary = new Dictionary<string, int>();
        nonSerializedDictionary = data.inventoryItems as Dictionary<string, int>;
        nonSerializedDictionary = this.inventoryItems;
    }
}