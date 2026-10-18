using UnityEngine;

public class Player_WallSlideState : PlayerState
{
    public Player_WallSlideState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();
        HandleWallSlide();

        if (input.Player.Jump.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.wallJumpState);
            return;
        }

        if (player.wallDetected == false)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        if (player.moveInput.x != 0 && (int)player.moveInput.x != player.facingDir)
        {
            player.Flip();
            stateMachine.ChangeState(player.fallState);
            return;
        }

        if (player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);

            if (player.moveInput.x != 0 && player.facingDir != (int)player.moveInput.x)
                player.Flip();
        }
    }

    private void HandleWallSlide()
    {
        float maxSlideSpeed = player.jumpForce * player.wallSlideSlowMultiplier;
        float yVelocity = rb.linearVelocity.y;

        if (player.moveInput.y < 0)
        {
            yVelocity = Mathf.Min(yVelocity, -maxSlideSpeed);
        }
        else
        {
            if (yVelocity < -maxSlideSpeed)
                yVelocity = -maxSlideSpeed;
        }

        player.SetVelocity(0, yVelocity);
    }
}
