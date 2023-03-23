using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This script manages collisions with the player and applies damage and other things upon collision
/// On contactCollision with this object, that object will get damaged or have another action applied
/// </summary>

public class DamagingObject : MonoBehaviour
{
    [SerializeField] private int damageDealt;
    public enum CollisionType
    {
        Contact,
        Trigger,
    }

    [Tooltip("Note: If a contact contactCollision, then the collider must NOT be a trigger. If a trigger, then collider must be a trigger, too")][SerializeField] private CollisionType collisionType;
    [SerializeField] private Collider2D objectCollider;
    private bool collidable = true;

    [SerializeField] private AudioSource audioSource;
    [Tooltip("The sound played when an object that can get damaged by this object (most of the time player) collides with this object")][SerializeField] private AudioClip collisionSound;
    [Tooltip("The multiplier of the bounce applied to the player after colliding with this object. Leave it as 0 to not have any multiplier")][Range(0,50)][SerializeField] private float bounceMultiplier;
    [Tooltip("After colliding with this object, the position to set the object. Leave this blank to not have any respawn position. ")][SerializeField] private Transform respawnTransform;

    public event Action OnPlayerCollision;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void Update()
    {

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null)
        {
            if (objectCollider != null && objectCollider.isTrigger && collisionType == CollisionType.Contact)
                UnityEngine.Debug.LogError($"{gameObject.name} is set to be a contact damaging object, but the collider is set as a trigger!");
            if (collision.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript) &&
                objectCollider!= null && !objectCollider.isTrigger && collisionType == CollisionType.Contact) HandleCollision(contactCollision: collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider!=null)
        {
            if (objectCollider != null && !objectCollider.isTrigger && collisionType == CollisionType.Trigger)
                UnityEngine.Debug.LogError($"{gameObject.name} is set to be a trigger damaging object, but the collider is not set to a trigger!");
            if (collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript) &&
                objectCollider != null && objectCollider.isTrigger && collisionType == CollisionType.Trigger) HandleCollision();
        }
    }

    //optional contactCollision parameter needed when contact colliding to be able to apply force back
    public void HandleCollision(Collision2D contactCollision= null)
    {
        UnityEngine.Debug.Log("The player has entered collider!");
        if (collidable)
        {
            OnPlayerCollision?.Invoke();
            if (damageDealt > 0) PlayerCharacter.Instance.TakeDamage(damageDealt);
            if (collisionSound != null && audioSource != null) audioSource.PlayOneShot(collisionSound);

            if (contactCollision != null)
            {
                Vector2 bounceForce = -(contactCollision.GetContact(0).normal * bounceMultiplier);
                PlayerCharacter.Instance.gameObject.GetComponent<Rigidbody2D>().AddForce(bounceForce, ForceMode2D.Impulse);
                UnityEngine.Debug.Log($"{PlayerCharacter.Instance.gameObject.GetComponent<Rigidbody2D>()} and the bounce force is {bounceForce}");
            }
            if (respawnTransform != null) PlayerCharacter.Instance.SetPlayerPosition(respawnTransform);
        }
    }

    public void SetCollidable(bool newCollidable) => collidable = newCollidable;

}