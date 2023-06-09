using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Reflection;
using System;

/// <summary>
/// A trigger with a specified area that invokes events when the player enters, exits, or stays and other settings
/// </summary>

public class AreaTrigger : Trigger
{
    [Header("Interactable")]
    [SerializeField] private bool isInteractableTrigger;
    [SerializeField] [Tooltip("Amount of time when the player can press the button and interact with the same item")] private float interactCooldown;
    [SerializeField] [Tooltip("Show a HUD message displaying that the player can press the choosen button to interact!")] private bool showInteractMessage;
    [SerializeField] private string interactMessage = "";
    [SerializeField] private HUD.TextType interactMessageTextSize;
    public UnityEvent OnInteract;
    private bool canInteract = true;

    [Header("Events")]
    public UnityEvent onEnter;

    public UnityEvent onStay;

    public UnityEvent onExit;

    public UnityEvent OnEntryExitButtonPressed;

    //public UnityEventThree[] NewUnitEvents;

    



    // Start is called before the first frame update
    private void Start()
    {
        PlayerCharacter.Instance.OnInteractableButtonPressed += InteractButtonPressed;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnEnter(collider);
        if (collider != null)
        {

            if (collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
            {
                onEnter.Invoke();
                inTrigger = true;

                if (showInteractMessage && interactMessage != null && interactMessageTextSize != HUD.TextType.None) HUD.Instance.EnableText(interactMessageTextSize, interactMessage);
                else if ((showInteractMessage && interactMessage == "") || (!showInteractMessage && interactMessage != ""))
                    UnityEngine.Debug.LogError($"Either {gameObject.name}'s interact button message is empty but is selected as an interact trigger or the message is filled but the interact trigger bool is false!");
                else if (showInteractMessage && interactMessage !=null && interactMessageTextSize == HUD.TextType.None)
                    UnityEngine.Debug.LogError($"{gameObject.name}'s interact button message is filled and is an interact trigger but the text type is selected as NONE!");
                if (disableOnEnter) gameObject.SetActive(false);
                
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider != null)
        {
            if (collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript)) onStay.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        base.OnExit(collider);
        if (collider != null)
        {
            if (collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
            {
                onExit.Invoke();
                inTrigger = false;
                if (showInteractMessage && interactMessage != null) HUD.Instance.DisableText(interactMessageTextSize);

                if (disableOnExit) gameObject.SetActive(false);
            }
        }
    }

    private void InteractButtonPressed()
    {
        if (inTrigger && canInteract)
        {
            if (interactCooldown>0.0f)
            {
                canInteract = false;
                StartCoroutine(InteractCooldown());
            }
            OnInteract?.Invoke();
        }
    }

    public void EnableReminderText(string text)=> DialogueManager.Instance.ShowReminderText(text);

    public void DisableReminderText()=> DialogueManager.Instance.DisableReminderText();

    private IEnumerator InteractCooldown()
    {
        yield return new WaitForSeconds(interactCooldown);
    }

    /*
    public void CallSingletonMethod(GameManager.Singletons singleton, string methodName, object[] methodArguments)
    {
        MethodInfo method = null;
        switch (singleton)
        {
            //gettings class type based on enum
            case GameManager.Singletons.PlayerCharacter:
                Type playerType = Type.GetType("PlayerCharacter"); 
                method = playerType.GetMethod(methodName);
                break;

            case GameManager.Singletons.AudioManager:
                Type audioManagerType = Type.GetType("AudioManager");
                method = audioManagerType.GetMethod(methodName);
                break;

            case GameManager.Singletons.InkDialogueManager:
                Type dialogueManagerType = Type.GetType("InkDialogueManager");
                method = dialogueManagerType.GetMethod(methodName);
                break;

            case GameManager.Singletons.GameManager:
                Type gameManagerType = Type.GetType("GameManager");
                method= gameManagerType.GetMethod(methodName);
                break;

            case GameManager.Singletons.HUD:
                Type HUDType = Type.GetType("HUD");
                method= HUDType.GetMethod(methodName);
                break;

            default:
                UnityEngine.Debug.LogError($"The singleton type (enum) argument {singleton} is not linked with any code in CallSingletonMethod()!");
                break;
        }

        if (method!= null && methodArguments!=null) method.Invoke(null, methodArguments);

    }
    */
}



