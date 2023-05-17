using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlideState : PlayerBaseState
{

    public enum WallSlideOrientation{
        RIGHT,
        LEFT,
    }

    private bool shouldWallJump = false;

    private IEnumerator clingRoutine = null;

    private WallSlideOrientation slideOrientation;

    public PlayerWallSlideState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory){
        IsRootState = true;
    }
    public override void EnterState()
    {
        Debug.Log("Enter Wallslide");
    }
    public override void UpdateState()
    {
        shouldWallJump = ShouldWallJump();
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
                if(!Ctx.WallSlideColliderRight.IsTouchingLayers(Ctx.WallSlideLayers.value)){
                    SwitchState(Factory.Airborne());
                    return;
                }
                else if(Ctx.MoveInput.x <= 0 && clingRoutine == null){
                    clingRoutine = ClingToWall(Ctx.Data.getWallSlideClingTime());
                    Ctx.StartCoroutine(clingRoutine);
                }
                break;
            case WallSlideOrientation.LEFT:
                if(!Ctx.WallSlideColliderLeft.IsTouchingLayers(Ctx.WallSlideLayers.value)){
                    SwitchState(Factory.Airborne());
                    return;
                }
                else if(Ctx.MoveInput.x >= 0 && clingRoutine == null){
                    clingRoutine = ClingToWall(Ctx.Data.getWallSlideClingTime());
                    Ctx.StartCoroutine(clingRoutine);
                }
                break;
        }
        if(shouldWallJump){
            shouldWallJump = false;
            SetSubState(Factory.WallJump());
        }
        // Work on refinining this. This section of code is responsible for slowing down the player in a wall slide.
        else if(Ctx.PlayerBody.velocity.y < -Mathf.Abs(Ctx.Data.getWallSlideSpeed())){
            Vector2 expectedVelocity = new Vector2(0, Ctx.PlayerBody.velocity.y + (Physics2D.gravity.y * Ctx.PlayerBody.gravityScale)/Ctx.PlayerBody.mass);
            Ctx.PlayerBody.AddForce(Ctx.PlayerBody.mass * Vector2.up * -expectedVelocity.y * (1/Ctx.Data.getMaxWallSlideSlowdownTime()));
        }
    }
    public override void ExitState()
    {
        if(clingRoutine != null){
            Ctx.StopCoroutine(clingRoutine);
            clingRoutine = null;
        }
        Debug.Log("Exit Wallslide");
    }
    public bool ShouldWallJump()
    {
        // Maybe I should have a seperate jump buffer time for walljumps
        if(Ctx.LastJumpPressTime > Ctx.LastJumpTime && Time.timeSinceLevelLoad - Ctx.LastJumpPressTime <= Ctx.Data.jumpBufferTime)
        {
            return true;
        }
        return false;
    }
    public void setSlideOrientation(WallSlideOrientation slideOrientation){
        this.slideOrientation = slideOrientation;
    }

    public WallSlideOrientation getSlideOrientation(){
        return slideOrientation;
    }

    private IEnumerator ClingToWall(float clingTime){
        yield return new WaitForSeconds(clingTime);
        switch(slideOrientation){
            case WallSlideOrientation.RIGHT:
                if(Ctx.MoveInput.x <= 0) SwitchState(Factory.Airborne());
                break;
            case WallSlideOrientation.LEFT:
                if(Ctx.MoveInput.x >= 0) SwitchState(Factory.Airborne());
                break;
        }
    }
}
