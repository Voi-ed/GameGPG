using UnityEngine;

public class Druid_RetreatState : EnemyState
{
    private Enemy_Druid druid;
    private const float retreatDuration = 0.65f;

    public Druid_RetreatState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        druid = enemy as Enemy_Druid;
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = retreatDuration;
    }

    public override void Update()
    {
        base.Update();

        stateTimer -= Time.deltaTime;

        Transform player = druid != null ? druid.GetPlayerTarget() : null;
        if (player != null)
        {
            int dirToPlayer = player.position.x > enemy.transform.position.x ? 1 : -1;
            // Quay mặt về phía người chơi nhưng lùi về phía sau
            enemy.HandleFlip(dirToPlayer);
            enemy.SetVelocity(druid.battleMoveSpeed * -dirToPlayer, rb.linearVelocity.y);
        }

        bool wallBehind = Physics2D.Raycast(enemy.transform.position, Vector2.right * -enemy.facingDir, 0.8f, enemy.whatIsGround);

        if (wallBehind || stateTimer <= 0f)
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
            stateMachine.ChangeState(druid.druidBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetVelocity(0f, rb.linearVelocity.y);
    }
}
