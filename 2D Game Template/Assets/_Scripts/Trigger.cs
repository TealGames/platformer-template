using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField][Tooltip("On enter, it turns inactive")] protected bool disableOnEnter;
    [SerializeField][Tooltip("On exit, it turns inactive")] protected bool disableOnExit;
    [SerializeField] protected ParticleSystem enterEffect;
    [Tooltip("If null, then spawns particles in this object's position")][SerializeField] protected Transform particleSpawnLocation;
    [SerializeField] protected AudioClip enterSound;
    [SerializeField] protected AudioSource audioSource;

    protected bool inTrigger = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void OnEnter(Collider2D collider)
    {
        if (collider != null && collider.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
        {
            inTrigger = true;
            if (audioSource != null && enterSound != null) enterSound.PlaySound(audioSource, 0.95f, 1.05f, 1f);
            if (enterEffect!=null)
            {
                if (particleSpawnLocation != null) Instantiate(enterEffect, particleSpawnLocation.position, Quaternion.identity);
                else Instantiate(enterEffect, transform.position, Quaternion.identity);
            }
            if (disableOnEnter) gameObject.SetActive(false);
        }
    }

    protected void OnExit(Collider2D collider)
    {
        if (collider != null && collider.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
        {
            inTrigger = false;
            if (disableOnExit) gameObject.SetActive(false);
        }
    }

}
