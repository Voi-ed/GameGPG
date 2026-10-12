using UnityEngine;

public class KnightCref_DashState : EnemyState
{
    private Enemy_KnightCref knightCref;
    private int dashDir;

    public KnightCref_DashState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        knightCref = enemy as Enemy_KnightCref;
    }

    public override void Enter()
    {
        base.Enter();

        Transform player = enemy.GetPlayerReference();
        if (player != null)
        {
            dashDir = player.position.x > enemy.transform.position.x ? 1 : -1;
            enemy.HandleFlip(dashDir);
        }
        else
        {
            dashDir = enemy.facingDir;
        }

        stateTimer = knightCref != null ? knightCref.DashDuration : 0.25f;
    }

    public override void Update()
    {
        base.Update();

        float speed = knightCref != null ? knightCref.DashSpeed : 9f;
        enemy.SetVelocity(speed * dashDir, rb.linearVelocity.y);

        if (stateTimer <= 0f)
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);

            // Nếu đã áp sát mục tiêu (trong tầm chém) -> Vung kiếm chém ngay lập tức!
            Transform player = enemy.GetPlayerReference();
            if (player != null)
            {
                float dist = Mathf.Abs(player.position.x - enemy.transform.position.x);
                if (dist <= enemy.attackDistance + 0.6f)
                {
                    stateMachine.ChangeState(knightCref.crefAttackState);
                    return;
                }
            }

            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
