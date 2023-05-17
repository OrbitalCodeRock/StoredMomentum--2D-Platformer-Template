using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    private bool shouldJump = false;

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
        //Debug.Log("Enter Airborne");
    }
    public override void UpdateState()
    {
        shouldJump = ShouldJump();

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
        // If the player is holding right while colliding with a wall on the right,
        // or holding left while colliding with a wall on the left.
        // Switch states to the wallslide state
        if(Ctx.MoveInput.x < 0 && Ctx.WallSlideColliderLeft.IsTouchingLayers(Ctx.WallSlideLayers.value)){
            PlayerWallSlideState wallslideState = (PlayerWallSlideState)Factory.Wallslide();
            wallslideState.setSlideOrientation(PlayerWallSlideState.WallSlideOrientation.LEFT);
            SwitchState(wallslideState);
            return;
        }
        else if(Ctx.MoveInput.x > 0 && Ctx.WallSlideColliderRight.IsTouchingLayers(Ctx.WallSlideLayers.value)){
            PlayerWallSlideState wallslideState = (PlayerWallSlideState)Factory.Wallslide();
            wallslideState.setSlideOrientation(PlayerWallSlideState.WallSlideOrientation.RIGHT);
            SwitchState(wallslideState);
            return;
        }
        if(shouldJump){
            shouldJump = false;
            SetSubState(Factory.Jump());
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
        //Debug.Log("Exit Airborne");
    }
    public bool ShouldJump()
    {
        if(!Ctx.IsJumping && Ctx.LastJumpPressTime > Ctx.LastJumpTime && Time.timeSinceLevelLoad - Ctx.LastJumpPressTime <= Ctx.Data.jumpBufferTime && Time.timeSinceLevelLoad - Ctx.LastOnGroundTime <= Ctx.Data.coyoteTime)
        {
            return true;
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
