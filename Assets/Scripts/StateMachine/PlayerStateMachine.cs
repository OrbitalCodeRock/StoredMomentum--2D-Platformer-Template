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

    [SerializeField]
    private PlayerData _data;

    public Rigidbody2D PlayerBody { get; private set; }

    public Transform GroundCheckPoint;
    public Vector2 GroundCheckSize;
    public float GroundCheckDistance;

    public Collider2D WallSlideColliderRight;

    public Collider2D WallSlideColliderLeft;

    [SerializeField]
    private LayerMask _walkableLayers;

    public LayerMask WalkableLayers { get { return _walkableLayers; } set { _walkableLayers = value; } }

    public LayerMask WallSlideLayers;

    public Vector2 MoveInput { get; private set; }

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

    public float LastJumpPressTime { get; set; }
    public float LastJumpTime { get; set; }

    public Animator PlayerAnimator{ get; set; }
    public SpriteRenderer PlayerSpriteRenderer {get; set;}


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


    public void Run()
    {
        // Get a vector parallel to the slope the surface the player is standing on and figure out how fast the player wants to run
        // Here we are only concerned with horizontal speed, because vertical speed would also include speed from jumps.
        Vector2 slopeVector = Vector2.right;
        float targetSpeed = Mathf.Abs(MoveInput.x) * _data.getMaxRunSpeed();
        if (IsGrounded)
        {
            slopeVector = -Vector2.Perpendicular(LastSurfaceNormal).normalized;
            targetSpeed *= slopeVector.x;
        }

        // Have different cases for: The player wanting to turn around, The player wanting to move faster, The player wanting to slow down.
        // If the player wants to continue moving in the same direction.
        if (Mathf.Sign(PlayerBody.velocity.x) == Mathf.Sign(MoveInput.x) || Mathf.Abs(PlayerBody.velocity.x) < 0.01f)
        {
            getUpToSpeed(targetSpeed, slopeVector);
        }
        else // If the player wants to turn around and move in the opposite direction.
        {
            turnAround(slopeVector);
        }
    }

    private void getUpToSpeed(float targetSpeed, Vector2 parallelSurfaceVector)
    {
        AnimationCurve accelCurve = Data.getRunAccelerationCurve();
        float speedPercentage = Mathf.Abs(PlayerBody.velocity.x) / _data.getMaxRunSpeed();

        // For now, if the player is already running faster than max speed, do nothing.
        if (speedPercentage > 1 || Mathf.Abs(PlayerBody.velocity.x) > targetSpeed)
        {
            return;
        }
        else
        {
            float targetAccel = accelCurve.Evaluate(speedPercentage) * _data.getMaxRunAcceleration();
            float forceMagnitude = PlayerBody.mass * targetAccel * Mathf.Sign(MoveInput.x);
            PlayerBody.AddForce(forceMagnitude * parallelSurfaceVector);
        }
    }

    private void turnAround(Vector2 parallelSurfaceVector)
    {
        AnimationCurve accelCurve = Data.getTurnAccelerationCurve();
        float speedPercentage = Mathf.Abs(PlayerBody.velocity.x) / _data.getMaxRunSpeed();
        float targetAccel;

        if (speedPercentage < 1) targetAccel = accelCurve.Evaluate(speedPercentage) * _data.getMaxTurnAcceleration();
        else targetAccel = targetAccel = accelCurve.Evaluate(1) * _data.getMaxTurnAcceleration();

        float forceMagnitude = PlayerBody.mass * targetAccel * -Mathf.Sign(PlayerBody.velocity.x);
        PlayerBody.AddForce(forceMagnitude * parallelSurfaceVector);
    }

    public void comeToStop()
    {
        // May want to remove this, don't think I should constant set velocity to 0 in an update loop.
        // Maybe I could make it easier for the player to stay stopped by adding friction to the idle state?
        // (Currently the player has an issue of sliding off slopes while idle)
        if(Mathf.Abs(PlayerBody.velocity.x) < 0.1f)
        {
            PlayerBody.velocity = new Vector2(0, PlayerBody.velocity.y);
            return;
        }

        Vector2 slopeVector = Vector2.right;
        if (IsGrounded)
        {
            slopeVector = -Vector2.Perpendicular(LastSurfaceNormal).normalized;
        }

        AnimationCurve accelCurve = Data.getStopAccelerationCurve();
        float speedPercentage = Mathf.Abs(PlayerBody.velocity.x) / _data.getMaxRunSpeed();
        float targetAccel;

        if (speedPercentage < 1) targetAccel = accelCurve.Evaluate(speedPercentage) * _data.getMaxStopAcceleration();
        else targetAccel = targetAccel = accelCurve.Evaluate(1) * _data.getMaxStopAcceleration();

        float forceMagnitude = PlayerBody.mass * targetAccel * -Mathf.Sign(PlayerBody.velocity.x);
        PlayerBody.AddForce(forceMagnitude * slopeVector);
    }

    public void Jump()
    {
        float forceMagnitude = _data.jumpForce;
        Vector2 force = forceMagnitude * Vector2.up;
        // Cancel out downward forces on jump;
        if (PlayerBody.velocity.y < 0)
        {
            force -= new Vector2(0, PlayerBody.velocity.y);
        }
        PlayerBody.AddForce(force, ForceMode2D.Impulse);
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

    private void Awake()
    {
        PlayerAnimator = this.GetComponentInChildren<Animator>();
        PlayerSpriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        PlayerBody = this.GetComponent<Rigidbody2D>();

        Controls = new GameControls();

        _states = new PlayerStateFactory(this);
        _currentState = _states.Airborne();
        _currentState.EnterState();

        Controls.Player.Move.performed += OnMoveStart;
        Controls.Player.Move.canceled += OnMoveCancel;

        Controls.Player.JumpStart.performed += OnJumpStart;
        Controls.Player.JumpEnd.performed += OnJumpEnd;
        
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
        _currentState.CallUpdateStates();        
    }

    private void FixedUpdate()
    {
        _currentState.CallFixedUpdateStates();
    }

    private void OnDrawGizmosSelected()
    {
        
    }

    public void FlipSprite(){
        SpriteRenderer spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }
}
