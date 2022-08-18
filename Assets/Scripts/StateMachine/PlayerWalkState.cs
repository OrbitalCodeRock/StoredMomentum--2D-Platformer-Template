using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
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
        if (!Ctx.ConserveMomentum)
        {
            Ctx.Run(1);
        }
        else if (Mathf.Sign(Ctx.MoveInput.x) != Mathf.Sign(Ctx.PlayerBody.velocity.x))
        {
            Ctx.Run(0.25f);
        }
        if (Mathf.Abs(Ctx.PlayerBody.velocity.x) < Ctx.Data.runMaxSpeed)
        {
            Ctx.ConserveMomentum = false;
        }
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
        return false;
    }
    public override void InitializeSubState() 
    {

    }
}
