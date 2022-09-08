using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{

    // TODO: Make GroundCheckSize larger, add logic to disallow jumping on slopes that are too steep
    // Glitches: Double Jump boost thingy (You can tap jump once then jump again while running into a slope for a sort of mega jump).


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
    public Vector2 GroundCheckSize;
    [SerializeField]
    private LayerMask _walkableLayers;

    public LayerMask WalkableLayers { get { return _walkableLayers; } set { _walkableLayers = value; } }

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

    public float LastOnGroundTime { get; set; }

    [SerializeField]
    private bool _isGrounded;

    public bool IsGrounded { get { return _isGrounded; } set { _isGrounded = value; } }

    [SerializeField]
    private bool isJumping;
    public bool IsJumping { get { return isJumping; } set { isJumping = value; } }

    public bool IsFalling { get; set; }

    public Collider2D LastGroundedSurface { get; set; }

    public Vector2 LastSurfaceNormal { get; set; }

    public bool ConserveMomentum { get; set; }
    public float LastJumpPressTime { get; set; }
    public float LastJumpTime { get; set; }
    public float LastMomentumStoreTime { get; set; }
    public float LastMomentumReleaseTime { get; set; }


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
        //Debug.Log("JumpPressed");
        LastJumpPressTime = Time.timeSinceLevelLoad;
    }

    private Coroutine delayedCut;

    // Credit to https://github.com/Dawnosaur/platformer-movement for general math behind acceleration and decceleration forces.
    // Honestly, I don't feel like I fully grasp the logic behind how the magnitude of the movement force is calculated. Maybe there is a more precise/different way to go about it.
    public void Run(float lerpAmount)
    {
        Vector2 slopeVector = Vector2.right;
        float targetSpeed = MoveInput.x * _data.runMaxSpeed;
        if (IsGrounded)
        {
            slopeVector = -Vector2.Perpendicular(LastSurfaceNormal).normalized;
            targetSpeed *= slopeVector.x;
        }
        float speedDif = targetSpeed - PlayerBody.velocity.x;

        float accelRate;

        if (_data.doKeepRunMomentum && ((PlayerBody.velocity.x > targetSpeed && targetSpeed > 0.01f) || (PlayerBody.velocity.x < targetSpeed && targetSpeed < -0.01f)))
        {
            accelRate = 0;
        }
        else
        {
            if (IsGrounded)
            {
                // If the target speed is greater than a minimum (0.01), use running acceleration. Otherwise, decelerate.
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _data.runAccel : _data.runDeccel;
            }
            else
            {
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _data.runAccel * _data.accelInAir : _data.runDeccel * _data.deccelInAir;
            }
        }

        float velPower;
        if (Mathf.Abs(targetSpeed) < 0.01f)
        {
            velPower = _data.stopPower;
        }
        else if (Mathf.Abs(PlayerBody.velocity.x) > 0 && (Mathf.Sign(targetSpeed) != Mathf.Sign(PlayerBody.velocity.x)))
        {
            velPower = _data.turnPower;
        }
        else
        {
            velPower = _data.accelPower;
        }

        // applies acceleration to speed difference, then is raised to a set power so the acceleration increases with higher speeds, finally multiplies by sign to preserve direction
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        movement = Mathf.Lerp(PlayerBody.velocity.x, movement, lerpAmount); // lerp so that we can prevent the Run from immediately slowing the player down, in some situations eg wall jump, dash 

        Vector2 force = movement * slopeVector;
        //Debug.Log("Force: " + force + ", Movement: " + movement + ", Target Velocity: " + targetSpeed + ", X Velocity: " + PlayerBody.velocity.x);
        PlayerBody.AddForce(force); // applies force force to rigidbody


    }

    public void Jump()
    {
        float force = _data.jumpForce;
        // Cancel out downward forces on jump;
        if (PlayerBody.velocity.y < 0)
        {
            force -= PlayerBody.velocity.y;
        }
        PlayerBody.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

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
        yield return new WaitForSeconds(_data.jumpBufferTime - (Time.timeSinceLevelLoad - LastJumpPressTime));
        if (CanJumpCut())
        {
            JumpCut();
        }
    }

    private void OnJumpEnd(InputAction.CallbackContext args)
    {

        //Debug.Log("JumpReleased");
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
        _currentState = _states.Airborne();
        _currentState.EnterState();

        Controls.Player.Move.performed += OnMoveStart;
        Controls.Player.Move.canceled += OnMoveCancel;

        Controls.Player.JumpStart.performed += OnJumpStart;
        Controls.Player.JumpEnd.performed += OnJumpEnd;

        /*Controls.Player.MomentumManipulate.performed += OnMomentumManipulate;
        Controls.Player.TimeSlow.performed += OnTimeSlow;
        Controls.Player.TimeRestore.performed += OnTimeRestore;*/

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

    public void Drag(float amount)
    {
        Vector2 force = amount * PlayerBody.velocity.normalized;
        force.x = Mathf.Min(Mathf.Abs(PlayerBody.velocity.x), Mathf.Abs(force.x));
        force.y = Mathf.Min(Mathf.Abs(PlayerBody.velocity.y), Mathf.Abs(force.y));
        force.x *= Mathf.Sign(PlayerBody.velocity.x); //finds direction to apply force
        force.y *= Mathf.Sign(PlayerBody.velocity.y);

        PlayerBody.AddForce(-force, ForceMode2D.Impulse);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetGravityScale(_data.gravityScale);
    }

    // Update is called once per frame
    void Update()
    {
        _currentState.CallUpdateStates();        
    }

    private void FixedUpdate()
    {
        _currentState.CallFixedUpdateStates();
    }

    private void OnDrawGizmosSelected()
    {
        
    }
}
