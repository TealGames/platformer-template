using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
using System;
using Ink.UnityIntegration;

public class InkDialogueManager : MonoBehaviour
{
    public static InkDialogueManager Instance;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private GameObject continueIcon;

    

    [Header("Global Ink Variables")]
    [SerializeField] private InkFile globalsInkFile;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;
    [SerializeField] [Tooltip("The amount of time before another letter is displayed in the UI dialogue text box")] private float typingSpeed = 0.04f;
    [SerializeField] [Tooltip("If true, player can only continue dialogue (pressing submit button) if the typing effect (and all the dialogue) is completed")] private bool canOnlyContinueOnTypingEnd = true;
    [SerializeField] [Tooltip("If true, if the player presses the submit button while dialogue is still typing, instantly complete dialogue")] private bool completeDialogueOnSubmit = true;


    [Header("Options")]
    [SerializeField][Tooltip("If true, will disable the dialogue box and exit dialogue when another scene is loaded while the dialogue is still displayed")] private bool disableOnSceneLoad = true;

    private InkDialogueVariables dialogueVariables;

    private Story currentStory;
    private bool isPlayingDialogue;
    private bool submitPressed;

    private GameObject currentTalkingNPC;

    private bool canContinueToNextLine = false;

    private Coroutine displayLineCoroutine;

    public event Action OnNextLineOfDialogue;
    public event Action OnDialogueEnd;
    public event Action OnDialogueStart;
   
    //currently talking NPC gameobject and tag array are parameters
    public event Action<GameObject, string[]> OnTagsFound;
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        dialogueVariables = new InkDialogueVariables(globalsInkFile.filePath);
    }


    private void Start()
    {
        PlayerCharacter.Instance.OnSubmitButtonPressed += (bool isSubmitPressed) => submitPressed = isSubmitPressed;
        GameManager.Instance.OnSceneLoadStarted += () => StartCoroutine(ExitDialogue());

        isPlayingDialogue = false;
        dialoguePanel.SetActive(false);

        if (!canOnlyContinueOnTypingEnd) canContinueToNextLine = true;

        // get all of the choices text 
        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        // return right away if dialogue isn't playing
        if (!isPlayingDialogue) return;

        // handle continuing to the next line in the dialogue when submit is pressed
        if (currentStory.currentChoices.Count == 0 && submitPressed && canContinueToNextLine) ContinueStory();

        UnityEngine.Debug.Log(currentTalkingNPC);
    }

    public void StartDialogue(TextAsset inkJSON, string characterName, GameObject currentNPC)
    {
        OnDialogueStart?.Invoke();
        currentStory = new Story(inkJSON.text);
        isPlayingDialogue = true;
        dialoguePanel.SetActive(true);
        characterNameText.text = characterName;

        currentTalkingNPC = currentNPC.gameObject;
        dialogueVariables.StartListening(currentStory);

        ContinueStory();
    }

    private IEnumerator ExitDialogue()
    {
        //if a scene loads and a conversation has not been started, don't do anything but if a conversation
        // has started (so current talking npc is not null) but a scene switches, just exit dialogue
        if (currentTalkingNPC != null)
        {
            currentTalkingNPC.gameObject.GetComponentInChildren<InkDialogueTrigger>().DialogueEnd();

            yield return new WaitForSeconds(0.2f);

            dialogueVariables.StopListening(currentStory);

            isPlayingDialogue = false;
            dialoguePanel.SetActive(false);
            dialogueText.text = "";
            characterNameText.text = "";
            currentTalkingNPC = null;

            UnityEngine.Debug.Log("Talking ended");
            OnDialogueEnd?.Invoke();
        }
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            OnNextLineOfDialogue?.Invoke();

            if (displayLineCoroutine != null) StopCoroutine(displayLineCoroutine);
            displayLineCoroutine= StartCoroutine(DisplayLine(currentStory.Continue()));
            CheckIfTagsExist();
        }
        else StartCoroutine(ExitDialogue());
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        // defensive check to make sure our UI can support the number of choices coming in
        if (currentChoices.Count > choices.Length) Debug.LogError($"More choices were given than the UI can support. Number of choices given: {currentChoices.Count}");

        int index = 0;
        // enable and initialize the choices up to the amount of choices for this line of dialogue
        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }
        // go through the remaining choices the UI supports and make sure they're hidden
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private void HideChoices()
    {
        foreach (var choiceButton in choices)
        {
            choiceButton.gameObject.SetActive(false);
        }
    }

    private IEnumerator SelectFirstChoice()
    {
        // Event System requires we clear it first, then wait
        // for at least one frame before we set the current selected object.
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    private IEnumerator DisplayLine(string line)
    //adds the typing effect to the text
    {
        HideChoices();
        dialogueText.text = "";
        if (canOnlyContinueOnTypingEnd) canContinueToNextLine = false;
        continueIcon.SetActive(false);

        bool isAddingRichTextTags = false;

        //to prevent submit button press to be counted before next dialogue begins
        if (submitPressed) submitPressed = false;


        foreach (char letter in line.ToCharArray())
        {
            if (submitPressed && completeDialogueOnSubmit)
            {
                submitPressed = false;
                dialogueText.text = line;
                break;
            }

            //TMPro's rich text tags modify text, but we do not want to show the tag symbols < > 
            if (letter == '<' || isAddingRichTextTags)
            {
                isAddingRichTextTags= true;
                dialogueText.text += letter;
                if (letter== '>') isAddingRichTextTags = false;
            }
            else
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            
        }

        DisplayChoices();
        if (canOnlyContinueOnTypingEnd) canContinueToNextLine = true;
        continueIcon.SetActive(true);
    }

    public void MakeChoice(int choiceIndex)
    {
        if (canContinueToNextLine)
        {
            currentStory.ChooseChoiceIndex(choiceIndex);
            submitPressed = false;
            ContinueStory();
        }
    }

    private void CheckIfTagsExist()
    {
        if (currentStory.currentTags.Count ==0) return;
        else
        {
            string[] tags = new string[currentStory.currentTags.Count];
            int index = 0;
            foreach (var tag in currentStory.currentTags)
            {
                tags[index] = tag;
                index++;
                UnityEngine.Debug.Log($"found these tags: {tag}");
            }
            OnTagsFound?.Invoke(currentTalkingNPC.gameObject,tags);
            
        }
        
    }

    public Ink.Runtime.Object GetVariableState(string variableName)
        // in order to get the value use "((Ink.Runtime.StringValue)DialogueManager.Instance.GetVariableState([VARIABLE NAME])).value"
        //note: change "StringValue" to "BoolValue", "IntValue" or "FloatValue" when that is the type of variable that is being stored in Ink
    {
        Ink.Runtime.Object variableValue = null;
        dialogueVariables.inkVariables.TryGetValue(variableName, out variableValue);
        if (variableValue != null) UnityEngine.Debug.LogWarning($"Ink variable with the name {variableName} was found to be null!");
        return variableValue;
    }

}
