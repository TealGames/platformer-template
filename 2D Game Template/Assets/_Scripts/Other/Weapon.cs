using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Contains weapon's data, draws attack range of this weapon and keeps track of when to disable/enabled collider and play weapon-specific sounds based
/// what this object's parent collider hits (Note: this script does NOT do damage- that is done in the Attack() method of the player). The weapon's collider is for detecting
/// specific hit objects and play the corresponding sounds, but the attack damage is done based on a Physics2D overlap circle in Attack()
/// </summary>

public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponSO weaponData;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Color weaponRangeColor;
    private AudioSource audioSource;
    private Collider2D collider;

    [Header("Tools")]
    [SerializeField] private bool drawAttackRange;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        collider = gameObject.GetComponent<Collider2D>();
        gameObject.name = weaponData.weaponName;
    }

    // Update is called once per frame
    void Update()
    {
        //if this weapon is the current weapon being used, make collider visible, else disable collider
        if (PlayerCharacter.Instance.GetCurrentWeapon().gameObject == transform.parent.gameObject) collider.enabled = true;
        else collider.enabled = false;
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider!=null)
        {
            //if weapon collider collides with object with enemy script, damage it and play random hit sound
            if (collider.gameObject.TryGetComponent<Enemy>(out Enemy enemyScript))
            {
                if (weaponData.swingSounds.Length > 0)
                {
                    int randomSoundIndex = UnityEngine.Random.Range(0, weaponData.hitSounds.Length);
                    PlaySound(weaponData.hitSounds[randomSoundIndex], 0.95f, 1.05f, 1.0f);
                }
            }

            //if weapon collides with an object with a material script (and a material type), play that weapon's corresponding
            //material sound
            else if (collider.gameObject.TryGetComponent<Material>(out Material materialScript))
            {
                Material.MaterialType collidedMaterial = materialScript.GetMaterialType();
                if (collidedMaterial != Material.MaterialType.None)
                {
                    for (int i = 0; i < weaponData.materialSounds.Length; i++)
                    {
                        if (weaponData.materialSounds[i].materialType == collidedMaterial)
                        {
                            PlaySound(weaponData.materialSounds[i].materialSound, 0.95f, 1.05f, 1);
                            break;
                        }
                    }
                }
                
            }
            //if no other types of colliders are found (swinging in air), play a random swing sound
            else
            {
                if (weaponData.swingSounds.Length>0)
                {
                    int randomSoundIndex = UnityEngine.Random.Range(0, weaponData.swingSounds.Length);
                    PlaySound(weaponData.swingSounds[randomSoundIndex], 0.95f, 1.05f, 1.0f);
                }
            }
        }
    }

    private void PlaySound(AudioClip sound, float minPitch, float maxPitch, float volume)
    {
        audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(sound, volume);
    }

    public WeaponSO GetWeaponData() => weaponData;

    public Transform GetAttackPoint() => attackPoint;

    public void AddThisWeaponToPlayerWeapons()
    {
        gameObject.SetActive(true);
        PlayerCharacter.Instance.AddWeapon(weaponData.weaponName, gameObject);
    }

    private void OnDrawGizmos()
    {
        if (drawAttackRange)
        {
            //draws sphere for attack range
            Gizmos.color = weaponRangeColor;
            Gizmos.DrawWireSphere(attackPoint.position, weaponData.attackRange);
        }
        
    }
}