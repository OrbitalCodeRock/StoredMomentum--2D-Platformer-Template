using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallslideState : PlayerBaseState
{

    public enum WallSlideOrientation{
        RIGHT,
        LEFT,
    }

    private WallSlideOrientation slideOrientation;

    public PlayerWallslideState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory){
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
        RaycastHit2D hit = Physics2D.CapsuleCast(Ctx.GroundCheckPoint.position, Ctx.GroundCheckSize, CapsuleDirection2D.Vertical, 0, Vector2.down, 0.1f, Ctx.WalkableLayers);
        if (hit.collider)
        {
            Ctx.LastGroundedSurface = hit.collider;
            if (hit.normal != Ctx.LastSurfaceNormal)
            {
                Vector2 perp = -Vector2.Perpendicular(hit.normal);
            }
            Ctx.LastSurfaceNormal = hit.normal;
            SwitchState(Factory.Grounded());
            return;
        }
        switch(slideOrientation){
            case WallSlideOrientation.RIGHT:
                if(Ctx.MoveInput.x <= 0 || !Ctx.WallSlideColliderRight.IsTouchingLayers(Ctx.WallSlideLayers.value)){
                    SwitchState(Factory.Airborne());
                    return;
                }
                break;
            case WallSlideOrientation.LEFT:
                if(Ctx.MoveInput.x >= 0 || !Ctx.WallSlideColliderLeft.IsTouchingLayers(Ctx.WallSlideLayers.value)){
                    SwitchState(Factory.Airborne());
                    return;
                }
                break;
        }
    }
    public override void ExitState()
    {

    }
    public override bool CheckSwitchStates()
    {
        if(Ctx.LastJumpPressTime > Ctx.LastJumpTime)
        {
            SetSubState(Factory.WallJump());
            return true;
        }
        return false;
    }
    public override void InitializeSubState()
    {
        
    }

    public void setSlideOrientation(WallSlideOrientation slideOrientation){
        this.slideOrientation = slideOrientation;
    }
}
