using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSway : MonoBehaviour
{
    [SerializeField] private float minSwaySpeed;
    [SerializeField] private float maxSwaySpeed;
    private float swaySpeed;

    [SerializeField] private float minRotateAngle;
    [SerializeField] private float maxRotateAngle;
    private float rotateAngle;

    private int swayDirection;

    [SerializeField] private float minSwayHoldDuration;
    [SerializeField] private float maxSwayHoldDuration;

    [SerializeField][Tooltip("The time for it to complete one full rotation from negative rotation back to neutral/starting position")] private float minAnimationDuration;
    [SerializeField][Tooltip("The time for it to complete one full rotation from negative rotation back to neutral/starting position")] private float maxAnimationDuration;
    private float animationDuration;
    private float timeValue;

    // Start is called before the first frame update
    void Start()
    {
        swaySpeed = UnityEngine.Random.Range(minSwaySpeed, maxSwaySpeed);
        animationDuration = UnityEngine.Random.Range(minAnimationDuration, maxAnimationDuration);


        int randomSway = UnityEngine.Random.Range(0, 2);
        switch (randomSway)
        {
            case 0:
                swayDirection = 1;
                //(0,0,1)
                break;
            case 1:
                swayDirection = -1;
                //(0,0,-1)
                break;
        }

        rotateAngle = UnityEngine.Random.Range(minRotateAngle, maxRotateAngle);
        timeValue = 0.0f;
        transform.Rotate(0f, 0f, swayDirection * 0.5f);
        UnityEngine.Debug.Log($"The start rotation value is {swayDirection * 0.5f}");
        //UnityEngine.Debug.Break();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0f, 0f, rotateAngle * swayDirection * swaySpeed * timeValue);

        UnityEngine.Debug.Log($"The angle check is {transform.rotation.eulerAngles.z} ");
        if (Mathf.Approximately(-rotateAngle, transform.rotation.eulerAngles.z))
        //if all the way on the right, move back to central position
        {
            swayDirection = 1;
            timeValue = 0.0f;
            UnityEngine.Debug.Log("Rotation checkpoint reached!");
        }

        else if (Mathf.Approximately(rotateAngle, transform.rotation.eulerAngles.z))
        //if all the way on the left move back to central postion
        {
            swayDirection = -1;
            timeValue = 0.0f;
            UnityEngine.Debug.Log("Rotation checkpoint reached!");
        }
        else if (Mathf.Approximately(0.0f, transform.rotation.eulerAngles.z) && swayDirection == -1)
        //if back from right side to neutral
        {
            swayDirection = 1;
            timeValue = 0.0f;
            UnityEngine.Debug.Log("Rotation checkpoint reached!");
        }

        else if (Mathf.Approximately(0.0f, transform.rotation.eulerAngles.z) && swayDirection == 1)
        {
            swayDirection = -1;
            timeValue = 0.0f;
            UnityEngine.Debug.Log("Rotation checkpoint reached!");
        }

        else
        {
            //UnityEngine.Debug.Log($"Rotate angle (CONST) is {rotateAngle} sway dir {swayDirection} sway speed (CONST) {swaySpeed} and time value {timeValue}");
            timeValue = timeValue + Time.fixedDeltaTime;
        }

    }
}