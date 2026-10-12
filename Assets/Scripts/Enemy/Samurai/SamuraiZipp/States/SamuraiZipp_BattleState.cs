using UnityEngine;

public class SamuraiZipp_BattleState : Enemy_BattleState
{
    private Enemy_SamuraiZipp samuraiZipp;

    public SamuraiZipp_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        samuraiZipp = enemy as Enemy_SamuraiZipp;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && samuraiZipp != null)
            player = samuraiZipp.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (player == null && samuraiZipp != null)
            player = samuraiZipp.GetPlayerTarget();

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

        if (player == null || samuraiZipp == null)
            return;

        enemy.HandleFlip(DirectionToPlayer());

        // 1. Tầm trung: Kỹ năng lướt đâm Xuyên Tâm Kích
        if (distToPlayer >= samuraiZipp.ThrustMinDistance && distToPlayer <= samuraiZipp.ThrustMaxDistance && samuraiZipp.CanThrust())
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            stateMachine.ChangeState(samuraiZipp.samuraiThrustState);
            return;
        }

        // 2. Tầm gần: Chém Đoạt Mệnh Trảm (Attack 1)
        if (distToPlayer <= samuraiZipp.Attack1Distance && samuraiZipp.CanAttack())
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            stateMachine.ChangeState(samuraiZipp.samuraiAttack1State);
            return;
        }

        // 3. Trong tầm gần nhưng đang hồi chiêu: Đứng thủ thế (Idle Spacing)
        if (distToPlayer <= samuraiZipp.Attack1Distance)
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
