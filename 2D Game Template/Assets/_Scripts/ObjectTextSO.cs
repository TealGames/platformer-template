using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectTextSO", menuName = "ScriptableObjects/Object Text")]
public class ObjectTextSO : ScriptableObject
{
    public string objectName;
    [TextArea(3,5)]public string objectDescription;
    [TextArea(3,10)] public string objectLore;

}
