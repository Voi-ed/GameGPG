using UnityEngine;

public class WanderingKnight_ThrustAttackState : EnemyState
{
    private Enemy_WanderingKnight wanderingKnight;

    public WanderingKnight_ThrustAttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        wanderingKnight = enemy as Enemy_WanderingKnight;
    }

    public override void Enter()
    {
        base.Enter();
        SyncAttackSpeed();

        // High velocity forward thrust
        enemy.SetVelocity(wanderingKnight.thrustSpeed * enemy.facingDir, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // Rapid deceleration during thrust
        enemy.SetVelocity(Mathf.MoveTowards(rb.linearVelocity.x, 0, 16f * Time.deltaTime), rb.linearVelocity.y);

        if (triggerCalled)
        {
            stateMachine.ChangeState(wanderingKnight.wkBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        wanderingKnight.SetThrustOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }

    public void OnThrustHit()
    {
        wanderingKnight.PerformAttackWithScale(wanderingKnight.thrustAttackScale);
        wanderingKnight.sfx?.PlayAttackHit();
    }
}
