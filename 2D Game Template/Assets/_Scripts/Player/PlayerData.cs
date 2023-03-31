using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")] //Create a new playerData object by right clicking in the Project Menu then Create/Player/Player Data and drag onto the player
public class PlayerData : ScriptableObject
{
    [Header("Gravity")]
    [HideInInspector]
    [Tooltip("Downwards force (gravity) needed for the desired jumpHeight and jumpTimeToApex.")]
    public float gravityStrength;
    [HideInInspector]
    [Tooltip("//Strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D).Also the value the player's rigidbody2D.gravityScale is set to.")]
    public float gravityScale;

    [Space(5)]
    [Tooltip("Multiplier to the player's gravityScale when falling.")] public float fallGravityMult;
    [Tooltip("Maximum fall speed (terminal velocity) of the player when falling.")] public float maxFallSpeed;
    [Space(5)]
    [Tooltip("Larger multiplier to the players gravityScale when they are falling and a downwards input is pressed.Seen in games such as Celeste, lets the player fall extra fast if they wish.")]
    public float fastFallGravityMult;

    [Tooltip("Maximum fall speed(terminal velocity) of the player when performing a faster fall.")] public float maxFastFallSpeed;

    [Space(20)]

    [Header("Run")]
    [Tooltip("Target speed we want the player to reach.")] public float runMaxSpeed;
    [Tooltip("The speed at which our player accelerates to max speed, can be set to runMaxSpeed for instant acceleration down to 0 for none at all")] public float runAcceleration; //
    [HideInInspector][Tooltip("The actual force (multiplied with speedDiff) applied to the player.")] public float runAccelAmount;

    [Tooltip("The speed at which our player decelerates from their current speed, can be set to runMaxSpeed for instant deceleration down to 0 for none at all")]
    public float runDecceleration;

    [HideInInspector][Tooltip("Actual force (multiplied with speedDiff) applied to the player")] public float runDeccelAmount;
    [Space(5)]
    [Range(0f, 1)][Tooltip("Multipliers applied to acceleration rate when airborne.")] public float accelInAir;
    [Range(0f, 1)] public float deccelInAir;
    [Space(5)]
    public bool doConserveMomentum = true;

    [Space(20)]

    [Header("Jump")]
    [Tooltip("Height of the player's jump")] public float jumpHeight;
    [Tooltip("Time between applying the jump force and reaching the desired jump height. These values also control the player's gravity and jump force.")]
    public float jumpTimeToApex;
    [HideInInspector][Tooltip("The actual force applied (upwards) to the player when they jump.")] public float jumpForce;

    [Header("Both Jumps")]
    [Tooltip("Multiplier to increase gravity if the player releases thje jump button while still jumping")] public float jumpCutGravityMult;
    [Range(0f, 1)][Tooltip("Reduces gravity while close to the apex (desired max height) of the jump")] public float jumpHangGravityMult;
    [Tooltip("Speeds (close to 0) where the player will experience extra \"jump hang\". The player's velocity.y is closest to 0 at the jump's apex (think of the gradient of a parabola or quadratic function)")]
    public float jumpHangTimeThreshold;
    [Space(0.5f)]
    public float jumpHangAccelerationMult;
    public float jumpHangMaxSpeedMult;

    [Header("Wall Jump")]
    [Tooltip("The actual force (this time set by us) applied to the player when wall jumping.")] public Vector2 wallJumpForce;
    [Space(5)]
    [Range(0f, 1f)][Tooltip("Reduces the effect of player's movement while wall jumping.")] public float wallJumpRunLerp;
    [Range(0f, 1.5f)][Tooltip("Time after wall jumping the player's movement is slowed for.")] public float wallJumpTime;
    [Tooltip("Player will rotate to face wall jumping direction")] public bool doTurnOnWallJump;

    [Space(20)]

    [Header("Slide")]
    public float slideSpeed;
    public float slideAccel;

    [Header("Assists")]
    [Range(0.01f, 0.5f)][Tooltip("Grace period after falling off a platform, where you can still jump")] public float coyoteTime;
    [Range(0.01f, 0.5f)]
    [Tooltip("Grace period after pressing jump where a jump will be automatically performed once the requirements (eg. being grounded) are met.")]
    public float jumpInputBufferTime;


    //Unity Callback, called when the inspector updates
    private void OnValidate()
    {
        //Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        //Calculate the rigidbody's gravity scale (ie: gravity strength relative to unity's gravity value, see project settings/Physics2D)
        gravityScale = gravityStrength / Physics2D.gravity.y;

        UnityEngine.Debug.Log("Gravity scale is "+ gravityScale);

        //Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        //Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        #region Variable Ranges
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
        #endregion
    }
}