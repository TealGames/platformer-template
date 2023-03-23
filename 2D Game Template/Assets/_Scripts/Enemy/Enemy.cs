using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] protected EnemySO baseEnemyData;
    //[Header("General")]
    //[Tooltip("The max amount of health when enemy spawns/activates")] [SerializeField] protected int maxEnemyHealth;
    //[Tooltip("The min amount of health when enemy spawns/activates. Note: this value CANNOT be 0")] [SerializeField] protected int minEnemyHealth;
    //[SerializeField] protected bool haveRandomHealthAmount;
    protected int currentEnemyHealth;

    //[Tooltip("The amount of time after the player has damaged the enemy that they can be damaged again")] [SerializeField] protected float invulnerableTime = 0.5f;
    protected bool canGetDamaged = true;
    
    //[Tooltip("The amount of damage to do when the enemy collides with the player")][SerializeField] protected int amountToDamage;
    //[Tooltip("If true, contact damage will be the same as amountToDamage")][SerializeField] protected bool doContactDamage;


    //[Tooltip("The radius of the circle with center being alertCenter's position. When player enters this circle, the things that enemy does depends on the classes that inherit this class")]
    //[SerializeField] protected float alertRange;
    [SerializeField] protected Transform alertCenterTransform;
    protected bool playerInAlertRange = false;

    //[Tooltip("If true, will show the alert visual cue when player enters the alert circle")][SerializeField] protected bool showAlertCue = false;
    private bool canShowCue = false;
    //[Tooltip("After this amount of time, the alert cue icon will disappear. If show alert cue is true and alert cue time is 0, it will always be shown when alerted")][SerializeField] protected float alertCueTime;

    [Header("References")]
    //[SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected Animator animator;
    [SerializeField] protected AudioSource audioSource;
    protected Transform target;
    [SerializeField] protected GameObject alertVisualCue;

    [SerializeField] protected AudioClip hurtSound;
    [SerializeField] protected AudioClip killedSound;

    [SerializeField] protected ParticleSystem damageEffect;
    [SerializeField] protected ParticleSystem killedEffect;
    [Tooltip("The position where particles and droppable items when enemy gets killed will be instantiated")][SerializeField] protected Transform instantiateTransform;

    [Header("Dropables")]
    [SerializeField] protected EnemyDrops[] enemyDrops;

    public class EnemyDrops
    {
        public GameObject dropPrefab;
        [Tooltip("If set to 1, drop change determines drop. Otherwise, randomly generated between min and max")] public int maxDropAmount;
        [Tooltip("If set to 1, drop change determines drop. Otherwise, randomly generated between min and max")] public int minDropAmount;
        [Tooltip("The chance to drop the item from 0 to 100 percent. If amount to drop is 1, then will generate drop chance precentage. If 0, will not drop the item ever.")]
        [Range(0, 100)]
        public float dropChancePrecentage = 100;
    }

    public enum EnemyState
    {
        Active,
        Killed,
    }
    private EnemyState currentState;



    public event Action OnEnemyHit;
    public event Action OnEnemyDestroyed;
    public event Action OnEnemyAlerted;

    // Start is called before the first frame update
    protected void Start()
    {
        target = PlayerCharacter.Instance.transform;
        alertVisualCue.SetActive(false);
        currentState= EnemyState.Active;

        if (baseEnemyData.haveRandomHealthAmount && baseEnemyData.minEnemyHealth !=0) 
            currentEnemyHealth= UnityEngine.Random.Range(baseEnemyData.minEnemyHealth, baseEnemyData.maxEnemyHealth);
        else
        {
            currentEnemyHealth = baseEnemyData.maxEnemyHealth;
            baseEnemyData.minEnemyHealth = baseEnemyData.maxEnemyHealth;
        }
        
    }

    // Update is called once per frame
    protected void Update()
    {
        if (currentEnemyHealth <= 0 && currentState == EnemyState.Active) Killed();
    }

    protected void FixedUpdate()
    {
        //if player enters the enemy's alert range
        if (Physics2D.OverlapCircle(alertCenterTransform.position, baseEnemyData.alertRange, baseEnemyData.playerLayer)==PlayerCharacter.Instance.GetComponent<Collider2D>())
        {
            UnityEngine.Debug.Log("Player in alert range: TRUE");
            OnEnemyAlerted?.Invoke();
            playerInAlertRange = true;
            if (baseEnemyData.showAlertCue && canShowCue)
            {
                alertVisualCue.SetActive(true);
                canShowCue = false;
                if (baseEnemyData.alertCueTime >0f) StartCoroutine(AlertCueTime());
            }
        }
        else
        {
            UnityEngine.Debug.Log("Player in alert range: FALSE");
            canShowCue = true;
            playerInAlertRange = false;
            alertVisualCue.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        if (canGetDamaged)
        {
            canGetDamaged = false;
            OnEnemyHit?.Invoke();
            if (damageEffect!=null) Instantiate(damageEffect, instantiateTransform.position, Quaternion.identity);
            currentEnemyHealth -= damage;

            UnityEngine.Debug.Log($"Enemy Health: {currentEnemyHealth}");
            if (animator != null) animator.SetTrigger("hurt_trig");

            PlaySound(hurtSound, 0.8f, 1.2f, 0.5f);
            StartCoroutine(InvulnerableTime());
        }
        
    }

    public void Killed()
    {
        currentState = EnemyState.Killed;

        OnEnemyDestroyed?.Invoke();

        PlaySound(killedSound, 1f, 1f, 1f);
        SpawnDrops();

        if (killedEffect != null) Instantiate(killedEffect, instantiateTransform.position, Quaternion.identity);

        UnityEngine.Debug.Log($"Enemy {gameObject.name} is destroyed!");

        if (animator == null) transform.parent.gameObject.SetActive(false);
        else animator.SetTrigger("killed_trig");
    }


    protected void PlaySound(AudioClip sound, float minPitch, float maxPitch, float volume)
    {
        audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(sound, volume);
    }

    protected void SpawnDrops()
    {
        if (enemyDrops!=null)
        {
            foreach (var drop in enemyDrops)
            {
                //if only drop 1 item, then generate chance if to drop it
                if (drop.minDropAmount == 1 && drop.maxDropAmount == 1)
                {
                    if (HelperFunctions.GenerateRandomPrecentage() <= drop.dropChancePrecentage)
                        Instantiate(drop.dropPrefab, instantiateTransform.position, Quaternion.identity);
                }
                //otherwise the drop amount is a random amount of min and max drop amounts
                else
                {
                    int dropAmount = UnityEngine.Random.Range(drop.minDropAmount, drop.maxDropAmount);
                    for (int i = 0; i < dropAmount; i++)
                    {
                        Instantiate(drop.dropPrefab, instantiateTransform.position, Quaternion.identity);
                    }
                }
            }
        }
        
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
        {
            if (baseEnemyData.doContactDamage) playerScript.TakeDamage(baseEnemyData.amountToDamage);
        }
    }

    protected IEnumerator InvulnerableTime()
    {
        yield return new WaitForSeconds(baseEnemyData.invulnerableTime);
        canGetDamaged = true;
    }

    protected IEnumerator AlertCueTime()
    {
        UnityEngine.Debug.Log("Alert cue time started!");
        yield return new WaitForSeconds(baseEnemyData.alertCueTime);
        alertVisualCue.SetActive(false);
    }


}
