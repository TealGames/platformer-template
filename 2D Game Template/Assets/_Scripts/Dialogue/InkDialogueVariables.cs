using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using System.IO;

/// <summary>
/// Manages and stores the state of Ink-created variables and allows it to persist and be used in other Ink stories and c# scripts
/// </summary>
/// 
public class InkDialogueVariables
{
    public Dictionary<string, Ink.Runtime.Object> inkVariables { get; set; }

    //since global ink files (ones that use 'INCLUDE [ink file name]') don't compile to a JSON file, we create constructor to create a new instance that adds each variable
    //after ink file compiles in that file path to a dictionary so c# classes can use those variables
    public InkDialogueVariables(string globalsFilePath)
    {
        string inkFileContents = File.ReadAllText(globalsFilePath);
        Ink.Compiler compiler = new Ink.Compiler(globalsFilePath);
        Story globalVariablesStory = compiler.Compile();

        inkVariables = new Dictionary<string, Ink.Runtime.Object>();
        foreach (string name in globalVariablesStory.variablesState)
        {
            Ink.Runtime.Object value = globalVariablesStory.variablesState.GetVariableWithName(name);
            inkVariables.Add(name, value);
        }
    }

    public void StartListening(Story story)
    {
        VariablesToStory(story);
        story.variablesState.variableChangedEvent += VariableChanged;
    }

    public void StopListening(Story story)
    {
        story.variablesState.variableChangedEvent -= VariableChanged;
    }

    private void VariableChanged(string name, Ink.Runtime.Object value)
    {
        if (inkVariables.ContainsKey(name))
        {
            inkVariables.Remove(name);
            inkVariables.Add(name, value);
        }
    }

    private void VariablesToStory(Story story)
    {
        foreach (KeyValuePair<string, Ink.Runtime.Object> variable in inkVariables)
        {
            story.variablesState.SetGlobal(variable.Key, variable.Value);
        }
    }

}
