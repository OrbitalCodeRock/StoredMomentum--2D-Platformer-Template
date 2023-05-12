using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Data")]
public class PlayerData : ScriptableObject
{
    public float gravityScale;
    public float fallGravityMult;
    public float quickFallGravityMult;

    public float airDrag;
    public float groundFriction;

    public float coyoteTime; // Grace period to jump after walking off a surface.

    // A curve meant to be used for accelerating the player.
    // I'm thinking that this curve will be evaluated based on the velocity of the player.
    [SerializeField]
    private AnimationCurve AccelerationCurve;

    [SerializeField]
    private float maxRunSpeed;

    public float runMaxSpeed;
    public float runAccel;
    public float runDeccel;
    public float airAccel;
    public float airDeccel;

    public float accelInAir;
    public float deccelInAir;

    public float accelPower;
    public float stopPower;
    public float turnPower;

    public float jumpForce;
    public float jumpCutMultiplier;
    public float jumpBufferTime; // Allows the player to buffer a jump input.

    public bool doKeepRunMomentum;

    public AnimationCurve getAccelerationCurve()
    {
        return AccelerationCurve;
    }

    public float getMaxRunSpeed()
    {
        return maxRunSpeed;
    }
}
