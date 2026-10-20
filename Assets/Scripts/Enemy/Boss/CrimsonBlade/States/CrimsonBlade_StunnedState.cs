using UnityEngine;

public class CrimsonBlade_StunnedState : Enemy_StunnedState
{
    private Enemy_Boss_CrimsonBlade crimsonBlade;
    private float effectiveStunDuration = 0.6f;
    private float timer = 0f;

    public CrimsonBlade_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        crimsonBlade = enemy as Enemy_Boss_CrimsonBlade;
    }

    public override void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
        timer = 0f;

        effectiveStunDuration = enemy.stunnedDuration > 0 ? enemy.stunnedDuration : 0.6f;

        enemy.SetVelocity(0f, 0f);
        enemy.EnableCounterWindow(false);
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

        enemy.SetVelocity(0f, enemy.rb.linearVelocity.y);
        timer += Time.deltaTime;

        if ((triggerCalled && timer >= 0.3f) || timer >= effectiveStunDuration)
        {
            stateMachine.ChangeState(crimsonBlade.crimsonBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.stunnedDuration = 0.6f;
    }
}
