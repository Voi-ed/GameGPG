using UnityEngine;

public class MartialHero_SwordWaveState : EnemyState
{
    private Enemy_MartialHero martialHero;
    private bool hasCastWave;
    private float castDelay = 0.28f;
    private float stateDuration = 0.55f;

    public MartialHero_SwordWaveState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        martialHero = enemy as Enemy_MartialHero;
    }

    public override void Enter()
    {
        base.Enter();

        hasCastWave = false;
        stateTimer = stateDuration;
        enemy.SetVelocity(0, rb.linearVelocity.y);

        // Turn to face player
        Transform target = martialHero.GetPlayerTarget();
        if (target != null)
        {
            int dirToPlayer = target.position.x > enemy.transform.position.x ? 1 : -1;
            if (dirToPlayer != 0 && dirToPlayer != enemy.facingDir)
            {
                enemy.Flip();
            }
        }
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(0, rb.linearVelocity.y);

        // Cast sword wave after short wind-up
        if (!hasCastWave && (stateDuration - stateTimer) >= castDelay)
        {
            hasCastWave = true;
            martialHero.CastSwordWave();
        }

        if (triggerCalled || stateTimer <= 0)
        {
            if (martialHero != null && martialHero.ShouldRetreatFromPlayer())
                stateMachine.ChangeState(martialHero.martialHeroRetreatState);
            else
                stateMachine.ChangeState(martialHero.martialHeroBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        martialHero.SetSwordWaveOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }
}
