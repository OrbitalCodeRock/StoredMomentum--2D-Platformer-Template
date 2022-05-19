using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Bug where player can jump in mid-air after getting flung by stored momentum (only during coyote time window)

    public GameControls controls;

    [SerializeField]
    private Camera mainCamera;

    private ObjectSelector objectSelector;

    public GameObject MomentumUI;
    private GameObject storedIndicator;

    [SerializeField]
    private PlayerData data;

    private Rigidbody2D targetBody;

    public Rigidbody2D PlayerBody { get; private set; }

    public Transform groundCheckPoint;
    public float groundCheckRadius;
    [SerializeField]
    private LayerMask walkableLayers;

    public enum ManipulationState
    {
        Store,
        Release
    }

    public ManipulationState momentumManipulation;

    public bool isSlowingTime { get; private set; }

    public Vector2 MoveInput { get; private set; }

    public Vector2 StoredVelocity { get; private set; }
    public float StoredMass { get; private set; }

    public float LastOnGroundTime { get; private set; }

    public bool IsGrounded { get; private set; }

    public bool IsJumping { get; private set; } = true;

    public bool conserveMomentum { get; private set; }
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
                targetBody = PlayerBody;
                StoredMass = PlayerBody.mass;
                break;
            case ManipulationState.Release:
                break;
        }
    }

    private void OnTimeSlow(InputAction.CallbackContext args)
    {
        Time.timeScale = 0.5f;
        isSlowingTime = true;
        MomentumUI.SetActive(true);
        objectSelector.enabled = true;
        objectSelector.mouseClick.performed += OnClick;
    }

    private void OnTimeRestore(InputAction.CallbackContext args)
    {
        objectSelector.mouseClick.performed -= OnClick;
        objectSelector.Deselect();
        objectSelector.enabled = false;
        switch (momentumManipulation)
        {
            case ManipulationState.Store:
                StoredVelocity = targetBody.velocity;
                StoredMass = targetBody.mass;
                momentumManipulation = ManipulationState.Release;
                LastMomentumStoreTime = Time.timeSinceLevelLoad;
                break;
            case ManipulationState.Release:
                conserveMomentum = true;
                ReleaseMomentum();
                momentumManipulation = ManipulationState.Store;
                LastMomentumReleaseTime = Time.timeSinceLevelLoad;
                break;
        }
        MomentumUI.SetActive(false);
        Time.timeScale = 1f;
        isSlowingTime = false;
    }

    private void ReleaseMomentum()
    {
        float forceMagnitude = StoredVelocity.magnitude * StoredMass;
        Vector2 direction = ((Vector2)mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - targetBody.position).normalized;
        targetBody.AddForce(forceMagnitude * direction, ForceMode2D.Impulse);
    }

    

    private void OnClick(InputAction.CallbackContext args)
    {
        Rigidbody2D newBody = objectSelector.SelectedObject?.GetComponent<Rigidbody2D>();
        switch (momentumManipulation)
        {
            case ManipulationState.Store:
                if(newBody != null)
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
        if(newBody != null)
        {
            targetBody = newBody;
        }

    }

    private void Awake()
    {
        objectSelector = GameObject.Find("CameraCanvas").GetComponent<ObjectSelector>();

        controls = new GameControls();
        
        controls.Player.Move.performed += OnMoveStart;
        controls.Player.Move.canceled += OnMoveCancel;

        controls.Player.JumpStart.performed += OnJumpStart;
        controls.Player.JumpEnd.performed += OnJumpEnd;

        controls.Player.MomentumManipulate.performed += OnMomentumManipulate;
        controls.Player.TimeSlow.performed += OnTimeSlow;
        controls.Player.TimeRestore.performed += OnTimeRestore;

        PlayerBody = this.GetComponent<Rigidbody2D>();
        targetBody = PlayerBody;
    }
    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetGravityScale(data.gravityScale);
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerBody.velocity.y >= 0)
        {
            SetGravityScale(data.gravityScale);
        }
        else if (MoveInput.y < 0)
        {
            SetGravityScale(data.gravityScale * data.quickFallGravityMult);
        }
        else
        {
            SetGravityScale(data.gravityScale * data.fallGravityMult);
        }
    }

    private bool CanJump()
    {
        return Time.timeSinceLevelLoad - LastOnGroundTime <= data.coyoteTime && !IsJumping;
    }

    private void FixedUpdate()
    {
        if(Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, walkableLayers))
        {
            LastOnGroundTime = Time.timeSinceLevelLoad;
            IsGrounded = true;
            IsJumping = false;
            Drag(data.groundFriction);
            
        }
        else
        {
            IsGrounded = false;
            Drag(data.airDrag);
        }
        if (CanJump() && Time.timeSinceLevelLoad - LastPressedJumpTime <= data.jumpBufferTime)
        {
            IsJumping = true;
            Jump();
        }

        if (!conserveMomentum)
        {
            Run(1);
        }
        else if(Mathf.Abs(MoveInput.x) >= 0.01f && Mathf.Sign(MoveInput.x) != Mathf.Sign(PlayerBody.velocity.x))
        {
            Run(0.25f);   
        }
        if (Mathf.Abs(PlayerBody.velocity.x) < data.runMaxSpeed)
        {
            conserveMomentum = false;
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }


    IEnumerator delayedJumpCut()
    {
        yield return new WaitForSeconds(data.jumpBufferTime - (Time.timeSinceLevelLoad - LastPressedJumpTime));
        if (CanJumpCut())
        {
            JumpCut();
        }
    }

    public void SetGravityScale(float scale)
    {
        PlayerBody.gravityScale = scale;
    }

    private void Drag(float amount)
    {
        Vector2 force = amount * PlayerBody.velocity.normalized;
        force.x = Mathf.Min(Mathf.Abs(PlayerBody.velocity.x), Mathf.Abs(force.x));
        force.y = Mathf.Min(Mathf.Abs(PlayerBody.velocity.y), Mathf.Abs(force.y));
        force.x *= Mathf.Sign(PlayerBody.velocity.x); //finds direction to apply force
        force.y *= Mathf.Sign(PlayerBody.velocity.y);

        PlayerBody.AddForce(-force, ForceMode2D.Impulse);
    }

    private void Run(float lerpAmount)
    {
        float targetSpeed = MoveInput.x * data.runMaxSpeed;
        float speedDif = targetSpeed - PlayerBody.velocity.x;

        float accelRate;

        if (data.doKeepRunMomentum && ((PlayerBody.velocity.x > targetSpeed && targetSpeed > 0.01f) || (PlayerBody.velocity.x < targetSpeed && targetSpeed < -0.01f)))
        {
            accelRate = 0;
        }
        else
        {
            if (IsGrounded)
            {
                // If the target speed is greater than a minimum (0.01), use running acceleration. Otherwise, decelerate.
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.runAccel : data.runDeccel;
            }
            else
            {
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? data.runAccel * data.accelInAir : data.runDeccel * data.deccelInAir;
            }
        }

        float velPower;
        if(Mathf.Abs(targetSpeed) < 0.01f)
        {
            velPower = data.stopPower;
        }
        else if(Mathf.Abs(PlayerBody.velocity.x) > 0 && (Mathf.Sign(targetSpeed) != Mathf.Sign(PlayerBody.velocity.x)))
        {
            velPower = data.turnPower;
        }
        else
        {
            velPower = data.accelPower;
        }

        // applies acceleration to speed difference, then is raised to a set power so the acceleration increases with higher speeds, finally multiplies by sign to preserve direction
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        movement = Mathf.Lerp(PlayerBody.velocity.x, movement, lerpAmount); // lerp so that we can prevent the Run from immediately slowing the player down, in some situations eg wall jump, dash 

        // Possibly change this to account for sloped movement
        PlayerBody.AddForce(movement * Vector2.right); // applies force force to rigidbody, multiplying by Vector2.right so that it only affects X axis 

    }

    private void Jump()
    {
        float force = data.jumpForce;
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
        PlayerBody.AddForce(Vector2.down * PlayerBody.velocity.y * (1 - data.jumpCutMultiplier), ForceMode2D.Impulse);
    }

    private bool CanJumpCut()
    {
        return IsJumping && PlayerBody.velocity.y > 0;
    }    
}

