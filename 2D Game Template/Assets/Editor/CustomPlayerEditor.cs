using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerCharacter))]
public class CustomPlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PlayerCharacter playerScript = (PlayerCharacter)target;
        if (GUILayout.Button("Freeze Player"))
        {
            playerScript.FreezePlayer(true);
        }

        if (GUILayout.Button("Unfreeze Player"))
        {
            playerScript.FreezePlayer(false);
        }

    }

}
