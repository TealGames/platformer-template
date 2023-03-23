using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[System.Serializable]
public class ItemQuest
{
    public PlayerCharacter.InventoryItemTypes itemType;
    public int itemQuantity;
}

[System.Serializable]
public class CollectibleQuest
{
    public Collectible.CollectibleType collectibleType;
    public int collectibleQuantity;
}

[System.Serializable]
public class Quest
{
    [Tooltip("Allows other scripts to identity different quests and complete them, which is helpful if there are many quests at the same time")] 
    public string questName;

    [Tooltip("NPC where you complete a task to gain reward. Perform task NPC's must have another script trigger call \"QuestComplete(DIALOGUE TRIGGER NAME)\" to complete it.")]
    public bool performTask;

    [Tooltip("Default is that NPC has quest set on initalization. If true, set a tag with \"#TRIGGER_QUEST:\" to trigger quest when cerrtain dialogue or choice is made")]
    public bool dialogueTriggersQuest;

    [Tooltip("Types of possible inventory item types for a quest")] 
    public ItemQuest[] itemQuestItems;

    [Tooltip("Types of possible collectible item types for a quest")]
    public CollectibleQuest[] collectibleQuestItems;

    public UnityEvent OnQuestComplete;

    [HideInInspector] public bool questCompleted = false;
    [HideInInspector] public bool questStarted = false;
    //add options for multiple quests to be active at the same time, or only one at a time
}




public class InkDialogueTrigger : MonoBehaviour
{
    [SerializeField] private GameObject talkVisualCue;
    private bool playerInCollider = false;
    private bool talkPressed = false;
    private bool firstEncounter = true;

    [SerializeField] private TextAsset inkJSON;

    [SerializeField] private string characterName;
    [SerializeField] [Tooltip("If true, will replay an NPC's dialogue from the start after starting dialogue again")] private bool replayDialogueFromStartAfterFinish;
    [SerializeField] [Tooltip("Automatically gets triggered when player enters collider")] private bool automaticTrigger;


    [Header("Audio")]

    [SerializeField] private AudioSource audioSource;
    [SerializeField] [Tooltip("Default talk sounds play iterating through array in order (and restarting after index 0)")] private AudioClip[] talkSounds;
    private int indexToPlay = 0;
    [SerializeField] [Tooltip("If true, everytime this NPC's dialogue is played, randomly choosen from talkSounds[] if length is greater than 1")] private bool doRandomTalkSounds;

    [SerializeField] [Tooltip("Plays voice sound every time dialogue is started with a character")] private bool playTalkSoundOnEnter;
    [SerializeField] [Tooltip("Plays voice sound every time a new dialogue element is started")] private bool playTalkSoundEveryDialogueLine;
    [SerializeField] private AudioClip questCompleteSound;


    [Header("Quests")]

    [SerializeField] private bool isQuestNPC;
    [SerializeField] [Tooltip("Create quests and allow NPC's to have multiple quests at the same time or right after each other")] private QuestSO[] quests;
    [SerializeField] [Tooltip("Default is that all non trigger quests are activated from start. If true, non trigger quests will be started after the previous one is done (Meaning there will not be 2 non trigger quests active at the same time)")]
    private bool triggerSequentially;

    public UnityEvent OnQuestComplete;

    private int currentQuestIndex = 0;

    [SerializeField] private ParticleSystem completionEffect;

    //private bool questCompleted = false;
    //private bool questStarted = false;

    [Header("Tags")]
    [SerializeField] private const string questTriggerTag = "TRIGGER_QUEST:";

    // Start is called before the first frame update
    void Start()
    {
        PlayerCharacter.Instance.OnTalkButtonPressed += TalkPressed;
        //InkDialogueManager.Instance.OnDialogueEnd += DialogueEnd;
        InkDialogueManager.Instance.OnTagsFound += CompleteTagActions;
        

        talkVisualCue.SetActive(false);    
        if ((playTalkSoundEveryDialogueLine || playTalkSoundOnEnter) && talkSounds.Length > 0) InkDialogueManager.Instance.OnNextLineOfDialogue += PlayTalkSound;
    }

