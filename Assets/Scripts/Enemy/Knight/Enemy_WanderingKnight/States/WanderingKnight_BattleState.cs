using UnityEngine;

public class WanderingKnight_BattleState : Enemy_BattleState
{
    private Enemy_WanderingKnight wanderingKnight;

    public WanderingKnight_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        wanderingKnight = enemy as Enemy_WanderingKnight;
    }

    public override void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
        UpdateBattleTimer();

        if (player == null)
            player = enemy.GetPlayerReference();

        // If player is crowding too close on battle enter, roll to space out
        if (ShouldRetreat() && wanderingKnight.CanRoll())
        {
            DecideAndExecuteRoll();
            return;
        }
    }

    public override void Update()
    {
        UpdateAnimationParameters();

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

        float dist = DistanceToPlayer();
        int dirToPlayer = DirectionToPlayer();
        bool playerBehind = (dirToPlayer != 0 && dirToPlayer != enemy.facingDir);

        // Flanking/Backstab punishment
        if (playerBehind && dist < 3.2f)
        {
            if (wanderingKnight.CanRoll())
            {
                wanderingKnight.SetRollDirection(enemy.facingDir);
                stateMachine.ChangeState(wanderingKnight.wkRollState);
                return;
            }
            else
            {
                enemy.Flip();
            }
        }

        // Survival: Drink Flask if health drops low and safe spacing exists
        float hpPercent = wanderingKnight.health != null ? wanderingKnight.health.GetHealthPercent() : 1f;
        if (hpPercent <= wanderingKnight.healHpThreshold && wanderingKnight.CanHeal() && dist >= 2.5f)
        {
            stateMachine.ChangeState(wanderingKnight.wkHealState);
            return;
        }

        // Buff: Pray for Holy/Battle Aura if at safe distance
        if (wanderingKnight.CanPray() && dist >= 3.0f && Random.value < 0.4f)
        {
            stateMachine.ChangeState(wanderingKnight.wkPrayState);
            return;
        }

        // Close Combat Range
        if (dist <= enemy.attackDistance)
        {
            if (dist < enemy.minRetreatDistance && wanderingKnight.CanRoll())
            {
                DecideAndExecuteRoll();
                return;
            }

            if (CanAttack())
            {
                lastTimeAttacked = Time.time;
                stateMachine.ChangeState(wanderingKnight.wkComboAttackState);
                return;
            }
        }
        // Mid Range: Swift Thrust
        else if (dist > enemy.attackDistance && dist <= wanderingKnight.thrustDistance)
        {
            if (wanderingKnight.CanThrust())
            {
                stateMachine.ChangeState(wanderingKnight.wkThrustState);
                return;
            }
        }
        // Long Range: Air Slash Leap
        else if (dist > wanderingKnight.thrustDistance && dist <= wanderingKnight.airSlashDistance)
        {
            if (wanderingKnight.CanAirSlash())
            {
                stateMachine.ChangeState(wanderingKnight.wkAirSlashState);
                return;
            }
        }

        // Chase Player
        float moveSpeed = enemy.canChasePlayer ? enemy.GetBattleMoveSpeed() : 0.0001f;
        if (enemy.groundDetected == false)
            moveSpeed = 0.00001f;

        enemy.SetVelocity(moveSpeed * dirToPlayer, rb.linearVelocity.y);
    }

    public void ResetAttackCooldown()
    {
        lastTimeAttacked = Time.time;
    }

    public void DecideAndExecuteRoll()
    {
        // Check if there is a wall behind
        bool wallBehind = Physics2D.Raycast(enemy.transform.position, Vector2.right * -enemy.facingDir, 2.5f, enemy.whatIsGround);

        if (wallBehind)
        {
            // Roll forward past player to escape corner
            wanderingKnight.SetRollDirection(enemy.facingDir);
        }
        else
        {
            // Roll backward to reset spacing
            wanderingKnight.SetRollDirection(-enemy.facingDir);
        }

        stateMachine.ChangeState(wanderingKnight.wkRollState);
    }
}
