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
        // Might need to change how often LastOnGroundTime is updated in the future
        Ctx.LastOnGroundTime = Time.timeSinceLevelLoad;
        //Debug.Log("Grounded!");
    }
    public override void UpdateState()
    {
        if (CheckSwitchStates()) return;
    }

    public override void FixedUpdateState()
    {
        RaycastHit2D hit = Physics2D.CapsuleCast(Ctx.GroundCheckPoint.position, Ctx.GroundCheckSize, CapsuleDirection2D.Vertical, 0, Vector2.down, Ctx.GroundCheckDistance, Ctx.WalkableLayers);
        if (!hit.collider)
        {
            // Start falling animation
            if(Ctx.PlayerAnimator != null && !Ctx.IsJumping)Ctx.PlayerAnimator.SetInteger("AnimationState", 3);
            SwitchState(Factory.Airborne());
            return;
        }
        Ctx.LastGroundedSurface = hit.collider;
        if(hit.normal != Ctx.LastSurfaceNormal)
        {
            Vector2 perp = -Vector2.Perpendicular(hit.normal);
            //Debug.Log(Vector2.SignedAngle(Vector2.right, perp));
        }
        Ctx.LastSurfaceNormal = hit.normal; 
        if (Time.timeSinceLevelLoad - Ctx.LastJumpPressTime > Ctx.Data.jumpBufferTime && (Ctx.PlayerBody.velocity.y <= 0 || Time.timeSinceLevelLoad - Ctx.LastOnGroundTime > Ctx.Data.jumpBufferTime))
        {
            Ctx.IsJumping = false;
        }
        Ctx.applyLinearDrag();
    }

    public override void ExitState()
    {
        // Might need to change how often LastOnGroundTime is updated in the future
        Ctx.LastOnGroundTime = Time.timeSinceLevelLoad;
        Ctx.IsGrounded = false;
    }
    public override bool CheckSwitchStates()
    {
        // Currently there is a glitch where multiple jumps occur where a single jump should, maybe something can be done here to prevent that.
        if(!Ctx.IsJumping && Ctx.LastJumpPressTime > Ctx.LastJumpTime && Time.timeSinceLevelLoad - Ctx.LastJumpPressTime <= Ctx.Data.jumpBufferTime)
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
            // Play Idle animation
            if (Ctx.PlayerAnimator != null) Ctx.PlayerAnimator.SetInteger("AnimationState", 0);
        }
        else{
            SetSubState(Factory.Walk());
            // Play walking animation
            if (Ctx.PlayerAnimator != null) Ctx.PlayerAnimator.SetInteger("AnimationState", 1);
        }
    }
}
