using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This enemy patrols a certain area that can be specified with Transform points and other settings
/// </summary>

public class PatrolEnemy : Enemy
{
    [Header("Patrol Enemy")]
    [Tooltip("The speed at which the patrol enemy walks when patroling")][SerializeField] private float speed;
    [SerializeField] private Transform[] patrolPoints;
    [Tooltip("How long an enemy stays at the patrol points")][SerializeField] private float waitTime;

    [Tooltip("If true, no matter player's position in relation to patrol enemy, will not attack")][SerializeField] private bool ignorePlayer = false;
    private float distanceToPlayer;

    [SerializeField] private Transform attackCenterTransform;
    [SerializeField] private float attackRange;
    [Tooltip("The amount of time that the enemy waits before attacking again. OPTIMAL AMOUNT IS GREATER THAN 1 otherwise attack may be registered quickly after previous attack")][SerializeField] private float attackCooldown;
    [Tooltip("If true, after noticing player and attack the limit, it will return to patroling")][SerializeField] private bool doAttackLimit;
    [Tooltip("The maximum amount of times that the enemy can attack when it notices player in alert range")][SerializeField] private int attackLimit;
    private int currentAttackAmount;


    private int currentPointIndex = 0;

    public enum PatrolEnemyState
    {
        Patroling,
        Attacking,
        Waiting,
    }
    private PatrolEnemyState currentPatrolEnemyState;


    //to prevent the enemy from attacking too many times
    private bool hasAttacked = false;

    private bool animationCalled = false; // to prevent the walking animation from restarting 
    [Tooltip("Whether the enemy is facing right in default pose before entering play mode")] [SerializeField]private bool isFacingRight;

    [Header("Tools")]
    [SerializeField] private Color alertRangeColor;
    [SerializeField] private Color attackRangeColor;

    // Start is called before the first frame update
    private void Start()
    { 
        base.Start();
        animator?.SetFloat("speed_f",10f);
        currentPatrolEnemyState = PatrolEnemyState.Patroling;

        //if enemy is standing on the first patrol point on start, just skip to the next one
        if (patrolPoints.Length > 0 && transform.position.x == patrolPoints[currentPointIndex].position.x) currentPointIndex++;
        if (doAttackLimit && attackLimit > 0) currentAttackAmount = 0;
        TurnCheck();
    }

    // Update is called isWaiting per frame
    private void FixedUpdate()
    {
        UnityEngine.Debug.Log($"current position of the patrol is {transform.position} and curernt patrol point is {patrolPoints[currentPointIndex].position}");
        base.FixedUpdate();
        if (patrolPoints.Length>0 && currentPatrolEnemyState!= PatrolEnemyState.Attacking)
        {
            UnityEngine.Debug.Log("Patrol points is >0");
            if (transform.position.x != patrolPoints[currentPointIndex].position.x) MoveToPoint();

            else if (transform.position.x == patrolPoints[currentPointIndex].position.x)
            {
                UnityEngine.Debug.Log("Patrol is waiting");
                if (currentPatrolEnemyState!= PatrolEnemyState.Waiting)
                {
                    animator?.SetFloat("speed_f", 0f);

                    UnityEngine.Debug.Log("End point has been reached!");

                    StartCoroutine(Wait());
                }
                
            }
        }
    }

    void Update()
    {
        UnityEngine.Debug.Log($"has attack is {hasAttacked}");
        base.Update();
        if (target != null)
        {
            distanceToPlayer = target.position.x - transform.position.x;

            //if player is in the agro range and has reached distance to attack
            if (playerInAlertRange)
            {
                //if the enemy is facing away from the player, turn the enemy to face the player
                if ((distanceToPlayer < 0 && isFacingRight) || (distanceToPlayer > 0 && !isFacingRight)) Turn();

                if (!hasAttacked && !ignorePlayer)
                {
                    currentPatrolEnemyState = PatrolEnemyState.Attacking;

                    //if doing limit and do attack stuff (otherwise it will be ignored)
                    if (doAttackLimit && attackLimit>0 && currentPatrolEnemyState!= PatrolEnemyState.Patroling)
                    {
                        //if enemy has attack limit and the amount of attack is >= to limit, make enemy return to patrol 
                        //and stop attack so it can turn around
                        if (currentAttackAmount >= attackLimit)
                        {
                            if (distanceToPlayer <= baseEnemyData.alertRange && transform.position.x != patrolPoints[currentPointIndex].position.x) IncreaseCurrentPointIndex();
                            currentPatrolEnemyState = PatrolEnemyState.Patroling;
                            return;
                        }

                        //otherwise, increase attack amount and do attack
                        else currentAttackAmount++;
                    }
                    hasAttacked = true;
                    animator?.SetTrigger("attack_trig");
                    StartCoroutine(AttackCooldown());
                }
            }
            else
            {
                currentAttackAmount = 0;
                TurnCheck();
            }
        }
    }
    IEnumerator Wait()
    {
        currentPatrolEnemyState = PatrolEnemyState.Waiting;
        yield return new WaitForSeconds(waitTime);
        IncreaseCurrentPointIndex();
        TurnCheck();
        animator?.SetFloat("speed_f", 10f);
    }

