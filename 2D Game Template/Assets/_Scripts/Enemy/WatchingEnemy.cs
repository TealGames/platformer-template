using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/*
 * This enemy will have a rotating line of sight raycast that can be used for enemy types like lasers/seaking mechanics to avoid enemies, etc.
 */

public class WatchingEnemy : MonoBehaviour
    // If there is weird jittery movement, fix what methods/code is in Update and what code is in FixedUpdate() so that Physics are in FixedUpdate()
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float visionDistance;
    [SerializeField]
    [Tooltip("Check this box if you want to input a LineRenderer Component (create empty game Object in hierarchy and add Line Renderer component) to show the raycast lines in Game View")]
    private bool showLines;

    [SerializeField] private LineRenderer lineOfSight;

    // Start is called before the first frame update
    void Start()
    {
        if (showLines)
        {
            lineOfSight.SetPosition(0, transform.position);
        }

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, transform.right, visionDistance);
        if (hitInfo.collider != null)
        {

            UnityEngine.Debug.DrawLine(transform.position, hitInfo.point, Color.red);
            if (showLines)
            {
                lineOfSight.SetPosition(1, hitInfo.point);
                lineOfSight.startColor = Color.red;
                lineOfSight.endColor = Color.red;
            }

            if (hitInfo.collider.tag == "player")
            {
                // code here to do what you want with the player when they enter line of sight
            }
        }

        else
        {
            UnityEngine.Debug.DrawLine(transform.position, transform.position + transform.right * visionDistance, Color.green);
            if (showLines)
            {
                lineOfSight.SetPosition(1, transform.position + transform.right * visionDistance);
                lineOfSight.startColor = Color.green;
                lineOfSight.endColor = Color.green;

            }

        }
    }
}