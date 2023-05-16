using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Data")]
public class PlayerData : ScriptableObject
{
    public float gravityScale;
    public float fallGravityMult;
    public float quickFallGravityMult;

    public float coyoteTime; // Grace period to jump after walking off a surface.

    
    // The curves below are meant to take the place of shaping functions, meaning that they are supposed to have values between 0 and 1.
    // So far, they are evaluated using the ratio of the player's x-velocity to maxRunSpeed.
    // Then, the value returned by the curve is multiplied by floats representing maximum acceleration values.

    // A curve meant to be used for Accelerating the player while running
    [SerializeField]
    private AnimationCurve runAccelerationCurve;

    [SerializeField]
    private float maxRunAcceleration;

    // A curve meant to be used for Accelerating the player in the opposite direction when turning.
    [SerializeField]
    private AnimationCurve turnAccelerationCurve;

    [SerializeField]
    private float maxTurnAcceleration;

    // A curve meant to be used for stopping the player when they are idle.
    [SerializeField]
    private AnimationCurve stopAccelerationCurve;

    [SerializeField]
    private float maxStopAcceleration;

    [SerializeField]
    private float maxRunSpeed;

    [SerializeField]
    private float linearDragCoefficent;

    public float jumpForce;
    public float jumpCutMultiplier;
    public float jumpBufferTime; // Allows the player to buffer a jump input.

    [SerializeField]
    private float wallJumpAngle;
    [SerializeField]
    private float wallJumpForce;

    public AnimationCurve getRunAccelerationCurve()
    {
        return runAccelerationCurve;
    }

    public AnimationCurve getTurnAccelerationCurve()
    {
        return turnAccelerationCurve;
    }

    public AnimationCurve getStopAccelerationCurve()
    {
        return stopAccelerationCurve;
    }

    public float getMaxStopAcceleration()
    {
        return maxStopAcceleration;
    }

    public float getMaxRunSpeed()
    {
        return maxRunSpeed;
    }

    public float getMaxRunAcceleration()
    {
        return maxRunAcceleration;
    }

    public float getLinearDragCoefficent()
    {
        return linearDragCoefficent;
    }
    
    public float getMaxTurnAcceleration()
    {
        return maxTurnAcceleration;
    }

    public float getWallJumpAngle(){
        return this.wallJumpAngle;
    }

    public float getWallJumpForce(){
        return this.wallJumpForce;
    }
}
