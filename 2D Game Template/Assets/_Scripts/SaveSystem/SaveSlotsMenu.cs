
//Code from Trever Mock's video: https://www.youtube.com/watch?v=Kokt0c8sbNc&t=883s

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotsMenu : Menu
{
    [Header("Menu Buttons")]
    [Tooltip("Button is save slot menu that returns user back to the main menu")][SerializeField] private Button backButton;

    //note: saving does not equal loading- new game saves data to a new file, but load game allows user to return to other save files
    private bool isLoadingGame = false;

    private SaveSlot[] saveSlots;

    [Header("Confirmation Popup")]
    [SerializeField] private ConfirmationPopupMenu confirmationPopupMenu;

    [Tooltip("The text that appears on the confirmation popup menu when you want to start a new game with a file that already has data")]
    [SerializeField] [TextArea(3,5)] private string overrideSaveFileWarning;

    [Tooltip("The text that appears on the confirmation popup menu when you click on the clear button for a save file")]
    [SerializeField][TextArea(3, 5)] private string clearSaveFileWarning;

    private void Awake()
    {
        saveSlots = gameObject.GetComponentsInChildren<SaveSlot>();
       
    }

    public void ActivateMenu(bool isLoadingGame)
    {

        this.isLoadingGame = isLoadingGame;
        gameObject.SetActive(true);

        //load all the profiles that exist
        Dictionary<string, GameData> profilesGameData= DataPersistenceManager.Instance.GetAllProfilesGameData();

        //ensure back button is enabled when we activate the menu
        backButton.interactable = true;

        GameObject firstSelected= backButton.gameObject;

        foreach (SaveSlot saveSlot in saveSlots)
        {
            GameData profileData = null;
            profilesGameData.TryGetValue(saveSlot.GetProfileId(), out profileData);
            saveSlot.SetData(profileData);

            if (profileData == null && isLoadingGame) saveSlot.SetInteractable(false);
            else
            {
                saveSlot.SetInteractable(true);
                if (firstSelected.Equals(backButton.gameObject)) firstSelected = saveSlot.gameObject;
            }
        }

        //set the first selected button
        Button firstSelectedButton = firstSelected.GetComponent<Button>();
        SetFirstSelected(firstSelectedButton);
    }

    private void SaveGameAndLoadScene()
    {
        DataPersistenceManager.Instance.SaveGame();

        //load the scene which will also save the game because of OnSceneUnloaded() in DataPersistenceManager
        GameManager.Instance.LoadFirstScene();
    }

    public void OnClearClicked(SaveSlot saveSlot)
    {
        DisableMenuButtons();
        confirmationPopupMenu.ActivateMenu(
            clearSaveFileWarning,

            //function to execute if we select "yes"
            () =>
            {
                DataPersistenceManager.Instance.DeleteProfileData(saveSlot.GetProfileId());
                ActivateMenu(isLoadingGame);
            },

            //function to execute if we select "cancel"
            ()=>
            {
                ActivateMenu(isLoadingGame);
            }
        );
    }

    public void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        //prevents buttons from getting double clicked when clicked once
        DisableMenuButtons();
        gameObject.SetActive(false);

        //case- loading game
        if (isLoadingGame)
        {
            DataPersistenceManager.Instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
            SaveGameAndLoadScene();
        }

        //case- new game, but the save slot has data
        else if (saveSlot.hasData)
        {
            confirmationPopupMenu.ActivateMenu(
                overrideSaveFileWarning,

                //function to execute if we select "yes"
                () =>
                {
                    DataPersistenceManager.Instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
                    DataPersistenceManager.Instance.NewGame();
                    SaveGameAndLoadScene();
                },

                //function to execute if we select "cancel"
                () =>
                {
                    ActivateMenu(isLoadingGame);

                }  
            );
        }

        //case- new game and save slot has no data
        else
        {
            DataPersistenceManager.Instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
            DataPersistenceManager.Instance.NewGame();
            SaveGameAndLoadScene();
        }


    }

    public void DisableMenuButtons()
    {
        foreach (SaveSlot saveSlot in saveSlots) saveSlot.SetInteractable(false);
        backButton.interactable = false;
    }

}
