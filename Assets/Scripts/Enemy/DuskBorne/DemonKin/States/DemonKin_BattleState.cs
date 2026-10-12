using UnityEngine;

public class DemonKin_BattleState : Enemy_BattleState
{
    private Enemy_DemonKin demonKin;

    public DemonKin_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        demonKin = enemy as Enemy_DemonKin;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && demonKin != null)
            player = demonKin.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (player == null && demonKin != null)
            player = demonKin.GetPlayerTarget();

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

        if (player == null || demonKin == null)
            return;

        int dirToPlayer = DirectionToPlayer();
        enemy.HandleFlip(dirToPlayer);

        // 1. Kỹ năng Tầm Trung: Bull Rush Húc Càn
        if (distToPlayer >= demonKin.ChargeMinDistance && distToPlayer <= demonKin.ChargeMaxDistance && demonKin.CanCharge())
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
            stateMachine.ChangeState(demonKin.demonKinChargeState);
            return;
        }

        // 2. Kỹ năng Cận Chiến: Bổ Rìu Tàn Bạo (Heavy Axe Slam)
        if (distToPlayer <= demonKin.AttackRange && demonKin.CanAttack())
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
            stateMachine.ChangeState(demonKin.demonKinAttackState);
            return;
        }

        // 3. Tiến thẳng áp sát mục tiêu
        demonKin?.CheckEnrageCondition();

        if (distToPlayer > demonKin.AttackRange)
        {
            anim.SetBool("idle", false);
            anim.SetBool("run", true);
            float speed = demonKin.IsEnraged ? demonKin.battleMoveSpeed * 1.35f : demonKin.battleMoveSpeed;
            enemy.SetVelocity(speed * dirToPlayer, rb.linearVelocity.y);
        }
        else
        {
            anim.SetBool("run", false);
            anim.SetBool("idle", true);
            enemy.SetVelocity(0f, rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("run", false);
        anim.SetBool("idle", false);
    }
}
