public abstract class PlayerBaseState
{
    private bool _isRootState = false;
    private PlayerStateMachine _ctx;
    private PlayerStateFactory _factory;

    private PlayerBaseState _currentSubState;

    private PlayerBaseState _currentSuperState;

    protected bool IsRootState {set{_isRootState = value;}}
    protected PlayerStateMachine Ctx{ get{return _ctx; }}
    protected PlayerStateFactory Factory{get{return _factory;} }

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        _ctx = currentContext;
        _factory = playerStateFactory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void ExitState();
    // Returns True if state switch was made, false if not
    public abstract bool CheckSwitchStates();
    public abstract void InitializeSubState();

    public void CallUpdateStates()
    {
        UpdateState();
        if(_currentSubState != null){
            _currentSubState.CallUpdateStates();
        }
    }

    public void CallFixedUpdateStates()
    {
        FixedUpdateState();
        if(_currentSubState != null)
        {
            _currentSubState.CallFixedUpdateStates();
        }
    }

    public void CallExitStates(){
        ExitState();
        if(_currentSubState != null){
            _currentSubState.CallExitStates();
        }
    }

    protected void SwitchState(PlayerBaseState newState)
    {

        ExitState();
        newState.EnterState();
        if(_isRootState){
            _ctx.CurrentState = newState;
        }
        else if(_currentSuperState != null){
            _currentSuperState.SetSubState(newState);
        }

    }

    protected void SetSuperState(PlayerBaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(PlayerBaseState newSubState)
    {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
        _currentSubState.EnterState();
    }

}
