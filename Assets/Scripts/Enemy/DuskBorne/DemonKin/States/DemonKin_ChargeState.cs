using UnityEngine;

public class DemonKin_ChargeState : EnemyState
{
    private Enemy_DemonKin demonKin;
    private int chargeDirection;

    public DemonKin_ChargeState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        demonKin = enemy as Enemy_DemonKin;
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = demonKin.ChargeDuration;

        Transform player = demonKin != null ? demonKin.GetPlayerTarget() : null;
        if (player != null)
        {
            chargeDirection = player.position.x > enemy.transform.position.x ? 1 : -1;
            enemy.HandleFlip(chargeDirection);
        }
        else
        {
            chargeDirection = enemy.facingDir;
        }
    }

    public override void Update()
    {
        base.Update();

        stateTimer -= Time.deltaTime;
        enemy.SetVelocity(demonKin.ChargeSpeed * chargeDirection, rb.linearVelocity.y);

        Transform player = demonKin != null ? demonKin.GetPlayerTarget() : null;
        float distToPlayer = player != null ? Vector2.Distance(enemy.transform.position, player.position) : 999f;

        // Nếu đã áp sát được Player trong tầm bổ rìu -> chuyển sang AttackState vung rìu chém luôn!
        if (distToPlayer <= demonKin.AttackRange)
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
            demonKin?.SetChargeOnCooldown();
            stateMachine.ChangeState(demonKin.demonKinAttackState);
            return;
        }

        // Nếu hết thời gian lướt hoặc đâm vào tường -> dừng lướt và quay lại BattleState
        if (stateTimer <= 0f || enemy.wallDetected)
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
            demonKin?.SetChargeOnCooldown();
            stateMachine.ChangeState(demonKin.demonKinBattleState);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
        demonKin?.SetChargeOnCooldown();
    }
}
