using UnityEngine;

public class Seeker_DashState : EnemyState
{
    private Enemy_Boss_Seeker seeker;
    private int dashDir = 1;

    public Seeker_DashState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        seeker = enemy as Enemy_Boss_Seeker;
    }

    public void SetDashDirection(int dir)
    {
        dashDir = dir != 0 ? dir : 1;
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = seeker.DashDuration;
        seeker.RecordDashTime();

        enemy.HandleFlip(dashDir);
        enemy.SetVelocity(dashDir * seeker.DashSpeed, 0f);
    }

    public override void Update()
    {
        base.Update();

        // Giữ vận tốc lướt chớp nhoáng
        enemy.SetVelocity(dashDir * seeker.DashSpeed, 0f);

        if (stateTimer <= 0f)
        {
            // Nếu sau cú dash mà đang ở cự ly gần Player và có thể tấn công -> nối combo chém luôn!
            Transform player = seeker.GetPlayerTarget();
            if (player != null && Vector2.Distance(enemy.transform.position, player.position) <= seeker.AttackRange && seeker.CanAttack())
            {
                stateMachine.ChangeState(seeker.seekerAttackState);
            }
            else
            {
                stateMachine.ChangeState(seeker.seekerBattleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetVelocity(0f, rb.linearVelocity.y);
    }
}
