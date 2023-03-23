using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor.Animations;

/// <summary>
/// Switches Cinemachine virtual cameras using animation states and functions and contians general methods for cinemachine cameras
/// </summary>

public class CinemachineVCamStates : MonoBehaviour
{
    private Animator animator;
    //[SerializeField] private AnimatorController controller;

    [SerializeField]
    [Tooltip("Note: the order of the animator states in the array must correspond with the vcams.")]
    private CinemachineVirtualCamera[] vcams;

    [SerializeField]
    [Tooltip("Note: the order of the vcams in the array must correspond with the names of their animator state.")]
    private string[] AnimatorStates;


    //public static CinemachineVCamStates Instance;

    // Start is called before the first frame update
    void Start()
    {
        /*
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        */
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SwitchState(int animatorStatesIndex)
    //this method should be called on events when the camera should switch
    {
        animator.Play(AnimatorStates[animatorStatesIndex]);
        vcams[animatorStatesIndex].Priority = 1;

        for (int i = 0; i < vcams.Length; i++)
        //this sets all other vcams in the array to have priorty of 0, except the vcam that you want to switch to
        {
            if (i == animatorStatesIndex) continue;
            else vcams[i].Priority = 0;
        }
    }

    public void SwitchState(CameraEffects.CameraType CameraType)
    {
        int animatorStatesIndex = -1;
        switch(CameraType)
        {
            case CameraEffects.CameraType.Default:
                animatorStatesIndex = (int)CameraEffects.CameraType.Default;
                break;
            case CameraEffects.CameraType.Stationary:
                animatorStatesIndex = (int)CameraEffects.CameraType.Stationary;
                break;
            default:
                break;
        }
        if (animatorStatesIndex != -1)
        {
            animator.Play(AnimatorStates[animatorStatesIndex]);
            vcams[animatorStatesIndex].Priority = 1;

            for (int i = 0; i < vcams.Length; i++)
            //this sets all other vcams in the array to have priorty of 0, except the vcam that you want to switch to
            {
                if (i == animatorStatesIndex) continue;
                else vcams[i].Priority = 0;
            }
        }
        else UnityEngine.Debug.LogError($"The argument of camera state (enum) {CameraType} does not have any animator state index correlated with it!");
    }

    public GameObject GetVCam(string name)
    {
        foreach (var vcam in vcams)
        {
            if (vcam.gameObject.name == name) return vcam.gameObject;
        }
        UnityEngine.Debug.LogError($"The argument {name} for a vcam name does not exist");
        return null;
    }

    public GameObject GetActiveVCam()
    {
        int maxPriority = 0;
        CinemachineVirtualCamera activeCamera = null;
        foreach (var vcam in vcams)
        {
            if (vcam.Priority> maxPriority)
            {
                maxPriority = vcam.Priority;
                activeCamera = vcam;
            }  
        }
        return activeCamera.gameObject;
    }

    /*
    private void AddAllStates()
    {
        AnimatorControllerLayer[] allLayers = controller.layers;
        List<string> stateNames = new List<string>();
        for (int i = 0; i < allLayers.Length; i++)
        {
            ChildAnimatorState[] states = allLayers[i].stateMachine.states;
            for (int j=0; j<states.Length; j++)
            {
                stateNames.Add(states[j].state.name);
            }
        }
    }
    */
}