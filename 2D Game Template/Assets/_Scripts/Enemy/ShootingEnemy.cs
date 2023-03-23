using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This enemy will have be able to shoot projectiles at the player and move away from the player if they get close enough
/// </summary>

public class ShootingEnemy : Enemy
{
    [Header("Shooting Enemy")]
    [Tooltip("The shooting enemies speed (used when moving away from player is they are too close)")][SerializeField] private float speed;
    [SerializeField][Tooltip("Distance when the enemy moves away from the target")] private float minimumDistance;
    [SerializeField] private Transform projectileSpawnTransform;

    private float distanceToPlayer;

    public GameObject projectilePrefab;
    [Tooltip("The min time after shooting projectile that enemy shoots another projectile. Note: set both min and max to the same value to avoid random times between shots")]
    [SerializeField] private float minTimeBetweenShots;

    [Tooltip("The max time after shooting projectile that enemy shoots another projectile. Note: set both min and max to the same value to avoid random times between shots")]
    [SerializeField] private float maxTimeBetweenShots;
    private float currentTimeBetweenShots;
    //[Tooltip("A random attack delay between 0 and this number is generated when enemy can attack (after time between shots) desynchronize multiple shooting enemies from shooting at the same time. " +
     //   "The greater this value is, the greater the variation in multiple shooting enemies is.")][SerializeField] private float maxAttackDelay;
    private float nextShotTime;

    [SerializeField] private bool isFacingRight = false;

    [Header("Tools")]
    [SerializeField] private Color alertRangeColor;

    private void Awake() => currentTimeBetweenShots = Time.time + UnityEngine.Random.Range(minTimeBetweenShots, maxTimeBetweenShots);

    private void FixedUpdate()
    {
        base.FixedUpdate();
        if (target!=null) distanceToPlayer = target.position.x - transform.position.x;
        if (baseEnemyData.alertRange> 0 && Mathf.Abs(distanceToPlayer) < baseEnemyData.alertRange)
        {
            if (Time.time > currentTimeBetweenShots) AttackDelay();

            if (Mathf.Abs(distanceToPlayer) < minimumDistance) transform.position = Vector2.MoveTowards(transform.position, target.position, -speed * Time.deltaTime);
            TurnCheck();
        }
    }

    private void Update()
    {
        //UnityEngine.Debug.Log("Enemy is facing: "+ isFacingRight);
    }

    //called in animation event
    private void Attack()=> Instantiate(projectilePrefab, projectileSpawnTransform.position, Quaternion.identity);

    private void Turn()
    {
        //stores scale and flips the player along the x axis
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        isFacingRight = !isFacingRight;
    }

    private void TurnCheck()
    {
        if ((distanceToPlayer < 0 && isFacingRight) || (distanceToPlayer > 0 && !isFacingRight)) Turn();
    }

    void OnDrawGizmosSelected()
    {
        //draws sphere for attack range
        if (minimumDistance == 0.0f || baseEnemyData.alertRange == 0f || alertCenterTransform == null)
        {
            UnityEngine.Debug.LogWarning($"Either {gameObject.name}'s minimum distance is 0, base enemy data's alert range is 0 or the alert center transform is null!");
            return;
        }
        Gizmos.color = alertRangeColor;
        Gizmos.DrawWireSphere(alertCenterTransform.position, baseEnemyData.alertRange);
    }

    /*
    private IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, maxAttackDelay));
        animator?.SetTrigger("attack_trig");
        currentTimeBetweenShots = Time.time + UnityEngine.Random.Range(minTimeBetweenShots, maxTimeBetweenShots);
    }
    */

    private void AttackDelay()
    {
        animator?.SetTrigger("attack_trig");
        currentTimeBetweenShots = Time.time + UnityEngine.Random.Range(minTimeBetweenShots, maxTimeBetweenShots);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

    }
}