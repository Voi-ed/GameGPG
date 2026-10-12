using UnityEngine;

public class SeveredFang_BattleState : Enemy_BattleState
{
    private Enemy_SeveredFang severedFang;

    public SeveredFang_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        severedFang = enemy as Enemy_SeveredFang;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && severedFang != null)
            player = severedFang.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (player == null && severedFang != null)
            player = severedFang.GetPlayerTarget();

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

        if (player == null || severedFang == null)
            return;

        int dirToPlayer = DirectionToPlayer();
        enemy.HandleFlip(dirToPlayer);

        // 1. Kỹ năng Tầm Trung/Xa: Thế Tra Kiếm Iaido Quick-Draw
        if (distToPlayer >= severedFang.IaidoMinDistance && distToPlayer <= severedFang.IaidoMaxDistance && severedFang.CanIaido())
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
            stateMachine.ChangeState(severedFang.severedFangIaidoState);
            return;
        }

        // 2. Kỹ năng Cận Chiến: Combo Chém Kiếm (Basic Attack)
        if (distToPlayer <= severedFang.AttackRange && severedFang.CanAttack())
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
            stateMachine.ChangeState(severedFang.severedFangAttackState);
            return;
        }

        // 3. Tiếp cận mục tiêu nếu chưa vào tầm
        if (distToPlayer > severedFang.AttackRange)
        {
            enemy.SetVelocity(severedFang.battleMoveSpeed * dirToPlayer, rb.linearVelocity.y);
            anim.SetBool("idle", false);
            anim.SetBool("run", true);
        }
        else
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
            anim.SetBool("run", false);
            anim.SetBool("idle", true);
        }
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("run", false);
        anim.SetBool("idle", false);
    }
}

