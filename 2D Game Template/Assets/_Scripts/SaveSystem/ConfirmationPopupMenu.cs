
//Code from Trever Mock's video: https://www.youtube.com/watch?v=yTWPcimAdvY&list=PL3viUl9h9k7-tMGkSApPdu4hlUBagKial&index=4

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ConfirmationPopupMenu : Menu
{
    [Header("Components")]

    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;


    //UnityAction is similar to C# Action predefined delegate event
    public void ActivateMenu(string displayText, UnityAction confirmAction, UnityAction cancelAction)
    {
        gameObject.SetActive(true);
        this.displayText.text = displayText;

        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        confirmButton.onClick.AddListener(() =>
        {
            DeactivateMenu();
            confirmAction();
        });

        cancelButton.onClick.AddListener(() =>
        {
            DeactivateMenu();
            cancelAction();
        });
    }

    private void DeactivateMenu()=> gameObject.SetActive(false);

}
