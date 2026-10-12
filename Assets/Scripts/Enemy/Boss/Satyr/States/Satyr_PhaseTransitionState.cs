using UnityEngine;

public class Satyr_PhaseTransitionState : EnemyState
{
    private Enemy_Satyr satyr;
    private float timer = 0f;

    public Satyr_PhaseTransitionState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        satyr = enemy as Enemy_Satyr;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.SetVelocity(0f, 0f);
        timer = 0f;

        satyr?.StopFireBreathEmitter();
        if (satyr?.currentSpiritBomb != null)
        {
            satyr.currentSpiritBomb.InterruptAndExplode();
        }

        satyr.ActivatePhase2();

        // Brief invincible frames during awakening
        if (satyr.satyrHealth != null)
            satyr.satyrHealth.SetCanTakeDamage(false);
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(0f, enemy.rb.linearVelocity.y);
        timer += Time.deltaTime;

        if (timer >= 0.5f)
        {
            stateMachine.ChangeState(satyr.satyrBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (satyr.satyrHealth != null)
            satyr.satyrHealth.SetCanTakeDamage(true);
    }
}
