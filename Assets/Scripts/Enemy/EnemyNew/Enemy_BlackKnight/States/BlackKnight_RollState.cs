using UnityEngine;

public class BlackKnight_RollState : EnemyState
{
    private Enemy_BlackKnight blackKnight;
    private int rollDirection;

    public BlackKnight_RollState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        blackKnight = enemy as Enemy_BlackKnight;
    }

    public override void Enter()
    {
        base.Enter();

        blackKnight.isRolling = true;
        rollDirection = blackKnight.rollDir != 0 ? blackKnight.rollDir : -enemy.facingDir;
        stateTimer = blackKnight.rollDuration;

        // Allow passing through targets and avoid collision lock during roll
        enemy.MakeUntargetable(false);

        enemy.SetVelocity(blackKnight.rollSpeed * rollDirection, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // Maintain roll speed throughout dodge duration
        enemy.SetVelocity(blackKnight.rollSpeed * rollDirection, rb.linearVelocity.y);

        if (triggerCalled || stateTimer <= 0)
        {
            OnRollFinished();
        }
    }

    private void OnRollFinished()
    {
        // Re-orient to face the player immediately after rolling
        Transform target = enemy.player != null ? enemy.player : enemy.GetPlayerReference();
        if (target != null)
        {
            int dirToPlayer = target.position.x > enemy.transform.position.x ? 1 : -1;
            if (dirToPlayer != 0 && dirToPlayer != enemy.facingDir)
            {
                enemy.Flip();
            }

            float dist = Mathf.Abs(target.position.x - enemy.transform.position.x);

            // Counter-attack right out of roll:
            // 1. Fast Thrust if Heavy Attack is ready (50% chance)
            if (blackKnight.CanHeavyAttack() && dist <= blackKnight.heavyEngagementDistance && Random.value < 0.5f)
            {
                blackKnight.SetHeavyAttackIndex(1); // Thrust
                stateMachine.ChangeState(blackKnight.bkHeavyAttackState);
                return;
            }

            // 2. Immediate combo slash if in close melee range
            if (dist <= enemy.attackDistance)
            {
                stateMachine.ChangeState(blackKnight.bkComboAttackState);
                return;
            }
        }

        stateMachine.ChangeState(blackKnight.bkBattleState);
    }

    public override void Exit()
    {
        base.Exit();

        blackKnight.isRolling = false;
        enemy.MakeUntargetable(true);
        blackKnight.SetRollOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }
}
