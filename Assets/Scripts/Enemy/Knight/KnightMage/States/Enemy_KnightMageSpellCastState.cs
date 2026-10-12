using UnityEngine;

public class Enemy_KnightMageSpellCastState : EnemyState
{
    private Enemy_KnightMage knightMage;

    public Enemy_KnightMageSpellCastState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        knightMage = enemy as Enemy_KnightMage;
    }

    public override void Enter()
    {
        base.Enter();

        knightMage.SetVelocity(0, 0);
        knightMage.SetSpellCastPerformed(false);
        knightMage.SetSpellOnCooldown();
    }

    public override void Update()
    {
        base.Update();

        if (knightMage.spellCastPerformed)
            anim.SetBool("spellCast_performed", true);

        if (triggerCalled && knightMage.spellCastPerformed)
            stateMachine.ChangeState(knightMage.battleState);
    }

    public override void Exit()
    {
        base.Exit();
        knightMage.StopSpellCast();
        anim.SetBool("spellCast_performed", false);
    }
}
