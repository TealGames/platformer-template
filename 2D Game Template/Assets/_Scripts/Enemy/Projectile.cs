using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script should be added to objects that have a projectile-like behavior and travel through the air to a certain target. Add the DamagingObject.cs script to this to make it
/// do damage. This can be placed in the projectile prefab out of the shooting enemy to be shot out of enemy
/// </summary>

public class Projectile : MonoBehaviour
{
    public enum TargetType
    {
        None,
        Player,
        SetTransform,
    }
    [Tooltip("The type of target or position that this projectile should reach. If a set transform, will end its journey on that position. " +
        "If its a player, will continue moving until a destroy or collision condition is met")][SerializeField] private TargetType targetType;

    [Tooltip("If the target type is set (not moving) then this should be that target's transform")][SerializeField] private Transform setTargetTransform;

    public enum PathType
    {
        Direct,
        Parabolic,
    }
    [Tooltip("The type of path to take to the target")][SerializeField] private PathType pathType;


    [Tooltip("The additional x or y values added to the target's transform (this can be useful if the target player's " +
        "transform is not where it wants to be hit or the set transform may not be the best location")]
    [SerializeField] private Vector2 targetOffset = new Vector2(0f, 2.5f);
    private Vector3 targetPosition;
    private float distanceToTarget;

    private bool playerHit = false;
    private bool continuePath = true;

    private float distanceToGround;
    private Vector2 groundLocation;

    [SerializeField] private float speed;
    [SerializeField] private bool isFacingRight;

    [Header("Collisions")]
    [SerializeField] private Collider2D collider;
    [Tooltip("When hitting these layers, then the projectile will become a non-trigger collider so it can do physical collisions. " +
        "However, GROUND and PLAYER (if target type is player) layers are already included, so these are additional layers. " +
        "If these are the collisions covered, do not add extra layers here until you need them. NOTE: will apply the missed destroy delay once hit")]
    [SerializeField] private LayerMask additionalCollidableLayers;
    private bool hitSpecifiedLayer = false;
    [Tooltip("The delay that the projectile gets destroyed after hitting the target type")][SerializeField] private float hitDestroyDelay;
    [Tooltip("The delay that the projectile gets destroyed after missing the target type and has made contact with another object")][SerializeField] private float missedDestroyDelay;

    [Header("Raycasts")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("The length of the ray that detects the distance to the ground")][SerializeField] private float groundRaycastLength;
    [Tooltip("The additional x and y values added to the ground location. Greater the values, the farther away from ground impact point will the projectile stop. " +
        "0.5 for both is a great value for more accurate impact")]
    [SerializeField] private Vector2 groundImpactOffset = new Vector2(0.5f, 0.5f);

    [Header("Extra Parabolic Path Fields")]
    [Tooltip("The maximum point on the downward facing parabola")][SerializeField] private float maxArcHeight;
    [Tooltip("If true, the arc height will be calculated based the distance from the player. Var maxArcHeigh will be used as a baseline and values will be added based on distance. " +
        "Note: if the target is not a player, this will do nothing since you can then just change max arc height directly if a there is a set path")][SerializeField] private bool doArcHeightFromDistance;
    //[Tooltip("When calculating based on player's distance, will multiply the maxArcHeight by this multplier for changes in vertical distance to target")][SerializeField] private float verticalArcMultiplier;
    [Tooltip("When calculating based on player's distance, will multiply the maxArcHeight by this multplier (after other formulaic values are applied) for changes in horizontal distance to target")]
    [Range(0.5f, 10f)][SerializeField] private float horizontalArcMultiplier = 1f;
    private Vector2 startPosition;

    private const int nothingLayer = 0;


