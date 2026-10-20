using UnityEngine;

public class Seeker_StunnedState : Enemy_StunnedState
{
    private Enemy_Boss_Seeker seeker;
    private float effectiveStunDuration = 0.5f;
    private float timer = 0f;

    public Seeker_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        seeker = enemy as Enemy_Boss_Seeker;
    }

    public override void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
        timer = 0f;

        effectiveStunDuration = enemy.stunnedDuration > 0 ? enemy.stunnedDuration : 0.5f;

        enemy.SetVelocity(0f, 0f);
        enemy.EnableCounterWindow(false);
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

        enemy.SetVelocity(0f, enemy.rb.linearVelocity.y);
        timer += Time.deltaTime;

        if ((triggerCalled && timer >= 0.35f) || timer >= effectiveStunDuration)
        {
            stateMachine.ChangeState(seeker.seekerBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.stunnedDuration = 0.5f;
    }
}
