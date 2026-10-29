using UnityEngine;

public class Satyr_BattleState : Enemy_BattleState
{
    private Enemy_Satyr satyr;

    public Satyr_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        satyr = enemy as Enemy_Satyr;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && satyr != null)
            player = satyr.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

        if (enemy.PlayerDetected())
            UpdateTargetIfNeeded();

        if (player == null)
        {
            player = satyr.GetPlayerTarget();
            if (player == null)
            {
                stateMachine.ChangeState(enemy.idleState);
                return;
            }
        }

        float distance = Vector2.Distance(enemy.transform.position, player.position);
        int dirToPlayer = DirectionToPlayer();
        enemy.HandleFlip(dirToPlayer);

        // Priority 1: Ultimate Spirit Bomb (Phase 2, mid-to-far range)
        if (satyr.CanSpiritBomb() && distance >= 3.5f)
        {
            stateMachine.ChangeState(satyr.satyrSpiritBombState);
            return;
        }

        // Priority 2: Demonic Maw Fire Breath
        // Roots completely still to cast continuous, uninterrupted flame when in range
        if (satyr.CanFireBreath())
        {
            if (distance <= 5.5f)
            {
                enemy.SetVelocity(0f, 0f);
                stateMachine.ChangeState(satyr.satyrFireBreathState);
                return;
            }
            else if (satyr.CanDash())
            {
                // Dash rush to instantly get into flame breath range
                satyr.satyrDashState.SetDashDirection(dirToPlayer);
                stateMachine.ChangeState(satyr.satyrDashState);
                return;
            }
        }

        // Priority 3: Continuous Dashing (Satyr specializes in non-stop phantom dashes)
        // Dashes continuously around and through the player between fire breaths
        if (satyr.CanDash())
        {
            satyr.satyrDashState.SetDashDirection(dirToPlayer);
            stateMachine.ChangeState(satyr.satyrDashState);
            return;
        }

        // Nghỉ sau combo dash: đi chậm, tạo cửa sổ phản công cho người chơi
        if (satyr.IsResting)
        {
            float restSpeed = distance > 3.0f ? satyr.BattleMoveSpeed * dirToPlayer : 0f;
            enemy.SetVelocity(restSpeed, enemy.rb.linearVelocity.y);
            return;
        }

        // Brief recovery window between rapid dashes (0.35s):
        // Keep glide pressure instead of slow sluggish walking
        if (distance > 3.0f)
        {
            enemy.SetVelocity(satyr.DashSpeed * 0.5f * dirToPlayer, enemy.rb.linearVelocity.y);
        }
        else
        {
            enemy.SetVelocity(0f, enemy.rb.linearVelocity.y);
        }
    }
}
