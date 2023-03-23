using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMove : MonoBehaviour
{
    [SerializeField] private bool constantlyMove;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;

    [SerializeField] private bool moveLeft;
    [SerializeField] private bool moveRight;

    [SerializeField] private Transform leftBarrier;
    [SerializeField] private Transform rightBarrier;
    private float moveSpeed;
    private float startPositionX;

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        startPositionX = transform.position.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (constantlyMove)
        {
            if (moveLeft)
            {
                transform.Translate(Vector2.left * moveSpeed * Time.fixedDeltaTime);
                if (transform.position.x < leftBarrier.position.x) transform.position = new Vector2(rightBarrier.position.x, transform.position.y);
            }
            if (moveRight)
            {
                transform.Translate(Vector2.right * moveSpeed * Time.fixedDeltaTime);
                if (transform.position.x > rightBarrier.position.x) transform.position = new Vector2(leftBarrier.position.x, transform.position.y);
            }


        }

    }
}