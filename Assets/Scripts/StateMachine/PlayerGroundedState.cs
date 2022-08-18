using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{

    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
        Ctx.IsJumping = false;
        Ctx.IsFalling = false;
        Ctx.IsGrounded = true;
        Debug.Log("Grounded!");
    }
    public override void UpdateState()
    {
        if (CheckSwitchStates()) return;
    }

    public override void FixedUpdateState()
    {
        Collider2D col = Physics2D.OverlapCapsule(Ctx.GroundCheckPoint.position, Ctx.GroundCheckSize, CapsuleDirection2D.Vertical, 0, Ctx.WalkableLayers);
        if (!col)
        {
            SwitchState(Factory.Airborne());
            return;
        }
        Ctx.LastGroundedSurface = col;
        // add something to if statement to cancel out any upward velocity caused just by walking. This is meant to account for cases where you jump but never leave the ground  
        /*else if(Ctx.PlayerBody.velocity.y <= 0)
        {
            Ctx.IsJumping = false;
        }*/
        Ctx.Drag(Ctx.Data.groundFriction);
    }

    public override void ExitState()
    {
        // Might need to change how often LastOnGroundTime is updated in the future
        Ctx.LastOnGroundTime = Time.timeSinceLevelLoad;
        Ctx.IsGrounded = false;
    }
    public override bool CheckSwitchStates()
    {
        if(!Ctx.IsJumping && Time.timeSinceLevelLoad - Ctx.LastPressedJumpTime <= Ctx.Data.jumpBufferTime)
        {
            SetSubState(Factory.Jump());
            return true;
        }
        return false;
    }
    public override void InitializeSubState()
    {
        if(Mathf.Abs(Ctx.MoveInput.x) <= 0.01f){
            SetSubState(Factory.Idle());
        }
        else{
            SetSubState(Factory.Walk());
        }
    }
}
