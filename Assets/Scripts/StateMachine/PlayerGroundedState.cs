using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
        Debug.Log("Grounded!");
    }
    public override void UpdateState()
    {
        CheckSwitchStates();
    }
    public override void ExitState()
    {

    }
    public override void CheckSwitchStates()
    {
        if(Time.timeSinceLevelLoad - Ctx.LastPressedJumpTime <= Ctx.Data.jumpBufferTime)
        {
            SwitchState(Factory.Jump());
        }
    }
    public override void InitializeSubState()
    {
        if(Mathf.Abs(Ctx.MoveInput.x) <= 0.01f){
            SetSubState(Factory.Idle());
        }
        else{
            SetSubState(Factory.Walk());
        }
    }
}