    private void Start()
    {
        UnityEngine.Debug.Log($"Start is called for {gameObject.name}");

        //based on path type, set the target position
        if (pathType == PathType.Parabolic) startPosition = transform.position;
        switch (targetType)
        {
            case TargetType.Player:
                targetPosition = new Vector3(PlayerCharacter.Instance.transform.position.x + targetOffset.x,
                    PlayerCharacter.Instance.transform.position.y + targetOffset.y, PlayerCharacter.Instance.transform.position.z);
                //if the target is a player, on hit, immediately destroy the projectile based on event from damaging object (if exists)
                if (gameObject.TryGetComponent<DamagingObject>(out DamagingObject damagingObjectScript)) damagingObjectScript.OnPlayerCollision += () =>
                {
                    UnityEngine.Debug.Log("Proj hit player");
                    playerHit = true;
                    StartCoroutine(DestroyDelay(hitDestroyDelay));
                };

                break;
            case TargetType.SetTransform:
                targetPosition = new Vector3(setTargetTransform.position.x + targetOffset.x,
                    setTargetTransform.position.y + setTargetTransform.position.y, setTargetTransform.position.z);
                break;
            default:
                UnityEngine.Debug.LogWarning("The target type of {gameObject.name} does not have a target position assignment!");
                break;
        }

        //turn the projectile from start if not facing the right direction
        distanceToTarget = targetPosition.x - transform.position.x;
        if ((distanceToTarget < 0 && isFacingRight) || (distanceToTarget > 0 && !isFacingRight))
        {
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
            isFacingRight=!isFacingRight;
        }
    }

    private void Update()
    {
        //UnityEngine.Debug.Log($"Is facing right is: {isFacingRight} and local scale is {transform.localScale}");
    }

        // Update is called once per frame
    private void FixedUpdate()
    {
        DestroyCheck();
        HitCheck();
        
        if (pathType == PathType.Direct)
        {
            //if the target is hit, based on path type, do actions, otherwise continuing moving
            if ((gameObject.transform.position == targetPosition))
            {
                if (targetType == TargetType.Player && !playerHit) MoveStraight();
                else if (targetType == TargetType.Player && playerHit || targetType==TargetType.SetTransform) StartCoroutine(DestroyDelay(hitDestroyDelay));
            }
            else MoveStraight();  
        }

        else if (pathType == PathType.Parabolic)
        {
            float destinationRotation;
            float destinationDistanceToGround;

            //compute the next position for the projectile to go to (with arc added)
            float startXPosition = startPosition.x;
            float endXPosition = targetPosition.x;
            float startEndDistance = endXPosition - startXPosition;
            float nextX = Mathf.MoveTowards(transform.position.x, endXPosition, speed * Time.deltaTime);
            float baseY = Mathf.Lerp(startPosition.y, targetPosition.y, (nextX - startXPosition) / startEndDistance);
            float baseArc = maxArcHeight;
            
            //if track players distance for height and trget is player, set base arc to accurately hit player with arc
            if (doArcHeightFromDistance && distanceToTarget!= 0f && targetType== TargetType.Player)
            {
                float verticalDistance= Mathf.Abs(PlayerCharacter.Instance.transform.position.y - transform.position.y);
                float horizontalDistance= Mathf.Abs(PlayerCharacter.Instance.transform.position.x - transform.position.x);
                if (maxArcHeight + 2f <= horizontalDistance && verticalDistance <= maxArcHeight) baseArc = (horizontalDistance - 1f) * horizontalArcMultiplier;
                //else if (verticalDistance> maxArcHeight+baseY) baseArc = (verticalDistance - baseY) * verticalArcMultiplier;
            }
            float arc = baseArc * (nextX - startXPosition) * (nextX - endXPosition) / (-0.25f * Mathf.Pow(startEndDistance, 2));
            
            Vector3 nextPosition = new Vector3(nextX, baseY + arc, transform.position.z);

            //if we hit the target, start the hit destroy delay, if not and is a player target type,
            //cache some vars for determining trajectory of continued path
            if (transform.position == targetPosition)
            {
                if (targetType == TargetType.Player && !playerHit) destinationRotation = transform.rotation.z;
                else if (targetType == TargetType.Player && playerHit || targetType == TargetType.SetTransform) StartCoroutine(DestroyDelay(hitDestroyDelay));
            }

            //if past the position after missing player, then begin linear downward descent and set position and rotation
            else if (targetType == TargetType.Player && continuePath && (transform.position.x- endXPosition <= 0.5f && !isFacingRight) || (transform.position.x-endXPosition >= -0.5f && isFacingRight))
            {
                //UnityEngine.Debug.Log($"Angles in deg are {transform.eulerAngles}");
                float currentY = transform.position.y;
                float missedNextY= currentY - (speed * Time.deltaTime);
                Vector3 missedNextPosition = new Vector3(transform.position.x, missedNextY , transform.position.z);
                if (distanceToTarget < 0) transform.rotation = HelperFunctions.LookAt2D(transform.position- missedNextPosition);
                else if (distanceToTarget > 0) transform.rotation = HelperFunctions.LookAt2D(missedNextPosition - transform.position);
                transform.position= missedNextPosition;
            }

            //otherwise rotate projectile to the next parabolic position and go there. Based on the target, rotate by the correct amount
            else if (continuePath)
            {
                if (distanceToTarget < 0) transform.rotation = HelperFunctions.LookAt2D(transform.position - nextPosition);
                else if (distanceToTarget > 0) transform.rotation = HelperFunctions.LookAt2D(nextPosition - transform.position);
                transform.position = nextPosition;
            }
        }
    }

