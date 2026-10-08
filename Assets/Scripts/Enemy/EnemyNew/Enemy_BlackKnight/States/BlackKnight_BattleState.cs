using UnityEngine;

public class BlackKnight_BattleState : Enemy_BattleState
{
    private Enemy_BlackKnight blackKnight;

    public BlackKnight_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        blackKnight = enemy as Enemy_BlackKnight;
    }

    public override void Enter()
    {
        // Avoid base.Enter() ShortRetreat physics hop
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
        UpdateBattleTimer();

        if (player == null)
            player = enemy.GetPlayerReference();

        // If player is crowding right in face on entering battle, roll immediately to reposition!
        if (ShouldRetreat() && blackKnight.CanRoll())
        {
            DecideAndExecuteRoll();
            return;
        }
    }

    public override void Update()
    {
        // Update animation parameters every frame so xVelocity drives the Blend Tree (Run vs Idle)
        UpdateAnimationParameters();

        // Phase 2 check: Enrage below HP threshold
        if (!blackKnight.isEnraged && blackKnight.health.GetHealthPercent() <= blackKnight.enrageHpThreshold)
        {
            stateMachine.ChangeState(blackKnight.bkEnrageState);
            return;
        }

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
        if (playerBehind && dist < 3.5f)
        {
            if (blackKnight.CanHeavyAttack() && Random.value < 0.6f)
            {
                blackKnight.SetHeavyAttackIndex(2); // Spin Cleave 360
                stateMachine.ChangeState(blackKnight.bkHeavyAttackState);
                return;
            }
            else if (blackKnight.CanRoll())
            {
                blackKnight.SetRollDirection(enemy.facingDir); // Roll forward away from flank
                stateMachine.ChangeState(blackKnight.bkRollState);
                return;
            }
            else
            {
                enemy.Flip();
            }
        }

        // Close Combat Range
        if (dist <= enemy.attackDistance)
        {
            // If player is crowding too close into minimum retreat distance, roll to reposition
            if (dist < enemy.minRetreatDistance && blackKnight.CanRoll())
            {
                DecideAndExecuteRoll();
                return;
            }

            // Parry bait attempt
            if (blackKnight.CanParry() && Random.value < blackKnight.parryChance)
            {
                stateMachine.ChangeState(blackKnight.bkParryState);
                return;
            }

            // Normal combo attack
            if (CanAttack())
            {
                lastTimeAttacked = Time.time;
                stateMachine.ChangeState(blackKnight.bkComboAttackState);
                return;
            }
        }
        // Mid Range (Gap Closing & Specials)
        else if (dist > enemy.attackDistance && dist <= blackKnight.heavyEngagementDistance)
        {
            // Ultimate: 5 Fire Orbs barrage if off cooldown
            if (blackKnight.CanWeaponArt())
            {
                stateMachine.ChangeState(blackKnight.bkWeaponArtState);
                return;
            }

            // Heavy attack gap-closer (Thrust or Leap Slam)
            if (blackKnight.CanHeavyAttack())
            {
                int heavyChoice = Random.value < 0.5f ? 1 : 3;
                blackKnight.SetHeavyAttackIndex(heavyChoice);
                stateMachine.ChangeState(blackKnight.bkHeavyAttackState);
                return;
            }
        }
        // Long Range Boss Punish (> 5.5m)
        else if (dist > blackKnight.heavyEngagementDistance)
        {
            // Ultimate: 5 Fire Orbs barrage to snipe from distance
            if (blackKnight.CanWeaponArt())
            {
                stateMachine.ChangeState(blackKnight.bkWeaponArtState);
                return;
            }

            // Hellfire Leap Slam to punish players camping at long range!
            if (blackKnight.CanHeavyAttack())
            {
                blackKnight.SetHeavyAttackIndex(3); // Leap Slam with Hellfire eruption
                stateMachine.ChangeState(blackKnight.bkHeavyAttackState);
                return;
            }
        }

        // Chase / Reposition
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
        // Check if there is a wall or obstacle behind within 2.5m
        bool wallBehind = Physics2D.Raycast(enemy.transform.position, Vector2.right * -enemy.facingDir, 2.5f, enemy.whatIsGround);

        if (wallBehind)
        {
            // Backed against a wall: Roll forward THROUGH/past the player to break out of the corner!
            blackKnight.SetRollDirection(enemy.facingDir);
        }
        else
        {
            // Open ground behind: Roll backward to reset spacing
            blackKnight.SetRollDirection(-enemy.facingDir);
        }

        stateMachine.ChangeState(blackKnight.bkRollState);
    }
}
