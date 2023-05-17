using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerBaseState
{
    public PlayerWallJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory): base(currentContext, playerStateFactory){

    }

    public override void EnterState()
    {
        // Debug.Log("Entered WallJump");
        Ctx.IsJumping = true;
        WallJump();
        // Set animation state to a wall jump animation.
        Ctx.LastJumpTime = Time.timeSinceLevelLoad;
    }
    public override void UpdateState()
    {
        if (CheckSwitchStates()) return;
    }

    public bool CheckSwitchStates()
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

    private void WallJump(){
        PlayerWallSlideState wallSlideState = (PlayerWallSlideState)Factory.Wallslide();
        PlayerWallSlideState.WallSlideOrientation orientation = wallSlideState.getSlideOrientation();
        Vector2 jumpDirection = Vector2.up;
        switch(orientation){
            case PlayerWallSlideState.WallSlideOrientation.RIGHT:
                jumpDirection = Quaternion.Euler(0,0,Ctx.Data.getWallJumpAngle()) * Vector2.up;
                break;
            case PlayerWallSlideState.WallSlideOrientation.LEFT:
                jumpDirection = Quaternion.Euler(0,0, -Ctx.Data.getWallJumpAngle()) * Vector2.up;
                break;
        }
        float forceMagnitude = Ctx.Data.getWallJumpForce();
        Vector2 force = forceMagnitude * jumpDirection;
        if (Ctx.PlayerBody.velocity.y < 0)
        {
            force -= new Vector2(0, Ctx.PlayerBody.velocity.y);
        }
        Ctx.PlayerBody.AddForce(force, ForceMode2D.Impulse);
    }
}
