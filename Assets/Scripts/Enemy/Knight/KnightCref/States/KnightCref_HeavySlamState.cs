using UnityEngine;

public class KnightCref_HeavySlamState : EnemyState
{
    private Enemy_KnightCref knightCref;

    public KnightCref_HeavySlamState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        knightCref = enemy as Enemy_KnightCref;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.SetVelocity(0, rb.linearVelocity.y);

        // Đòn Heavy Slam là Unblockable -> tắt cửa sổ Counter
        enemy.EnableCounterWindow(false);
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(0, rb.linearVelocity.y);

        if (triggerCalled)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.EnableCounterWindow(false);
    }
}
