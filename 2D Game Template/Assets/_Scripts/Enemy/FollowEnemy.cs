using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This enemy will follow the player when it is detected in is agression range and will go back to idle when the player leaves. If the player is in its agressive
/// range, it will try to get closer and when it reaches the minimum distance, it will stop and attack the player.
/// </summary>

public class FollowEnemy : Enemy
{
    [Header("Follow Enemy")]
    [SerializeField] private float speed;
    [SerializeField][Tooltip("The distance when the enemy stops in front of the target's position")] private float minimumDistance;

    [Tooltip("The radius of the enemy's attacl range")][SerializeField] private float attackRange;
    [Tooltip("The point where the enemy's attack circle is centered")][SerializeField] private Transform attackPoint;
    [SerializeField] private float attackCooldown;
    private bool hasAttacked;

    [Tooltip("Whether the enemy is facing right in default pose before entering play mode")][SerializeField] private bool isFacingRight;

    [Tooltip("To prevent bugs and player getting stuck on top of enemies, the turn delay makes it so that when enemies has to turn to face player, there is a delay")]
    [SerializeField] private float turnDelay;
    private bool turnStarted = false;

    [Header("Tools")]
    [SerializeField] private Color alertRangeColor;
    [SerializeField] private Color attackRangeColor;


    // Start is called before the first frame update
    private void Start()
    {
        base.Start();
        if (animator!=null) animator.SetFloat("speed_f", 0.0f);
    }


    // Update is called once per frame
    private void FixedUpdate()
    {
        base.FixedUpdate();
        if (target != null)
        {
            float distanceToTarget = target.position.x - transform.position.x;

            if (playerInAlertRange)
            {
                //if player is in the agro range but has not yet reached distance to attack
                if (Mathf.Abs(distanceToTarget) > minimumDistance) MoveTowardsTarget(distanceToTarget);

                //if player is in the agro range but has reached distance to attack
                else if (Mathf.Abs(distanceToTarget) <= minimumDistance)
                {
                    //a cooldown on the enemy's attack
                    if (!hasAttacked)
                    {
                        hasAttacked = true;
                        if (animator != null) animator.SetTrigger("attack_trig");
                        StartCoroutine(AttackCooldown());
                    }
                    else MoveTowardsTarget(distanceToTarget);
                }
            }

            else
            {
                if (animator!=null) animator.SetFloat("speed_f", 0.0f);
            }
        }
    }

    private void MoveTowardsTarget(float distanceToTarget)
    {
        if (animator != null) animator.SetFloat("speed_f", 10.0f);
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        //if not facing player's direction, turn
        if (!turnStarted && (distanceToTarget > 0.0f && !isFacingRight) || (distanceToTarget < 0.0f && isFacingRight)) StartCoroutine(Turn());
    }

    public void Attack()
    //this is called in the animation as an animation event
    {
        //UnityEngine.Debug.Log("Enemy attack has been called");
        Collider2D[] collidersFound = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, baseEnemyData.playerLayer);

        //if (collidersFound.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
        if (collidersFound.Length>=1)
        {
            PlayerCharacter.Instance.TakeDamage(baseEnemyData.amountToDamage);
            //UnityEngine.Debug.Log("Player should have been damaged");
            collidersFound = null;
        }
    }


    private void OnDrawGizmosSelected()
    {
        //draws sphere for attack range
        if (attackPoint == null || attackRange == 0f)
        {
            UnityEngine.Debug.LogWarning("Either the attack range is 0f or the attack range's center is null! In order to draw gizmos, make sure these are both set!");
        }
        Gizmos.color = attackRangeColor;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        if (baseEnemyData.alertRange == 0f || alertCenterTransform == null)
        {
            UnityEngine.Debug.LogWarning("Either the alert range is 0f or the alert range's center is null! In order to draw gizmos, make sure these are both set!");
        }
        Gizmos.color = alertRangeColor;
        Gizmos.DrawWireSphere(alertCenterTransform.position, baseEnemyData.alertRange);
    }



    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        hasAttacked = false;
    }

    private IEnumerator Turn()
    {
        if (!turnStarted)
        {
            turnStarted = true;
            UnityEngine.Debug.Log($"Turn happeneing and turn started is {turnStarted}");

            //stores scale and flips the enemy along the x axis after delay
            yield return new WaitForSeconds(turnDelay);
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;

            isFacingRight = !isFacingRight;
            turnStarted = false;
        }
        
    }
}