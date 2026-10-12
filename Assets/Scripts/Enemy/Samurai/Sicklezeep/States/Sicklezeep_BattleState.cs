using UnityEngine;

public class Sicklezeep_BattleState : Enemy_BattleState
{
    private Enemy_Sicklezeep sicklezeep;

    public Sicklezeep_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        sicklezeep = enemy as Enemy_Sicklezeep;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && sicklezeep != null)
            player = sicklezeep.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (player == null && sicklezeep != null)
            player = sicklezeep.GetPlayerTarget();

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

        if (player == null || sicklezeep == null)
            return;

        enemy.HandleFlip(DirectionToPlayer());

        // 1. Tầm trung: Kỹ năng trượt quét liêm đao (Sweep Dash)
        if (distToPlayer >= sicklezeep.SweepMinDistance && distToPlayer <= sicklezeep.SweepMaxDistance && sicklezeep.CanSweepDash())
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            stateMachine.ChangeState(sicklezeep.sickleSweepDashState);
            return;
        }

        // 2. Tầm gần: Chém Đoạt Phách Trảm (Attack 1)
        if (distToPlayer <= sicklezeep.Attack1Distance && sicklezeep.CanAttack())
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            stateMachine.ChangeState(sicklezeep.sickleAttack1State);
            return;
        }

        // 3. Trong tầm gần nhưng đang hồi chiêu: Đứng thủ thế (Idle Spacing)
        if (distToPlayer <= sicklezeep.Attack1Distance)
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            anim.SetBool("run", false);
            anim.SetBool("idle", true);
            return;
        }

        // 4. Ngoài tầm đánh: Chạy áp sát Player
        enemy.SetVelocity(enemy.battleMoveSpeed * DirectionToPlayer(), rb.linearVelocity.y);
        anim.SetBool("idle", false);
        anim.SetBool("run", true);
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("run", false);
        anim.SetBool("idle", false);
    }
}
