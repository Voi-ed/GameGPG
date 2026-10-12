using UnityEngine;

public class Druid_CastEarthState : Enemy_AttackState
{
    private Enemy_Druid druid;
    private const float fallbackDuration = 1.6f;

    public Druid_CastEarthState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        druid = enemy as Enemy_Druid;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.SetVelocity(0f, rb.linearVelocity.y);

        Transform player = druid != null ? druid.GetPlayerTarget() : null;
        if (player != null)
        {
            int dir = player.position.x > enemy.transform.position.x ? 1 : -1;
            enemy.HandleFlip(dir);
        }

        stateTimer = fallbackDuration;
    }

    public override void Update()
    {
        base.Update();

        stateTimer -= Time.deltaTime;
        enemy.SetVelocity(0f, rb.linearVelocity.y);

        if (triggerCalled || stateTimer <= 0f)
        {
            druid?.SetCastOnCooldown();
            stateMachine.ChangeState(druid.druidBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        druid?.SetCastOnCooldown();
    }
}
