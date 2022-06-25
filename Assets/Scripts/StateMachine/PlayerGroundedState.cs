using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {

    }

    public override void EnterState()
    {
        Debug.Log("Grounded!");
    }
    public override void UpdateState()
    {

    }
    public override void ExitState()
    {

    }
    public override void CheckSwitchStates()
    {
        if(Time.timeSinceLevelLoad - _ctx.LastPressedJumpTime <= _ctx.Data.jumpBufferTime)
        {
            SwitchState(_factory.Jump());
        }
    }
    public override void InitializeSubState()
    {

    }
}
