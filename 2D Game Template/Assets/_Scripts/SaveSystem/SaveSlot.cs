
//Code from Trever Mock's video: https://www.youtube.com/watch?v=Kokt0c8sbNc&t=883s

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private string profileId = "";

    [Header("Content")]
    [SerializeField] private GameObject noDataContent;
    [SerializeField] private GameObject hasDataContent;

    private Button saveSlotButton;

    [Tooltip("The completion precentage text on a save slot")][SerializeField] private TextMeshProUGUI completionPrecentageText;

    [Header("Clear Data")]
    [SerializeField] private Button clearButton;

    public bool hasData { get; private set; } = false;

    private void Awake() => saveSlotButton = gameObject.GetComponent<Button>();

    public void SetData(GameData data)
    {
        if (data == null)
        {
            hasData = false;
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
            clearButton.gameObject.SetActive(false);
        }
        else
        {
            hasData = true;
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);
            clearButton.gameObject.SetActive(true);

            completionPrecentageText.text = $"{data.GetPrecentageComplete().ToString()}% COMPLETE";
        }
    }

    public string GetProfileId() => this.profileId;

    public void SetInteractable(bool isInteractable)
    {
        saveSlotButton.interactable = isInteractable;
        clearButton.interactable = isInteractable;
    }
        
        
        
}
