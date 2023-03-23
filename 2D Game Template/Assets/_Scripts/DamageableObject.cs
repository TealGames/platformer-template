using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this script to any object that can be damaged such as a breakable wall, turret, etc. However, do not add it to enemies (or the player)
/// because they already have health and damage systems integrated into scriptable objects and scripts
/// </summary>

public class DamageableObject : MonoBehaviour, IDamageable
{
    [Tooltip("The smallest amount that could be randomly generated for this object's health. If 0, will be ignored and will not generate random health amount")][SerializeField] private int minSpawnHealth;
    [Tooltip("The larest amount that could be randomly generated for this object's health.")] [SerializeField] private int maxSpawnHealth;
    private int currentHealth;
    private int maxHealth;

    private bool isDamageable = true;

    [Header("References")]
    [SerializeField] private ParticleSystem damagedEffect;
    [SerializeField] private ParticleSystem destroyedEffect;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damagedSound;
    [SerializeField] private AudioClip destroyedSound;

    [Tooltip("Must contain the following paramters: damaged_trig, killed_trig")][SerializeField] private Animator animator;

    private void Start()
    {
        if (maxSpawnHealth > 0 && minSpawnHealth > 0) currentHealth = UnityEngine.Random.Range(minSpawnHealth, maxSpawnHealth);
        else if (maxSpawnHealth > 0)
        {
            currentHealth = maxSpawnHealth;
            minSpawnHealth = maxSpawnHealth;

        }
        else UnityEngine.Debug.LogError($"The object {gameObject.name} is a DamageableObject but its max health is not set!");
        maxHealth = currentHealth;
    }

    private void Update()
    {
        if (currentHealth <= 0) Killed();
    }

    public void TakeDamage(int damage)
    {
        if (isDamageable)
        {
            currentHealth= Mathf.Clamp(currentHealth - damage, 0, maxHealth);
            if (audioSource!=null) damagedSound?.PlaySound(audioSource, 0.95f, 1.05f, 1f);
            if (damagedEffect!=null) Instantiate(damagedEffect, transform.parent.transform.position, Quaternion.identity);
            animator?.SetTrigger("damaged_trig");
        }
    }

    public void Killed()
    {
        if (audioSource != null) destroyedSound?.PlaySound(audioSource, 0.95f, 1.05f, 1f);
        if (destroyedEffect != null) Instantiate(destroyedEffect, transform.parent.transform.position, Quaternion.identity);
    }


}
