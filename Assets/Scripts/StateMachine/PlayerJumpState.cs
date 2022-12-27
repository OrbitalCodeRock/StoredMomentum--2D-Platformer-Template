using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {

    }

    public override void EnterState()
    {
        Ctx.IsJumping = true;
        Ctx.Jump();
        // Start Jumping Rise animation
        Ctx.PlayerAnimator.SetInteger("AnimationState", 2);
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
