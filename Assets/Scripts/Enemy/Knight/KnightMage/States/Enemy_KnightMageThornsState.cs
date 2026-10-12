using UnityEngine;

public class Enemy_KnightMageThornsState : EnemyState
{
    private Enemy_KnightMage knightMage;

    public Enemy_KnightMageThornsState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        knightMage = enemy as Enemy_KnightMage;
    }

    public override void Enter()
    {
        base.Enter();
        knightMage.SetVelocity(0, 0);
    }

    public override void Update()
    {
        base.Update();

        if (triggerCalled)
            stateMachine.ChangeState(knightMage.battleState);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
