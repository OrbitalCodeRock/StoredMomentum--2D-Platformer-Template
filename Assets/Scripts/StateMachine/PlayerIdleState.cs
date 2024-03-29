using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {

    }

    public override void EnterState()
    {
        
    }
    public override void UpdateState()
    {
        if (CheckSwitchStates()) return;
    }

    public override void FixedUpdateState()
    {
       Ctx.comeToStop();
    }

    public override void ExitState()
    {

    }
    public override bool CheckSwitchStates()
    {
        if (Mathf.Abs(Ctx.MoveInput.x) > 0.01f)
        {
            SwitchState(Factory.Walk());
            // Play walking animation
            if(Ctx.PlayerAnimator != null && !Ctx.IsJumping)Ctx.PlayerAnimator.SetInteger("AnimationState", 1);
            return true;
        }
        return false;
    }
    public override void InitializeSubState()
    {

    }
}