    // Update is called once per frame
    void Update()
    {
        if (talkPressed || (automaticTrigger && playerInCollider))
        {
            talkPressed = false;
            PlayerCharacter.Instance.OnTalkButtonPressed -= TalkPressed;
            talkVisualCue.SetActive(false);
            InkDialogueManager.Instance.StartDialogue(inkJSON, characterName, transform.parent.gameObject);

            //if its a quest NPC on the first encounter (to prevent quest being started multiple times)
            //then foreach non completed, non trigger quest, set it to start
            if (isQuestNPC && quests.Length > 0)
            {
                if (firstEncounter)
                {
                    foreach (var quest in quests)
                    {
                        //check to make sure that no collectible quest item is set as an inventory item
                        foreach (var collectible in quest.collectibleQuestItems)
                        {
                            if (collectible.collectibleType == Collectible.CollectibleType.InventoryItem)
                                UnityEngine.Debug.LogError($"{gameObject.name} has set the collectible type as an inventory item and that is an invalid option for a collectible quest!");
                        }
                        

                        //if not a trigger quest and quest is not triggered in order, that quest is activated from talking at the start
                        if (!quest.questCompleted && !triggerSequentially && !quest.dialogueTriggersQuest) quest.questStarted = true;
                    }
                    //if sequentially activated, just being the first one as active
                    if (triggerSequentially) quests[0].questStarted = true;
                    firstEncounter = false;
                }
                else
                {
                    //foreach quest, if its not completed, not a task quest and there are items to collect, check if its completed
                    foreach (var quest in quests)
                    {
                        if (!quest.questCompleted && !quest.performTask && (quest.collectibleQuestItems.Length > 0 || quest.itemQuestItems.Length > 0)) QuestCompleteCheck();
                    }
                }
                
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject != null && collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
        {
            UnityEngine.Debug.Log("Player in collider");
            playerInCollider = true;
            talkVisualCue.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject != null && collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
        {
            playerInCollider = false;
            talkVisualCue.SetActive(false);
        }
    }

    private void TalkPressed()
    {
        if (playerInCollider) talkPressed = true;
    }

    public void DialogueEnd()
    {
        PlayerCharacter.Instance.OnTalkButtonPressed += TalkPressed;
        talkVisualCue.SetActive(true);
    }

    private void PlayTalkSound()
    {

        if (doRandomTalkSounds)
        {
            int randomIndex = UnityEngine.Random.Range(0, talkSounds.Length);
            audioSource.PlayOneShot(talkSounds[randomIndex]);
        }
        else
        {
            if (indexToPlay > talkSounds.Length) indexToPlay = 0;
            audioSource.PlayOneShot(talkSounds[indexToPlay]);
            indexToPlay++;
        }

        if (playTalkSoundOnEnter && !playTalkSoundEveryDialogueLine) InkDialogueManager.Instance.OnNextLineOfDialogue -= PlayTalkSound;
    }

    private void QuestCompleteCheck()
        //checks each quest, if it has been started, then check if it meets requirements to complete
        //note: dialogue trigger quests will not be checked initially, but once tag is found, they will be started
    {
        if (quests.Length>0)
        {
            foreach (var quest in quests)
            {
                if (quest.questStarted)
                //foreach quest that has been started, if it gets all the items necessary, continue checking the other items, if not return from method
                {

                    if (quest.itemQuestItems.Length > 0)
                    {
                        for (int i = 0; i < quest.itemQuestItems.Length; i++)
                        {
                            if (PlayerCharacter.Instance.GetInventoryItemQuantity(quest.itemQuestItems[i].itemType.ToString()) == quest.itemQuestItems[i].itemQuantity) continue;
                            else return;
                        }
                    }
                    if (quest.collectibleQuestItems.Length > 0)
                    {

                        for (int i = 0; i < quest.collectibleQuestItems.Length; i++)
                        //checks to make sure that player's health or player's currency matches the required collectible quest amount
                        {
                            if (quest.collectibleQuestItems[i].collectibleType == Collectible.CollectibleType.InventoryItem)
                                UnityEngine.Debug.LogError($"{gameObject.name} has set the collectible type as an inventory item and that is an invalid option for a collectible quest!");
                            if (quest.collectibleQuestItems[i].collectibleType == Collectible.CollectibleType.Health && PlayerCharacter.Instance.GetHealth() == quest.collectibleQuestItems[i].collectibleQuantity) continue;
                            else if (quest.collectibleQuestItems[i].collectibleType == Collectible.CollectibleType.Currency && PlayerCharacter.Instance.GetCurrency() == quest.collectibleQuestItems[i].collectibleQuantity) continue;
                            else return;
                        }
                    }
                    //if not returned from method, it means it met all requirements, so then we can complete that quest
                    CompleteQuest(quest.questName);
                }
                else UnityEngine.Debug.LogError($"{quest.questName} in {gameObject.name} is being checked if it is completed, but is has not started yet!");     
            }
        }
    }

    private void CompleteQuest(string questName)
    {
        bool questFound = false;
        //search through quests looking for argument
        foreach (var quest in quests)
        {
            if (quest.questName== questName)
                //once quest is found, change variables and invoke that quest's completion
                //as well as general completion actions for that quest
            {
                questFound = true;
                quest.questCompleted = true;
                quest.questStarted = false;
                OnQuestComplete.Invoke();

                if (questCompleteSound != null) audioSource.PlayOneShot(questCompleteSound);
                if (completionEffect != null)
                {
                    Vector2 spawnPos = new Vector2(PlayerCharacter.Instance.transform.position.x, 1.5f * PlayerCharacter.Instance.transform.position.y);
                    Instantiate(completionEffect, spawnPos, Quaternion.identity);
                }

                if (triggerSequentially)
                {
                    currentQuestIndex++;
                    quests[currentQuestIndex].questStarted= true;
                }
                break;
            }
        }
        if (!questFound) UnityEngine.Debug.Log($"Argument {questName} was not found in {gameObject.name}'s quests. Make sure it is spelled correctly!");
    }

    //allows for other scripts to trigger completed quests for other game objects
    public void CompleteQuest(InkDialogueTrigger triggerInstance, string questName) => triggerInstance.CompleteQuest(questName);

    private void CompleteTagActions(GameObject currentNPCTalking, string[] tags)
    {
        //To prevent errors when talking after scene changes, we check to make sure talking NPC is the same as this script
        if (currentNPCTalking!=null && currentNPCTalking.GetComponentInChildren<InkDialogueTrigger>() == this)
        {
            foreach (string tag in tags)
            {
                UnityEngine.Debug.Log(tag);
                if (tag.Contains(questTriggerTag))
                {
                    string questName = tag.Remove(0, questTriggerTag.Length);
                    foreach (var quest in quests)
                    {
                        UnityEngine.Debug.Log($"{quest.questName} {questName} {quest.questCompleted}");
                        if (quest.questName.ToLower() == questName.ToLower() && quest.dialogueTriggersQuest && !quest.questCompleted) quest.questStarted = true;
                        break;
                    }
                }
            }
        }
    }
}
