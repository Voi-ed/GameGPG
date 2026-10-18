using UnityEngine;

public class Player_MoveState : Player_GroundedState
{
    public Player_MoveState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }

    public override void Update()
    {
        base.Update();

        bool movingIntoWall = player.wallDetected && (player.moveInput.x == player.facingDir);

        if (player.moveInput.x == 0 || movingIntoWall)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        player.SetVelocity(player.moveInput.x * player.moveSpeed, rb.linearVelocity.y);
    }
}
