using UnityEngine;

public class WanderingKnight_PrayState : EnemyState
{
    private Enemy_WanderingKnight wanderingKnight;

    public WanderingKnight_PrayState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        wanderingKnight = enemy as Enemy_WanderingKnight;
    }

    public override void Enter()
    {
        base.Enter();

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

    public void OnPrayBuffTrigger()
    {
        wanderingKnight.ApplyPrayBuff();
    }

    public override void Exit()
    {
        base.Exit();

        wanderingKnight.SetPrayOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }
}
