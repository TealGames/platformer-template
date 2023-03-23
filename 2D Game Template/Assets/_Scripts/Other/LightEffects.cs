using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Contains functions related to changing effects and lighting
/// </summary>


public class LightEffects : MonoBehaviour
{
    [SerializeField][Range(0,2)] private float defaultIntensity;
    private Light2D globalLight;

    // Start is called before the first frame update
    void Start()
    {
        globalLight = gameObject.GetComponentInChildren<Light2D>();
        globalLight.intensity = defaultIntensity;   
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeIntensity(float intensity)
    {
        globalLight.intensity = intensity;
    }

    public void SetDefaultIntensity()
    {
        globalLight.intensity= defaultIntensity;
    }
}
