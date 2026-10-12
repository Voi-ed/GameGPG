using UnityEngine;

public class ArchDemon_GlideTeleportState : EnemyState
{
    private Enemy_ArchDemon archDemon;
    private const float glideDuration = 0.35f;
    private int glideDirection;

    public ArchDemon_GlideTeleportState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        archDemon = enemy as Enemy_ArchDemon;
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = glideDuration;

        Transform player = archDemon != null ? archDemon.GetPlayerTarget() : null;
        if (player != null)
        {
            // Lướt ngược hướng vị trí người chơi để tạo cự ly an toàn
            int dirToPlayer = player.position.x > enemy.transform.position.x ? 1 : -1;
            glideDirection = -dirToPlayer;
            enemy.HandleFlip(dirToPlayer);
        }
        else
        {
            glideDirection = -enemy.facingDir;
        }
    }

    public override void Update()
    {
        base.Update();

        stateTimer -= Time.deltaTime;
        enemy.SetVelocity(archDemon.battleMoveSpeed * 2.2f * glideDirection, 0f);

        bool wallBehind = Physics2D.Raycast(enemy.transform.position, Vector2.right * glideDirection, 0.8f, enemy.whatIsGround);

        if (wallBehind || stateTimer <= 0f)
        {
            enemy.SetVelocity(0f, 0f);
            archDemon?.SetTeleportOnCooldown();
            stateMachine.ChangeState(archDemon.archDemonBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        enemy.SetVelocity(0f, 0f);
        archDemon?.SetTeleportOnCooldown();
    }
}