    //direct path's position calculation
    private void MoveStraight()
    {
        Vector3 direction = new Vector3(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y, 0).normalized;
        transform.position += speed * direction * Time.deltaTime;
    }

    private void Turn()
    {
        UnityEngine.Debug.Log("Proj being turned");
        //stores scale and flips the player along the x axis, 
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        isFacingRight = !isFacingRight;
    }

    private void TurnCheck()
    {
        if ((distanceToTarget < 0 && isFacingRight) || (distanceToTarget > 0 && !isFacingRight)) Turn();
    }

    // a delay added to the destruction
    private IEnumerator DestroyDelay(float delay)
    {
        continuePath = false;
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    //Checks if the projectile meets requirements to get destroyed, regardless of projectile type
    private void DestroyCheck()
    {
        GameObject activeVCam = CameraSingleton.Instance.GetComponentInChildren<CinemachineVCamStates>().GetActiveVCam();
        float maxXCameraView = activeVCam.GetComponent<CameraEffects>().GetMaxXCameraView();
        float minXCameraView = activeVCam.GetComponent<CameraEffects>().GetMinXCameraView();
        //UnityEngine.Debug.Log($"Max camera view is {maxXCameraView} and {minXCameraView}");

        //if passes camera view immediately destroy it
        if (transform.position.x >= maxXCameraView || transform.position.x <= minXCameraView)
        {
            UnityEngine.Debug.Log("Proj being destroyed by leaving camera view");
            Destroy(gameObject);
        }
    }

    //checks if the projectile hits the ground (or additional layers to check) and applies destroy delays
    private void HitCheck()
    {
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down.normalized, groundRaycastLength, groundLayer);
        UnityEngine.Debug.DrawRay(transform.position, Vector2.down.normalized, Color.green, 10f);

        if (collider != null)
        {
            //if projectile touches another object of another layer specified in additional layers
            if (additionalCollidableLayers.value!= nothingLayer && collider.IsTouchingLayers(additionalCollidableLayers) && collider.isTrigger)
            {
                //UnityEngine.Debug.Log("Should be trigger now");
                collider.isTrigger = false;
                StartCoroutine(DestroyDelay(missedDestroyDelay));
            }
        }

        if (groundHit == null) UnityEngine.Debug.LogWarning($"The projectile's {gameObject.name} racyast length {groundRaycastLength} was not long enough to detect the ground!");
        else
        {
            distanceToGround = groundHit.distance;
            groundLocation = groundHit.point;
            UnityEngine.Debug.Log($"Raycast distance to ground: {distanceToGround} and ground location {groundLocation}");

            //if the ground location found by raycast is the same as curent position (plus optional offset), stop motion and destroy
            if (transform.position.x+ groundImpactOffset.x <= groundLocation.x && transform.position.y+ groundImpactOffset.y <= groundLocation.y)
            {
                if (gameObject.TryGetComponent<DamagingObject>(out DamagingObject damagingObjectScript)) damagingObjectScript.SetCollidable(false);
                UnityEngine.Debug.Log("Is touching ground layer!");
                collider.isTrigger = false;
                StartCoroutine(DestroyDelay(missedDestroyDelay));
                return;
            } 
        }
    }
}