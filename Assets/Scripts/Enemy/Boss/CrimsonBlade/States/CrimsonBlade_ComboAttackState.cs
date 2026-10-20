using UnityEngine;

public class CrimsonBlade_ComboAttackState : EnemyState
{
    private Enemy_Boss_CrimsonBlade crimsonBlade;

    public CrimsonBlade_ComboAttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        crimsonBlade = enemy as Enemy_Boss_CrimsonBlade;
    }

    public override void Enter()
    {
        base.Enter();

        crimsonBlade.RecordAttackTime();

        // Nhích nhẹ một bước về phía mục tiêu khi chém nếu cự ly còn xa, nếu đã sát người thì đứng yên chém
        Transform player = crimsonBlade.GetPlayerTarget();
        if (player != null)
        {
            int dir = (player.position.x > enemy.transform.position.x) ? 1 : -1;
            enemy.HandleFlip(dir);

            float distX = Mathf.Abs(player.position.x - enemy.transform.position.x);
            if (distX > 1.5f)
            {
                enemy.SetVelocity(dir * 1.5f, rb.linearVelocity.y);
            }
            else
            {
                enemy.SetVelocity(0f, rb.linearVelocity.y);
            }
        }
        else
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
        }
    }

    public override void Update()
    {
        base.Update();

        // Giảm dần quán tính trượt khi chém
        enemy.SetVelocity(Mathf.MoveTowards(rb.linearVelocity.x, 0f, 6f * Time.deltaTime), rb.linearVelocity.y);

        if (triggerCalled)
        {
            stateMachine.ChangeState(crimsonBlade.crimsonBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetVelocity(0f, rb.linearVelocity.y);
        enemy.EnableCounterWindow(false);
    }
}
