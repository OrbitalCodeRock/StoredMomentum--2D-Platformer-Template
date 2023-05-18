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
        airborne,
        wallslide,
        walljump,
    }

    Dictionary<PlayerStates, PlayerBaseState> _states = new Dictionary<PlayerStates, PlayerBaseState>();

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        _states[PlayerStates.idle] = new PlayerIdleState(_context, this);
        _states[PlayerStates.walk] = new PlayerWalkState(_context, this);
        _states[PlayerStates.jump] = new PlayerJumpState(_context, this);
        _states[PlayerStates.grounded] = new PlayerGroundedState(_context, this);
        _states[PlayerStates.airborne] = new PlayerAirborneState(_context, this);
        _states[PlayerStates.wallslide] = new PlayerWallSlideState(_context, this);
        _states[PlayerStates.walljump] = new PlayerWallJumpState(_context, this);
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

    public PlayerBaseState Airborne()
    {
        return _states[PlayerStates.airborne];
    }

    public PlayerBaseState WallSlide(){
        return _states[PlayerStates.wallslide];
    }

    public PlayerBaseState WallJump(){
        return _states[PlayerStates.walljump];
    }
}
