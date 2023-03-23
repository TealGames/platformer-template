using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyWall : MonoBehaviour
{
    private AudioSource audioSource;
    private Animator animator;
    [SerializeField] private AudioClip keySound;
    [SerializeField] private AudioClip wallSound;

    [SerializeField] private ParticleSystem dustDoorEffect;
    [SerializeField] private int nativeSceneIndex;
    [SerializeField] private bool notKeyWall;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(true);
        audioSource = gameObject.GetComponent<AudioSource>();
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!notKeyWall)
        {
            if (GameManager.lastLoadedSceneIndex == nativeSceneIndex)
            {
                UnityEngine.Debug.Log("The doors scene is active and the door should be visible!");
                EnableObject();
            }
            else DisableObject();
        }

    }

    public void PlayKeySound()
    //used for animation event during the opening wall animation
    {
        audioSource.PlayOneShot(keySound);
    }

    public void PlayWallSound()
    {
        audioSource.PlayOneShot(wallSound);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider != null && collider.gameObject.CompareTag("player"))
        {
            UnityEngine.Debug.Log("Player found in wall hitbox");
            
            if (PlayerCharacter.Instance.keyAmount > 0)
            {
                PlayerCharacter.Instance.keyAmount--;
                animator.SetTrigger("wallOpen_trig");
            }

        }
    }

    public void CloseWall()
    {
        InstantiateDust();
        animator.SetTrigger("wallClose_trig");
    }

    public void OpenWall()
    {
        InstantiateDust();
        animator.SetTrigger("wallOpen_trig");
    }

    public void InstantiateDust()
    {
        Vector2 position = new Vector2(transform.position.x - 6.5f, transform.position.y + 5.0f);
        Instantiate(dustDoorEffect, position, Quaternion.identity);
    }

    void DisableObject()
    {
        SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>(true);
        foreach (var renderer in spriteRenderers)
        {
            renderer.enabled = false;
        }
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
    }

    void EnableObject()
    {
        SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>(true);
        foreach (var renderer in spriteRenderers)
        {
            renderer.enabled = true;
        }
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }
    }
}