using UnityEngine;

public class ArchDemon_StunnedState : Enemy_StunnedState
{
    private Enemy_ArchDemon archDemon;

    public ArchDemon_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        archDemon = enemy as Enemy_ArchDemon;
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (archDemon != null)
                stateMachine.ChangeState(archDemon.archDemonBattleState);
            else
                stateMachine.ChangeState(enemy.idleState);
        }
    }
}
