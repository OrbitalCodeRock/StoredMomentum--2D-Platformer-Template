using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallslideState : PlayerBaseState
{
    public PlayerWallslideState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory){
        IsRootState = true;
    }

     public override void EnterState()
    {
        InitializeSubState();
    }
    public override void UpdateState()
    {
    
    }
    public override void FixedUpdateState()
    {
       
    }
    public override void ExitState()
    {

    }
    public override bool CheckSwitchStates()
    {
        return false;
    }
    public override void InitializeSubState()
    {
        
    }
}
