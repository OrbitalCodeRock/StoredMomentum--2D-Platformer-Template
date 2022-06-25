using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{

    // State Variables
    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;

    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public PlayerData Data { get { return _data; } set { _data = value; } }

    public GameControls Controls;

    [SerializeField]
    private Camera _mainCamera;

    private ObjectSelector _objectSelector;

    public GameObject MomentumUI;
    private GameObject _storedIndicator;

    [SerializeField]
    private PlayerData _data;

    private Rigidbody2D _targetBody;

    public Rigidbody2D PlayerBody { get; private set; }

    public Transform GroundCheckPoint;
    public float GroundCheckRadius;
    [SerializeField]
    private LayerMask _walkableLayers;

    public enum ManipulationState
    {
        Store,
        Release
    }

    public ManipulationState momentumManipulation;

    public bool IsSlowingTime { get; private set; }

    public Vector2 MoveInput { get; private set; }

    public Vector2 StoredVelocity { get; private set; }
    public float StoredMass { get; private set; }

    public float LastOnGroundTime { get; private set; }

    public bool IsGrounded { get; private set; }

    public bool IsJumping { get; private set; } = true;

    public bool ConserveMomentum { get; private set; }
    public float LastPressedJumpTime { get; private set; }
    public float LastMomentumStoreTime { get; private set; }
    public float LastMomentumReleaseTime { get; private set; }


    private void OnMoveStart(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCancel(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
    }

    private void OnJumpStart(InputAction.CallbackContext args)
    {
        Debug.Log("JumpStarted");
        LastPressedJumpTime = Time.timeSinceLevelLoad;
    }

    private Coroutine delayedCut;

    private void JumpCut()
    {
        //applies force downward when the jump button is released. Allowing the player to control jump height
        PlayerBody.AddForce(Vector2.down * PlayerBody.velocity.y * (1 - _data.jumpCutMultiplier), ForceMode2D.Impulse);
    }

    private bool CanJumpCut()
    {
        return IsJumping && PlayerBody.velocity.y > 0;
    }

    IEnumerator delayedJumpCut()
    {
        yield return new WaitForSeconds(_data.jumpBufferTime - (Time.timeSinceLevelLoad - LastPressedJumpTime));
        if (CanJumpCut())
        {
            JumpCut();
        }
    }

    private void OnJumpEnd(InputAction.CallbackContext args)
    {

        Debug.Log("JumpEnded");
        if (CanJumpCut())
        {
            JumpCut();
        }
        else
        {
            if (delayedCut != null) StopCoroutine(delayedCut);
            delayedCut = StartCoroutine(delayedJumpCut());
        }
    }

    private void OnMomentumManipulate(InputAction.CallbackContext args)
    {
        // Add an if statement to limit action spamming
        switch (momentumManipulation)
        {
            case ManipulationState.Store:
                StoredVelocity = PlayerBody.velocity;
                _targetBody = PlayerBody;
                StoredMass = PlayerBody.mass;
                break;
            case ManipulationState.Release:
                break;
        }
    }

    private void OnTimeSlow(InputAction.CallbackContext args)
    {
        Time.timeScale = 0.5f;
        IsSlowingTime = true;
        MomentumUI.SetActive(true);
        _objectSelector.enabled = true;
        _objectSelector.mouseClick.performed += OnClick;
    }

    private void OnTimeRestore(InputAction.CallbackContext args)
    {
        _objectSelector.mouseClick.performed -= OnClick;
        _objectSelector.Deselect();
        _objectSelector.enabled = false;
        switch (momentumManipulation)
        {
            case ManipulationState.Store:
                StoredVelocity = _targetBody.velocity;
                StoredMass = _targetBody.mass;
                momentumManipulation = ManipulationState.Release;
                LastMomentumStoreTime = Time.timeSinceLevelLoad;
                break;
            case ManipulationState.Release:
                ConserveMomentum = true;
                ReleaseMomentum();
                momentumManipulation = ManipulationState.Store;
                LastMomentumReleaseTime = Time.timeSinceLevelLoad;
                break;
        }
        MomentumUI.SetActive(false);
        Time.timeScale = 1f;
        IsSlowingTime = false;
    }
    private void ReleaseMomentum()
    {
        float forceMagnitude = StoredVelocity.magnitude * StoredMass;
        Vector2 direction = ((Vector2)_mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - _targetBody.position).normalized;
        _targetBody.AddForce(forceMagnitude * direction, ForceMode2D.Impulse);
    }



    private void OnClick(InputAction.CallbackContext args)
    {
        Rigidbody2D newBody = _objectSelector.SelectedObject?.GetComponent<Rigidbody2D>();
        switch (momentumManipulation)
        {
            case ManipulationState.Store:
                if (newBody != null)
                {
                    StoredVelocity = newBody.velocity;
                    StoredMass = newBody.mass;
                    momentumManipulation = ManipulationState.Release;
                }
                break;
            case ManipulationState.Release:
                // Restore the Indicator of the last selected object
                // store the Indicator of the currently selected object
                // replace with new Indicator that shows release trajectory
                break;
        }
        if (newBody != null)
        {
            _targetBody = newBody;
        }

    }
    private void Awake()
    {

        _objectSelector = GameObject.Find("CameraCanvas").GetComponent<ObjectSelector>();

        Controls = new GameControls();

        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        Controls.Player.Move.performed += OnMoveStart;
        Controls.Player.Move.canceled += OnMoveCancel;

        Controls.Player.JumpStart.performed += OnJumpStart;
        Controls.Player.JumpEnd.performed += OnJumpEnd;

        Controls.Player.MomentumManipulate.performed += OnMomentumManipulate;
        Controls.Player.TimeSlow.performed += OnTimeSlow;
        Controls.Player.TimeRestore.performed += OnTimeRestore;

        PlayerBody = this.GetComponent<Rigidbody2D>();
        _targetBody = PlayerBody;
    }
    private void OnEnable()
    {
        Controls.Enable();
    }

    private void OnDisable()
    {
        Controls.Disable();
    }

    public void SetGravityScale(float scale)
    {
        PlayerBody.gravityScale = scale;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetGravityScale(_data.gravityScale);
    }

    // Update is called once per frame
    void Update()
    {
        _currentState.UpdateState();        
    }
}
