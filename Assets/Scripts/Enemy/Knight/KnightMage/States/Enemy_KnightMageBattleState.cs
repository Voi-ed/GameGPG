using UnityEngine;

public class Enemy_KnightMageBattleState : Enemy_BattleState
{
    private Enemy_KnightMage knightMage;

    public Enemy_KnightMageBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        knightMage = enemy as Enemy_KnightMage;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && knightMage != null)
            player = knightMage.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

        if (player == null && knightMage != null)
            player = knightMage.GetPlayerTarget();

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

        if (player == null || knightMage == null)
            return;

        // Luôn quay mặt về phía Player
        enemy.HandleFlip(DirectionToPlayer());

        float distToPlayer = DistanceToPlayer();

        // 1. Nếu Player áp sát trong tầm kích hoạt gai (Thorns distance)
        if (distToPlayer <= knightMage.GetThornsTriggerDistance())
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);

            if (knightMage.CanUseThorns())
            {
                knightMage.SetThornsOnCooldown();
                stateMachine.ChangeState(knightMage.knightMageThornsState);
                return;
            }

            if (knightMage.CanTeleport())
            {
                stateMachine.ChangeState(knightMage.knightMageTeleportState);
                return;
            }

            // Nếu đang cooldown cả 2 skill: lùi nhanh né đòn nếu an toàn
            if (!knightMage.CantMoveBackwards())
                enemy.SetVelocity(enemy.GetBattleMoveSpeed() * 1.5f * -DirectionToPlayer(), rb.linearVelocity.y);
            else
                enemy.SetVelocity(0, rb.linearVelocity.y);
            return;
        }

        // 2. Bắn phép từ xa
        if (distToPlayer <= knightMage.DetectionRadius)
        {
            if (knightMage.CanCastSpell())
            {
                enemy.SetVelocity(0, rb.linearVelocity.y);
                stateMachine.ChangeState(knightMage.knightMageSpellCastState);
                return;
            }

            // Duy trì khoảng cách chiến đấu tối ưu
            if (distToPlayer < enemy.minRetreatDistance)
            {
                if (!knightMage.CantMoveBackwards())
                    enemy.SetVelocity(enemy.GetBattleMoveSpeed() * -DirectionToPlayer(), rb.linearVelocity.y);
                else
                    enemy.SetVelocity(0, rb.linearVelocity.y);
            }
            else if (distToPlayer > enemy.attackDistance)
            {
                enemy.SetVelocity(enemy.GetBattleMoveSpeed() * DirectionToPlayer(), rb.linearVelocity.y);
            }
            else
            {
                enemy.SetVelocity(0, rb.linearVelocity.y);
            }
        }
        else
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
        }
    }
}
