
//Code from Trever Mock's video: https://www.youtube.com/watch?v=aUi9aijvpgs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make any script that has data that needs to be saved/loaded implement this interface
/// </summary>

public interface IDataPersistence
{
    void LoadData(GameData data);

    void SaveData(GameData data);
}
