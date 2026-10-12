using UnityEngine;

public class KnightCref_BattleState : Enemy_BattleState
{
    private Enemy_KnightCref knightCref;

    public KnightCref_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        knightCref = enemy as Enemy_KnightCref;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && knightCref != null)
            player = knightCref.GetPlayerReference();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (player == null && knightCref != null)
            player = knightCref.GetPlayerReference();

        if (enemy.PlayerDetected())
        {
            UpdateTargetIfNeeded();
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        if (player == null || knightCref == null)
            return;

        enemy.HandleFlip(DirectionToPlayer());

        float distToPlayer = DistanceToPlayer();

        // 1. Khoảng cách tầm trung (3.2m - 6.0m): Dash đột kích áp sát
        if (distToPlayer >= knightCref.DashMinDistance && distToPlayer <= knightCref.DashMaxDistance && knightCref.CanDash())
        {
            knightCref.SetDashOnCooldown();
            stateMachine.ChangeState(knightCref.crefDashState);
            return;
        }

        // 2. Khoảng cách cận chiến
        if (WithinAttackRange() && CanAttack())
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);

            // Ưu tiên Heavy Slam nếu hồi chiêu xong
            if (knightCref.CanHeavySlam())
            {
                knightCref.SetHeavySlamOnCooldown();
                lastTimeAttacked = Time.time;
                stateMachine.ChangeState(knightCref.crefHeavySlamState);
                return;
            }

            // Đòn chém thường
            lastTimeAttacked = Time.time;
            stateMachine.ChangeState(knightCref.crefAttackState);
            return;
        }

        // 3. StalkWalk: Bước chậm rãi chắc nịch về phía Player
        float moveSpeed = enemy.canChasePlayer ? enemy.GetBattleMoveSpeed() : 0.0001f;
        enemy.SetVelocity(moveSpeed * DirectionToPlayer(), rb.linearVelocity.y);
    }
}
