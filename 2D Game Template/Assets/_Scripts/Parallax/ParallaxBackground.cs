using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script should be placed on the parent object that contains all the parallax layers.
 */

[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    public ParallaxCamera parallaxCamera;
    List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();

    void Start()
    {
        if (parallaxCamera == null)
            UnityEngine.Debug.LogWarning($"{gameObject.name}'s parallaxCamera field is null! It must be assigned in order for the parallax to work correctly!");
        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate += Move;


        GameManager.Instance.OnSceneLoadEnded += () =>
        {
            parallaxCamera = CameraSingleton.Instance.GetComponentInChildren<ParallaxCamera>();
            if (parallaxCamera != null)
            {
                parallaxCamera.onCameraTranslate += Move;
                if (this!=null)SetLayers();
            }
            else UnityEngine.Debug.LogWarning($"{gameObject.name} could not find the Parllax Camera script!");
            
        };
    }

    void SetLayers()
    {
        parallaxLayers.Clear();
        UnityEngine.Debug.Log($"Accessing {gameObject.name}'s children!");
        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();

            if (layer != null)
            {
                layer.name = "Layer-" + i;
                parallaxLayers.Add(layer);
            }
        }
    }

    void Move(float delta)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.Move(delta);
        }
    }
}
