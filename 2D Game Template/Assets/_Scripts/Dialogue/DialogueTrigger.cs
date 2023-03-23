using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;

/// <summary>
/// This script is where you write dialogue and detect collisions of the player in the Collider and triggers the dialogue when a certain button is pressed.
/// Place this script as a child object of NPCs with dialogue or place it in the default layer in the Parallax Layers Object to trigger with no conection to
/// an NPC.
/// </summary>

public class DialogueTrigger : MonoBehaviour
//note: the player must have a tag named "player" for the collision to be detected
{
    [Header("Fields")]

    [SerializeField] private AudioSource audioSource;

    [SerializeField] [Tooltip("Automatically gets triggered when player enters collider")] private bool automaticTrigger;
    [SerializeField] [Tooltip("Disables this gameobject when player exits collider")] private bool oneTimeTrigger;
    private bool playerInHitbox = false;

    [SerializeField] [Tooltip("Plays voice sound every time dialogue is started with a character")] private bool doOnEnterVoice;       
    [SerializeField] [Tooltip("Plays voice sound every time a new dialogue element is started")] private bool doEverySentenceVoice;

    
    [SerializeField] [Tooltip("All the potential talk sounds used (if not random, just cycles through each one in order)")] private AudioClip[] talkSounds;
    [SerializeField] [Tooltip("Randomly picks a talk sound from array")] private bool doRandomTalkSounds;

    [SerializeField] [Tooltip("If not set to 0, has a talk cooldown to prevent player from skipping dialogue")] private float continueDialogueCooldown = 0.0f;
    private bool canTalk;
    //determines when the player presses the talk button and can talk (not changed when automatic trigger)
    private bool canContinueDialogue = true;
    private bool inConversation = false;

    [SerializeField] private Dialogue dialogue;

    [Header("Quests")]
    [SerializeField] private bool isQuestNPC;
    [SerializeField] private ParticleSystem completionEffect;
    [SerializeField] private int coinsToCollect;

    [SerializeField] private Dialogue afterQuestCompletionDialogue;
    private bool questCompleted = false;

    [SerializeField] private float onCompleteChangeVolumeTo;
    [SerializeField] private float onCompleteVolumeChangeDuration;

    
    




    public UnityEvent onQuestCompletion;



    void Start()
    {
        PlayerCharacter.Instance.OnTalkButtonPressed += TalkPressed;

        DialogueManager.Instance.OnDialogueStart+= () => inConversation = true;
        DialogueManager.Instance.OnDialogueEnd += () =>
        {
            DialogueManager.Instance.EnableTalkText();
            inConversation = false;
            PlayerCharacter.Instance.OnTalkButtonPressed += TalkPressed;
        };

        if (automaticTrigger) canTalk = true;
        else canTalk = false;
    }


    void Update()
    {
        if (playerInHitbox && !automaticTrigger && canTalk && canContinueDialogue)
        {
            canTalk = false;
            if (continueDialogueCooldown > 0.0f)
            {
                canContinueDialogue = false;
                StartCoroutine(talkCooldown(continueDialogueCooldown));
            }

            DialogueManager.Instance.currentTriggerScript = this;
            //this is needed so that the voices played will be from the trigger script being activated

            if (isQuestNPC && (coinsToCollect > 0 && PlayerCharacter.Instance.GetCurrency() >= coinsToCollect) || questCompleted)
            //if its a quest NPC and the player has completed the coin quest or the quest has alread been completed play this dialogue
            {
                if (!questCompleted) PlayerCharacter.Instance.SetCurrency(PlayerCharacter.Instance.GetCurrency() - coinsToCollect);

                DialogueManager.Instance.StartDialogue(afterQuestCompletionDialogue);
                if (!questCompleted) onQuestCompletion.Invoke();
                questCompleted = true;

                Vector2 spawnLocation = new Vector2(PlayerCharacter.Instance.transform.position.x, PlayerCharacter.Instance.transform.position.y + 4.0f);
                Instantiate(completionEffect, spawnLocation, Quaternion.identity);
            }

            else
            {
                DialogueManager.Instance.StartDialogue(dialogue);
            }

            if (Input.GetKeyDown(KeyCode.T) && canTalk)
            {
                
               
                    
                    
                    


                
            }
        }
    }

    private void TalkPressed()
    {
        if (!automaticTrigger)
        {
            canTalk = true;
            PlayerCharacter.Instance.OnTalkButtonPressed -= TalkPressed;
        }
        
    }

    private IEnumerator talkCooldown(float talkCooldown)
    {
        yield return new WaitForSeconds(talkCooldown);
        canContinueDialogue = true;
    }



    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }

    public void TriggerVoice()
    {
        if (doEverySentenceVoice)
        {
            int index = UnityEngine.Random.Range(0, talkSounds.Length);
            audioSource.PlayOneShot(talkSounds[index]);
            // UnityEngine.Debug.Log($"The audioclip {talkSounds[index].name} was played");
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider != null && collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
        {
            DialogueManager.Instance.currentTriggerScript = this;
            DialogueManager.Instance.EnableTalkText();
            playerInHitbox = true;

            PlayerCharacter.Instance.FreezePlayer(true);
            PlayerCharacter.Instance.gameObject.GetComponent<Animator>().SetFloat("Speed_f", 0.0f);

            if (automaticTrigger && canTalk)
            {
                




                canTalk = false;
                StartCoroutine(talkCooldown(continueDialogueCooldown));
                
                
                TriggerDialogue();

            }
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider != null && collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
        {
            // talkText.gameObject.SetActive(false);
            playerInHitbox = false;
            DialogueManager.Instance.DisableTalkText();
            PlayerCharacter.Instance.FreezePlayer(false);
            if (automaticTrigger)
            {
                if (oneTimeTrigger) gameObject.SetActive(false);

            }
        }
    }

}