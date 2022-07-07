using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFactory
{
    private PlayerStateMachine _context;

    private enum PlayerStates
    {
        idle,
        walk,
        jump,
        grounded,
    }

    Dictionary<PlayerStates, PlayerBaseState> _states = new Dictionary<PlayerStates, PlayerBaseState>();

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        _states[PlayerStates.idle] = new PlayerIdleState(_context, this);
        _states[PlayerStates.walk] = new PlayerWalkState(_context, this);
        _states[PlayerStates.jump] = new PlayerJumpState(_context, this);
        _states[PlayerStates.grounded] = new PlayerGroundedState(_context, this);

    }

    public PlayerBaseState Idle()
    {
        return _states[PlayerStates.idle];
    }

    public PlayerBaseState Walk()
    {
        return _states[PlayerStates.walk];
    }

    public PlayerBaseState Jump()
    {
        return _states[PlayerStates.jump];
    }

    public PlayerBaseState Grounded()
    {
        return _states[PlayerStates.grounded];
    }
}
