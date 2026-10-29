using UnityEngine;

public class ArchDemon_BattleState : Enemy_BattleState
{
    private Enemy_ArchDemon archDemon;

    public ArchDemon_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        archDemon = enemy as Enemy_ArchDemon;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && archDemon != null)
            player = archDemon.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (player == null && archDemon != null)
            player = archDemon.GetPlayerTarget();

        float distToPlayer = DistanceToPlayer();

        if (enemy.PlayerDetected())
        {
            UpdateTargetIfNeeded();
            UpdateBattleTimer();
        }
        else if (player != null && distToPlayer <= 12f)
        {
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        if (player == null || archDemon == null)
            return;

        int dirToPlayer = DirectionToPlayer();
        enemy.HandleFlip(dirToPlayer);

        // 1. Nếu người chơi áp sát quá gần: Lướt hư không/dịch chuyển né tránh
        if (distToPlayer <= archDemon.CloseDangerDistance && archDemon.CanTeleport())
        {
            enemy.SetVelocity(0f, 0f);
            stateMachine.ChangeState(archDemon.archDemonGlideTeleportState);
            return;
        }

        // 2. Tấn công phun hắc hỏa khi đã vào cự ly hiệu quả (<= 4.2m)
        if (distToPlayer <= 4.2f && archDemon.CanSpellcast())
        {
            enemy.SetVelocity(0f, 0f);
            stateMachine.ChangeState(archDemon.archDemonSpellcastState);
            return;
        }

        // 3. Tiếp cận người chơi vào cự ly tấn công (bay tới khi cách ~ 3.5m)
        if (distToPlayer > 3.5f)
        {
            enemy.SetVelocity(archDemon.battleMoveSpeed * dirToPlayer, 0f);
        }
        else
        {
            enemy.SetVelocity(0f, 0f);
        }
    }
}