    //increases current point index while also making it loop back
    //if reaches max length
    private void IncreaseCurrentPointIndex()
    {
        if (currentPointIndex + 1 < patrolPoints.Length) currentPointIndex++;
        else currentPointIndex = 0;
    }

    IEnumerator AttackCooldown()
    {
        //TurnCheck();
        yield return new WaitForSeconds(attackCooldown);
        hasAttacked = false;
        //TurnCheck();
    }

    private void TurnCheck()
    {
        if (patrolPoints.Length>0)
        {
            //if enemy is not facing the correct direction, turn it
            if ((patrolPoints[currentPointIndex].position.x > transform.position.x && !isFacingRight) ||
                              (patrolPoints[currentPointIndex].position.x < transform.position.x && isFacingRight)) Turn();
        }
    }

    private void Turn()
    {
        //stores scale and flips the player along the x axis, 
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        isFacingRight = !isFacingRight;
    }

    private void MoveToPoint()
    {
        currentPatrolEnemyState = PatrolEnemyState.Patroling;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(patrolPoints[currentPointIndex].position.x, transform.position.y), speed * Time.deltaTime);
        UnityEngine.Debug.Log("Patrol should be moving");
    }

    private void ContinuePatrol()
    {
        //if done attacking, move to the next position away from the player
        currentPatrolEnemyState = PatrolEnemyState.Patroling;
        float distanceToNextPosition = patrolPoints[currentPointIndex++].position.x - transform.parent.transform.position.x;

        //if the distance to next point is having to go past the player, then find the next location
        if (Mathf.Abs(distanceToNextPosition) >= Mathf.Abs(distanceToPlayer) && Mathf.Sign(distanceToNextPosition)== Mathf.Sign(distanceToPlayer))
        {
            int index = currentPointIndex;
            foreach (var position in patrolPoints)
            {
                if (index >= patrolPoints.Length) index = 0;
                float distanceToPosition = Mathf.Abs(patrolPoints[index].position.x - transform.parent.transform.position.x);
                if (Mathf.Sign(distanceToPlayer) != Mathf.Sign(distanceToPosition))
                {
                    currentPointIndex = index;
                    TurnCheck();
                    return;
                }
                index++;
            }
        }
    }

    public void Attack()
    //this is called in the animation as an animation event
    {
        UnityEngine.Debug.Log("Enemy attack has been called");
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackCenterTransform.position, attackRange, baseEnemyData.playerLayer);

        foreach (var collider in hitPlayer)
        {
            UnityEngine.Debug.Log($"{collider.gameObject.name} is in the enemy's hitplayer array!");
        }

        if (hitPlayer.Length > 0) UnityEngine.Debug.Log("Player has been hit by enemy!");

        if (hitPlayer.Length > 0)
        {
            PlayerCharacter.Instance.TakeDamage(baseEnemyData.amountToDamage);
            UnityEngine.Debug.Log("Player should have been damaged");

            Array.Clear(hitPlayer, 0, hitPlayer.Length);
        }
    }

    private void OnDrawGizmosSelected()
    {
        //draws sphere for attack range
        if (attackCenterTransform == null || attackRange == 0f)
        {
            UnityEngine.Debug.LogWarning("Either the attack range is 0f or the attack range's center is null! In order to draw gizmos, make sure these are both set!");
        }
        Gizmos.color = attackRangeColor;
        Gizmos.DrawWireSphere(attackCenterTransform.position, attackRange);

        if (baseEnemyData.alertRange == 0f || alertCenterTransform == null || ignorePlayer)
        {
            UnityEngine.Debug.LogWarning("Either the alert range is 0f or the alert range's center is null! In order to draw gizmos, make sure these are both set!");
        }
        Gizmos.color = alertRangeColor;
        Gizmos.DrawWireSphere(alertCenterTransform.position, baseEnemyData.alertRange);
    }
}
