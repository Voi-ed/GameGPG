using UnityEngine;

public class SeveredFang_AttackState : Enemy_AttackState
{
    private Enemy_SeveredFang severedFang;
    private const float fallbackDuration = 1.8f;

    public SeveredFang_AttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        severedFang = enemy as Enemy_SeveredFang;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.SetVelocity(0f, rb.linearVelocity.y);

        Transform player = severedFang != null ? severedFang.GetPlayerTarget() : null;
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
            severedFang?.SetAttackOnCooldown();
            stateMachine.ChangeState(severedFang.severedFangBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        severedFang?.SetAttackOnCooldown();
    }
}
