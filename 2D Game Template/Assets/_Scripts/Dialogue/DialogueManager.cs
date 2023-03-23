using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
//note: in order to add the freezing effect when dialogue occurs, change the animator for the Dialogue Box animator from "Normal" to "Unscaled Time" and remove
// the commented messages of timeScale, typing speed and yield return.
{

    [SerializeField] private TextMeshProUGUI nameText;  //the name of the NPC
    [SerializeField] private TextMeshProUGUI talkText;  //the text that appears when getting close to an NPC, reminding to press button
    [SerializeField] private TextMeshProUGUI dialogueText;  //the actual spoken text box
    [SerializeField] private TextMeshProUGUI smallerTalkText;
    public TextMeshProUGUI reminderText;  //the continue dialogue button reminder text

    [SerializeField] private string talkTextMessage;
    [SerializeField] private HUD.TextType talkTextType;

    [SerializeField] private float typingSpeed;
    [SerializeField] private float timeBeforeReminder;

    [SerializeField] public AudioSource audioSource;
    [SerializeField] private AudioClip dialogueBoxSound;

    private List<string> sentences;

    [SerializeField] private Animator dialogueBoxAnimator;
    [SerializeField] private GameObject dialogueBox;

    private int sentenceIndex;

    private bool canContinueDialogue = false;
    public bool inConversation { get; private set; }  //this is used to detect when the talk text needs to be activated
    public bool playerInHitbox = false;

    private bool talkTextTriggered = false;

    [SerializeField] private float timedTextDuration;

    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;




    [HideInInspector] public DialogueTrigger currentTriggerScript;
    private Dialogue currentDialogue;
    public static DialogueManager Instance;

    // Start is called before the first frame update
    void Awake()
    {
        sentences = new List<string>();


        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        inConversation = false;

    }

    void Update()
    {

        if (canContinueDialogue)
        {
            StartCoroutine(EnableReminderText());

            if (Input.GetKeyDown(KeyCode.Return))
            {
                audioSource.PlayOneShot(dialogueBoxSound);

                currentTriggerScript.TriggerVoice();

                StopCoroutine(EnableReminderText());
                sentenceIndex++;

                if (sentenceIndex == sentences.Count)
                {
                    // UnityEngine.Debug.Log("sentence count is 0");
                    EndDialogue();
                }
                else
                {
                    DisplayNextSentence(sentences[sentenceIndex]);
                }
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        OnDialogueStart?.Invoke();

        currentDialogue = dialogue;

        PlayerCharacter.Instance.FreezePlayer(true);
        inConversation = true;

        audioSource.PlayOneShot(dialogueBoxSound);

        currentTriggerScript.TriggerVoice();

        dialogueBox.gameObject.SetActive(true);
        dialogueBoxAnimator.SetBool("isOpen", true);

        nameText.text = dialogue.characterName;

        foreach (string sentence in currentDialogue.sentences)
        {
            sentences.Add(sentence);
            // UnityEngine.Debug.Log($"{sentence} is added"); 
        }

        sentenceIndex = 0;

        StartCoroutine(TypeSentence(sentences[sentenceIndex]));
    }

    public void DisplayNextSentence(string currentSentence)
    {
        reminderText.gameObject.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    public void EndDialogue()
    {
        OnDialogueEnd?.Invoke();

        sentences.Clear();

        canContinueDialogue = false;
        StopAllCoroutines();
        reminderText.gameObject.SetActive(false);
        //prevents the reminder text from popping up after done talking

        inConversation = false;
        PlayerCharacter.Instance.FreezePlayer(false);
        dialogueBoxAnimator.SetBool("isOpen", false);

        currentDialogue = null;
    }

    IEnumerator TypeSentence(string sentence)
    //this creates the typing effect when the dialogue box text appears
    {
        dialogueText.text = "";
        canContinueDialogue = false;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        canContinueDialogue = true;
    }

    IEnumerator EnableReminderText()
    {
        if (canContinueDialogue)
        //makes it so that timer for reminder text starts when the sentence is done
        {
            yield return new WaitForSeconds(timeBeforeReminder);
            reminderText.text = "Press \"Enter\" to Continue Dialogue";
            reminderText.gameObject.SetActive(true);
        }

        else yield return null;

    }

    public void EnableTalkText()
    //this serves as a method for events to show the talk text (text that appears for reminding to press button)
    {
        HUD.Instance.EnableText(talkTextType, talkTextMessage);
    }

    public void DisableTalkText()
    {
        HUD.Instance.DisableText(talkTextType);
    }

    public void ShowReminderText(string text)
    //this serves as a method for events to show the reminder text (text that appears in small letters)
    {
        reminderText.text = text;
        //talkTextTriggered = true;
        reminderText.gameObject.SetActive(true);
    }

    public void DisableReminderText()
    {
        //talkTextTriggered = false;
        reminderText.gameObject.SetActive(false);
    }

    public void ShowTimedSmallerTalkText(string text)
    {
        smallerTalkText.text = text;
        smallerTalkText.gameObject.SetActive(true);
        StartCoroutine(TextTimer());
    }

    IEnumerator TextTimer()
    {
        UnityEngine.Debug.Log("Timer for text began!");
        yield return new WaitForSeconds(timedTextDuration);
        UnityEngine.Debug.Log("Timer ran out!");
        smallerTalkText.gameObject.SetActive(false);
    }
}