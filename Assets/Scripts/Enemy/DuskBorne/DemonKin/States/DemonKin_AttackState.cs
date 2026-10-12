using UnityEngine;

public class DemonKin_AttackState : Enemy_AttackState
{
    private Enemy_DemonKin demonKin;
    private const float fallbackDuration = 1.5f;

    public DemonKin_AttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        demonKin = enemy as Enemy_DemonKin;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.SetVelocity(0f, rb.linearVelocity.y);

        Transform player = demonKin != null ? demonKin.GetPlayerTarget() : null;
        if (player != null)
        {
            int dir = player.position.x > enemy.transform.position.x ? 1 : -1;
            enemy.HandleFlip(dir);
        }

        // Bật Siêu Giáp khi giáng rìu xuống đất
        if (demonKin != null)
            demonKin.hasHyperArmor = true;

        stateTimer = fallbackDuration;
    }

    public override void Update()
    {
        base.Update();

        stateTimer -= Time.deltaTime;
        enemy.SetVelocity(0f, rb.linearVelocity.y);

        if (triggerCalled || stateTimer <= 0f)
        {
            demonKin?.SetAttackOnCooldown();
            stateMachine.ChangeState(demonKin.demonKinBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (demonKin != null)
            demonKin.hasHyperArmor = false;

        demonKin?.SetAttackOnCooldown();
    }
}
