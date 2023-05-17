using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {

    }
    
    public override void UpdateState() 
    {
        if (CheckSwitchStates()) return;
    }

    public override void FixedUpdateState()
    {
        if(Ctx.MoveInput.x > 0){
            Ctx.PlayerSpriteRenderer.flipX = false;
        }
        else if(Ctx.MoveInput.x < 0){
            Ctx.PlayerSpriteRenderer.flipX = true;
        }
        Ctx.Run();
    }
    public bool CheckSwitchStates()
    {
        if (Mathf.Abs(Ctx.MoveInput.x) <= 0.01f)
        {
            SwitchState(Factory.Idle());

            // Play Idle Animation
            //if(Ctx.PlayerAnimator != null && !Ctx.IsJumping)Ctx.PlayerAnimator.SetInteger("AnimationState", 0);
            if(Ctx.PlayerAnimator != null && !Ctx.IsJumping)Ctx.StartCoroutine(IdleDelay(0.1f));
            return true;
        }
        return false;
    }
    IEnumerator IdleDelay(float timeDelay){
        yield return new WaitForSeconds(timeDelay);
        if(Mathf.Abs(Ctx.MoveInput.x) <= 0.01f){
            Ctx.PlayerAnimator.SetInteger("AnimationState", 0);
        }
    }
}
