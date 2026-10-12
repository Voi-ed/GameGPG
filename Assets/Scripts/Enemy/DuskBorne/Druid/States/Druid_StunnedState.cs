using UnityEngine;

public class Druid_StunnedState : Enemy_StunnedState
{
    private Enemy_Druid druid;

    public Druid_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        druid = enemy as Enemy_Druid;
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (druid != null)
                stateMachine.ChangeState(druid.druidBattleState);
            else
                stateMachine.ChangeState(enemy.idleState);
        }
    }
}
