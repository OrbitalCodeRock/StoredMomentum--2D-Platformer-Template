using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerBaseState
{
    public PlayerWallJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory): base(currentContext, playerStateFactory){

    }

    public override void EnterState()
    {
        Ctx.IsJumping = true;
        // Use a method for doing a wall jump, allow customization of the wall jump angle and force.
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
}
