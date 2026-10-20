using UnityEngine;

public class CrimsonBlade_ParryStanceState : EnemyState
{
    private Enemy_Boss_CrimsonBlade crimsonBlade;

    public CrimsonBlade_ParryStanceState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        crimsonBlade = enemy as Enemy_Boss_CrimsonBlade;
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = crimsonBlade.ParryStanceDuration;
        crimsonBlade.RecordParryTime();

        enemy.SetVelocity(0f, rb.linearVelocity.y);

        // Kích hoạt thế thủ phản đòn Blood Guard
        crimsonBlade.isParryGuarding = true;

        Transform player = crimsonBlade.GetPlayerTarget();
        if (player != null)
        {
            int dir = (player.position.x > enemy.transform.position.x) ? 1 : -1;
            enemy.HandleFlip(dir);
        }
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(0f, rb.linearVelocity.y);

        if (stateTimer <= 0f)
        {
            stateMachine.ChangeState(crimsonBlade.crimsonBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        crimsonBlade.isParryGuarding = false;
    }
}
