using UnityEngine;

public class WanderingKnight_RollState : EnemyState
{
    private Enemy_WanderingKnight wanderingKnight;
    private int rollDirection;

    public WanderingKnight_RollState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        wanderingKnight = enemy as Enemy_WanderingKnight;
    }

    public override void Enter()
    {
        base.Enter();

        wanderingKnight.isRolling = true;
        rollDirection = wanderingKnight.rollDir != 0 ? wanderingKnight.rollDir : -enemy.facingDir;
        stateTimer = wanderingKnight.rollDuration;

        enemy.MakeUntargetable(false);
        enemy.SetVelocity(wanderingKnight.rollSpeed * rollDirection, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(wanderingKnight.rollSpeed * rollDirection, rb.linearVelocity.y);

        if (triggerCalled || stateTimer <= 0)
        {
            OnRollFinished();
        }
    }

    private void OnRollFinished()
    {
        Transform target = enemy.player != null ? enemy.player : enemy.GetPlayerReference();
        if (target != null)
        {
            int dirToPlayer = target.position.x > enemy.transform.position.x ? 1 : -1;
            if (dirToPlayer != 0 && dirToPlayer != enemy.facingDir)
            {
                enemy.Flip();
            }

            float dist = Mathf.Abs(target.position.x - enemy.transform.position.x);

            // Counter right out of roll:
            // 1. Fast thrust if ready
            if (wanderingKnight.CanThrust() && dist <= wanderingKnight.thrustDistance && Random.value < 0.5f)
            {
                stateMachine.ChangeState(wanderingKnight.wkThrustState);
                return;
            }

            // 2. Combo if close
            if (dist <= enemy.attackDistance)
            {
                stateMachine.ChangeState(wanderingKnight.wkComboAttackState);
                return;
            }
        }

        stateMachine.ChangeState(wanderingKnight.wkBattleState);
    }

    public override void Exit()
    {
        base.Exit();

        wanderingKnight.isRolling = false;
        enemy.MakeUntargetable(true);
        wanderingKnight.SetRollOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }
}
