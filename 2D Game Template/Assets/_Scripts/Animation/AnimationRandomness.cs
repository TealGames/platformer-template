using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates randomness in animations using animation parameters and setting different fields in the animation tab correspond to those parameters
/// </summary>

public class AnimationRandomness : MonoBehaviour
{
    [SerializeField][Tooltip("Make a float animation parameter titled animationSpeed_f as a parameter in the animation clip for Multiplier")] float minAnimationSpeed;
    [SerializeField][Tooltip("Make a float animation parameter titled animationSpeed_f as a parameter in the animation clip for Multiplier")] float maxAnimationSpeed;


    [SerializeField][Tooltip("Make a bool animation parameter titled animationMirror_b as a parameter in the animation clip for Mirror")] private bool doRandomMirroredAnimation;


    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        TriggerAnimation();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        TriggerAnimation();
    }

    public void TriggerAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("animationSpeed_f", UnityEngine.Random.Range(minAnimationSpeed, maxAnimationSpeed));
            if (doRandomMirroredAnimation)
            {
                int doMirror = UnityEngine.Random.Range(0, 2);
                if (doMirror == 1) animator.SetBool("animationMirror_b", true);
            }
        }
    }
}