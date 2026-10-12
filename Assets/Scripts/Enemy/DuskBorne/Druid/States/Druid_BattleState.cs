using UnityEngine;

public class Druid_BattleState : Enemy_BattleState
{
    private Enemy_Druid druid;

    public Druid_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        druid = enemy as Enemy_Druid;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && druid != null)
            player = druid.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (player == null && druid != null)
            player = druid.GetPlayerTarget();

        float distToPlayer = DistanceToPlayer();

        if (player != null && distToPlayer <= 10f)
        {
            UpdateBattleTimer();
        }
        else if (enemy.PlayerDetected())
        {
            UpdateTargetIfNeeded();
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        if (player == null || druid == null)
            return;

        int dirToPlayer = DirectionToPlayer();
        enemy.HandleFlip(dirToPlayer);

        // 1. Quá gần người chơi: Đi lùi giữ cự ly an toàn
        if (distToPlayer <= druid.RetreatDistance)
        {
            stateMachine.ChangeState(druid.druidRetreatState);
            return;
        }

        // 2. Trong tầm niệm chú & hết hồi chiêu: Gõ trượng gọi gai đất
        if (distToPlayer >= druid.MinCastDistance && distToPlayer <= druid.MaxCastDistance && druid.CanCast())
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
            stateMachine.ChangeState(druid.druidCastEarthState);
            return;
        }

        // 3. Nếu ở quá xa: Đi lại gần để vào tầm niệm phép
        if (distToPlayer > druid.MaxCastDistance)
        {
            enemy.anim.SetBool("run", true);
            enemy.anim.SetBool("idle", false);
            enemy.SetVelocity(druid.battleMoveSpeed * dirToPlayer, rb.linearVelocity.y);
        }
        else
        {
            enemy.anim.SetBool("run", false);
            enemy.anim.SetBool("idle", true);
            enemy.SetVelocity(0f, rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.anim.SetBool("run", false);
        enemy.anim.SetBool("idle", false);
    }
}
