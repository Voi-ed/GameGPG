using UnityEngine;

public class SeveredFang_StunnedState : Enemy_StunnedState
{
    private Enemy_SeveredFang severedFang;

    public SeveredFang_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        severedFang = enemy as Enemy_SeveredFang;
    }

    public override void Enter()
    {
        base.Enter();

        severedFang?.SetCounterWindow(false);
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (severedFang != null)
                stateMachine.ChangeState(severedFang.severedFangBattleState);
            else
                stateMachine.ChangeState(enemy.idleState);
        }
    }
}
