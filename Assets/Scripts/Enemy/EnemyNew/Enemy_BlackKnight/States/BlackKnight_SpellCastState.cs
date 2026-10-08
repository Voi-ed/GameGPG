using UnityEngine;

public class BlackKnight_SpellCastState : EnemyState
{
    private Enemy_BlackKnight blackKnight;

    public BlackKnight_SpellCastState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        blackKnight = enemy as Enemy_BlackKnight;
    }

    public override void Enter()
    {
        base.Enter();

        blackKnight.hasHyperArmor = true;
        enemy.SetVelocity(0, rb.linearVelocity.y);
        stateTimer = blackKnight.hellfireChannelDuration;

        blackKnight.StartHellfireSpell();
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(0, rb.linearVelocity.y);

        if (stateTimer <= 0 && blackKnight.isCastingHellfire == false)
        {
            stateMachine.ChangeState(blackKnight.bkBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        blackKnight.hasHyperArmor = false;
        blackKnight.SetHellfireOnCooldown();
    }
}
