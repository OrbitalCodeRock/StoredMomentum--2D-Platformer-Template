using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    // The purpose of these two variables is to help ensure that the player does not get stuck in an incorrectly airborne state.
    // (and also to keep track of the airborne start time)
    // Making this float less than JumpBufferTime seems to allow a glitch were multiple jumps rapidly occur where only a single jump should.
    private float minimumAirborneTime;
    private float airborneStartTime;

    public PlayerAirborneState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
        minimumAirborneTime = Ctx.Data.jumpBufferTime;
    }

    public override void EnterState()
    {
        InitializeSubState();
        airborneStartTime = Time.timeSinceLevelLoad;
    }
    public override void UpdateState()
    {
        if(CheckSwitchStates()) return;

    }
    public override void FixedUpdateState()
    {
        // Intention is to only enter grounded state on the way down, could change this later if needed
        if(Ctx.PlayerBody.velocity.y <= 0)
        {
            Ctx.IsFalling = true;
            // Start Jumping Fall animation
            if(Ctx.PlayerAnimator != null) Ctx.PlayerAnimator.SetInteger("AnimationState", 3);
        }
        RaycastHit2D hit = Physics2D.CapsuleCast(Ctx.GroundCheckPoint.position, Ctx.GroundCheckSize, CapsuleDirection2D.Vertical, 0, Vector2.down, 0.1f, Ctx.WalkableLayers);
        if (hit.collider && (Ctx.IsFalling || Time.timeSinceLevelLoad - airborneStartTime > minimumAirborneTime))
        {
            Ctx.LastGroundedSurface = hit.collider;
            if (hit.normal != Ctx.LastSurfaceNormal)
            {
                Vector2 perp = -Vector2.Perpendicular(hit.normal);
                //Debug.Log(Vector2.SignedAngle(Vector2.right, perp));
            }
            Ctx.LastSurfaceNormal = hit.normal;
            SwitchState(Factory.Grounded());
            return;
        }
        if (Ctx.PlayerBody.velocity.y >= 0)
        {
            Ctx.SetGravityScale(Ctx.Data.gravityScale);
        }
        else if (Ctx.MoveInput.y < 0)
        {
            Ctx.SetGravityScale(Ctx.Data.gravityScale * Ctx.Data.quickFallGravityMult);
        }
        else
        {
            Ctx.SetGravityScale(Ctx.Data.gravityScale * Ctx.Data.fallGravityMult);
        }
    }
    public override void ExitState()
    {

    }
    public override bool CheckSwitchStates()
    {
        if(!Ctx.IsJumping && Ctx.LastJumpPressTime > Ctx.LastJumpTime && Time.timeSinceLevelLoad - Ctx.LastJumpPressTime <= Ctx.Data.jumpBufferTime && Time.timeSinceLevelLoad - Ctx.LastOnGroundTime <= Ctx.Data.coyoteTime)
        {
            SetSubState(Factory.Jump());
        }
        return false;
    }
    public override void InitializeSubState()
    {
        if (Mathf.Abs(Ctx.MoveInput.x) <= 0.01f)
        {
            SetSubState(Factory.Idle());
        }
        else
        {
            SetSubState(Factory.Walk());
        }
    }
}
