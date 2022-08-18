using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{

    public PlayerAirborneState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
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
        }
        Collider2D col = Physics2D.OverlapCapsule(Ctx.GroundCheckPoint.position, Ctx.GroundCheckSize, CapsuleDirection2D.Vertical, 0, Ctx.WalkableLayers);
        if (col && Ctx.IsFalling)
        {
            Ctx.LastGroundedSurface = col;
            SwitchState(Factory.Grounded());
            return;
        }
        else
        {
            Ctx.Drag(Ctx.Data.airDrag);
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
        if(!Ctx.IsJumping && Time.timeSinceLevelLoad - Ctx.LastPressedJumpTime <= Ctx.Data.jumpBufferTime && Time.timeSinceLevelLoad - Ctx.LastOnGroundTime <= Ctx.Data.coyoteTime)
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
