
//Code from Trever Mock's video: https://www.youtube.com/watch?v=Kokt0c8sbNc&t=883s

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("First Selected Button")]
    [Tooltip("When navigating the script that inherits this script's UI, this will be the first button gameObject selected")][SerializeField] private Button firstSelected;

    protected virtual void OnEnable()
    {
        SetFirstSelected(firstSelected);
    }

    public void SetFirstSelected(Button firstSelectedButon) => firstSelectedButon.Select();
}
