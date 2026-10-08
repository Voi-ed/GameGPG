using UnityEngine;

public class WanderingKnight_HealState : EnemyState
{
    private Enemy_WanderingKnight wanderingKnight;
    private bool hasHealed;

    public WanderingKnight_HealState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        wanderingKnight = enemy as Enemy_WanderingKnight;
    }

    public override void Enter()
    {
        base.Enter();

        hasHealed = false;
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(0, rb.linearVelocity.y);

        if (triggerCalled)
        {
            stateMachine.ChangeState(wanderingKnight.wkBattleState);
        }
    }

    public void OnHealTrigger()
    {
        if (hasHealed)
            return;

        hasHealed = true;
        wanderingKnight.ConsumeHealCharge();
        wanderingKnight.sfx?.PlayAttackHit();
    }

    public void InterruptHeal()
    {
        // Cancel heal and force into Stunned state if hit while drinking
        stateMachine.ChangeState(wanderingKnight.stunnedState);
    }

    public override void Exit()
    {
        base.Exit();

        wanderingKnight.SetHealOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }
}
