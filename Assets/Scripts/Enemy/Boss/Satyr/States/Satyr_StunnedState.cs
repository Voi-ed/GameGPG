using UnityEngine;

public class Satyr_StunnedState : Enemy_StunnedState
{
    private Enemy_Satyr satyr;
    private float effectiveStunDuration = 0.45f;
    private float timer = 0f;

    public Satyr_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        satyr = enemy as Enemy_Satyr;
    }

    public override void Enter()
    {
        anim.SetBool(animBoolName, true);
        triggerCalled = false;
        timer = 0f;

        // Use duration passed to enemy.stunnedDuration if set, otherwise default to clip duration (0.45s)
        effectiveStunDuration = enemy.stunnedDuration > 0 ? enemy.stunnedDuration : 0.45f;

        // Ensure boss doesn't slide and gravity is restored if interrupted in mid-air
        enemy.SetVelocity(0f, 0f);
        if (enemy.rb != null && enemy.rb.gravityScale == 0f)
            enemy.rb.gravityScale = 3f;

        // Clear counterable window
        enemy.EnableCounterWindow(false);
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

        // Keep horizontal velocity 0 while allowing vertical gravity fall
        enemy.SetVelocity(0f, enemy.rb.linearVelocity.y);
        timer += Time.deltaTime;

        // Exit as soon as animation trigger fires (time >= 0.35s) or effective duration expires
        if ((triggerCalled && timer >= 0.35f) || timer >= effectiveStunDuration)
        {
            stateMachine.ChangeState(satyr.satyrBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.stunnedDuration = 0.45f;
    }
}
