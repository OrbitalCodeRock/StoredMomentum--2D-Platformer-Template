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

    public virtual void EnterState(){

    }
    public virtual void UpdateState(){

    }
    public virtual void FixedUpdateState(){

    }
    public virtual void ExitState(){

    }
    public virtual void InitializeSubState(){

    }

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
