using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerData data;

    public Rigidbody2D PlayerBody { get; private set; }

    public float LastOnGroundTime { get; private set; }

    public bool IsGrounded { get; private set; }

    public bool IsJumping { get; private set; }
    public float LastPressedJumpTime { get; private set; }

    private int groundLayer;

    private void Awake()
    {
        PlayerBody = this.GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.NameToLayer("Ground");
    }
    // Start is called before the first frame update
    void Start()
    {
        InputHandler.instance.OnJumpPressed += args => OnJumpStart(args);

        InputHandler.instance.OnJumpReleased += args => OnJumpEnd(args);

        SetGravityScale(data.gravityScale);
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerBody.velocity.y >= 0)
        {
            SetGravityScale(data.gravityScale);
        }
        else if (InputHandler.instance.MoveInput.y < 0)
        {
            SetGravityScale(data.gravityScale * data.quickFallGravityMult);
        }
        else
        {
            SetGravityScale(data.gravityScale * data.fallGravityMult);
        }

        if(CanJump() && Time.timeSinceLevelLoad - LastPressedJumpTime <= data.jumpBufferTime)
        {
            IsJumping = true;
            Jump();
        }
    }

    private bool CanJump()
    {
        return Time.timeSinceLevelLoad - LastOnGroundTime <= data.coyoteTime && !IsJumping;
    }

    private void FixedUpdate()
    {
        if(!IsGrounded)
        {
            Drag(data.airDrag);
        }
        else
        {
            Drag(data.groundFriction);
        }

        Run(1);

    }

    public void OnJumpStart(InputHandler.InputArgs args)
    {
        Debug.Log("JumpStarted");
        LastPressedJumpTime = Time.timeSinceLevelLoad;
    }

    private Coroutine delayedCut;
    public void OnJumpEnd(InputHandler.InputArgs args)
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
        float targetSpeed = InputHandler.instance.MoveInput.x * data.runMaxSpeed;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == groundLayer)
        {
            IsGrounded = true;
            LastOnGroundTime = Time.timeSinceLevelLoad;
            IsJumping = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == groundLayer)
        {
            LastOnGroundTime = Time.timeSinceLevelLoad;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == groundLayer)
        {
            IsGrounded = false;
            LastOnGroundTime = Time.timeSinceLevelLoad;
        }
    }
}
