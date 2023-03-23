using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script should be placed on each individual parallax layer and should have its own parallax factor to make it have a different distance from the camera.
 * It is best if each parallax layer has its own z-distance and sorting layer.
 */

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("-3 to -0.1 for foreground; 0.0 for camera speed; 0.1 to 3 for background. UPDATE METHOD and BLEND UPDATE METHOD of CINEMACHINE BRAIN OF MAIN CAMERA MUST BE SET TO FIXED UPDATE FOR IT TO WORK!")]
    [Range(-3.0f, 3.0f)] public float parallaxFactor;
    public void Move(float delta)
    {
        Vector3 newPos = transform.localPosition;
        newPos.x -= delta * parallaxFactor;
        transform.localPosition = newPos;
    }
}