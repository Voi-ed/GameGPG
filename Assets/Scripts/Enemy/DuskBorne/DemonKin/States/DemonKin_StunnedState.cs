using UnityEngine;

public class DemonKin_StunnedState : Enemy_StunnedState
{
    private Enemy_DemonKin demonKin;

    public DemonKin_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        demonKin = enemy as Enemy_DemonKin;
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (demonKin != null)
                stateMachine.ChangeState(demonKin.demonKinBattleState);
            else
                stateMachine.ChangeState(enemy.idleState);
        }
    }
}
