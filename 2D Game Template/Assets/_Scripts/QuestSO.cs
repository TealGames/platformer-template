using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName= "QuestSO", menuName= "ScriptableObjects/Quest")]
public class QuestSO : ScriptableObject
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


    [HideInInspector] public bool questCompleted = false;
    [HideInInspector] public bool questStarted = false;
    //add options for multiple quests to be active at the same time, or only one at a time
}
