using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerBaseState
{
    public PlayerWallJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory): base(currentContext, playerStateFactory){

    }

    public override void EnterState()
    {
        Debug.Log("Entered WallJump");
        Ctx.IsJumping = true;
        WallJump();
        // Set animation state to a wall jump animation.
        Ctx.LastJumpTime = Time.timeSinceLevelLoad;
    }
    public override void UpdateState()
    {
        if (CheckSwitchStates()) return;
    }

    public override void FixedUpdateState()
    {
        
    }

    public override void ExitState()
    {

    }
    public override bool CheckSwitchStates()
    {
        if (Mathf.Abs(Ctx.MoveInput.x) <= 0.01f)
        {
            SwitchState(Factory.Idle());
            return true;
        }
        else
        {
            SwitchState(Factory.Walk());
            return true;
        }
    }
    public override void InitializeSubState()
    {

    }

    private void WallJump(){
        PlayerWallslideState wallSlideState = (PlayerWallslideState)Factory.Wallslide();
        PlayerWallslideState.WallSlideOrientation orientation = wallSlideState.getSlideOrientation();
        Vector2 jumpDirection = Vector2.up;
        switch(orientation){
            case PlayerWallslideState.WallSlideOrientation.RIGHT:
                jumpDirection = Quaternion.Euler(0,0,Ctx.Data.getWallJumpAngle()) * Vector2.up;
                break;
            case PlayerWallslideState.WallSlideOrientation.LEFT:
                jumpDirection = Quaternion.Euler(0,0, -Ctx.Data.getWallJumpAngle()) * Vector2.up;
                break;
        }
        float force = Ctx.Data.getWallJumpForce();
        if (Ctx.PlayerBody.velocity.y < 0)
        {
            force -= Ctx.PlayerBody.velocity.y;
        }
        Ctx.PlayerBody.AddForce(jumpDirection * force, ForceMode2D.Impulse);
    }
}
