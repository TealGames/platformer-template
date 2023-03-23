using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BreakableWall : MonoBehaviour
{



    //[SerializeField] private AudioClip bambooBreak;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private int timesHitToBreak = 8;
    private int currentTimesHit = 0;

    bool activatedOnce = false;   //these prevents the sound layering effect when bamboo break sound plays in Update()

    private TextMeshProUGUI reminderText;

    [SerializeField] private ParticleSystem breakEffect;
    [SerializeField][Tooltip("Precent chance out of 100 to do particle effect on hit")] private float particleEffectPercent;


    // Start is called before the first frame update
    void Start()
    {
        reminderText = DialogueManager.Instance.reminderText;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTimesHit == timesHitToBreak && !activatedOnce)
        {
            //audioSource.PlayOneShot(bambooBreak, 0.5f);
            UnityEngine.Debug.Log("wall should destroy");
            animator.SetTrigger("destroy_trig");
            activatedOnce = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.CompareTag("sword") && !activatedOnce)
            {


                animator.SetTrigger("hit_trig");
                currentTimesHit++;

                float chance = UnityEngine.Random.Range(0, 100);
                Vector2 position = new Vector2(transform.position.x, transform.position.y + 7.0f);
                if (chance <= particleEffectPercent) Instantiate(breakEffect, position, Quaternion.identity);


            }
        }
    }
}