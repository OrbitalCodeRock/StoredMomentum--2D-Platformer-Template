using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallControlState : PlayerBaseState
{
    // Currently not in use, but the idea is to seperate free-falling from regular walking if needed

    private PlayerStateMachine context;
    private PlayerStateFactory factory;

    public PlayerFallControlState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        context = currentContext;
        factory = playerStateFactory;
    }
}
