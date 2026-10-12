using UnityEngine;

public class Sicklezeep_Attack1State : Enemy_AttackState
{
    private Enemy_Sicklezeep sicklezeep;

    public Sicklezeep_Attack1State(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        sicklezeep = enemy as Enemy_Sicklezeep;
    }

    public override void Enter()
    {
        base.Enter();
        sicklezeep?.IncrementCombo();
        enemy.SetVelocity(0, rb.linearVelocity.y);

        int dir = DirectionToPlayer();
        if (dir != 0)
            enemy.HandleFlip(dir);
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();
        enemy.SetVelocity(0, rb.linearVelocity.y);

        if (triggerCalled)
        {
            EvaluateComboDecision();
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.EnableCounterWindow(false);
    }

    private void EvaluateComboDecision()
    {
        if (sicklezeep == null)
        {
            stateMachine.ChangeState(enemy.battleState);
            return;
        }

        float dist = DistanceToPlayer();
        int dir = DirectionToPlayer();
        bool playerInFront = (dir == 0 || dir == enemy.facingDir);

        // Nếu Player vẫn đứng phía trước trong tầm (<= 2.0m) -> Nối tiếp combo Tàn Nguyệt Bổ (Attack 2)!
        if (playerInFront && dist <= sicklezeep.Attack2Distance)
        {
            stateMachine.ChangeState(sicklezeep.sickleAttack2State);
            return;
        }
        else if (playerInFront && dist >= sicklezeep.SweepMinDistance && dist <= sicklezeep.SweepMaxDistance && sicklezeep.CanSweepDash())
        {
            // Player vừa lùi ra tầm trung -> Truy kích bằng cú trượt liêm đao!
            stateMachine.ChangeState(sicklezeep.sickleSweepDashState);
            return;
        }

        sicklezeep.SetAttackOnCooldown();
        stateMachine.ChangeState(sicklezeep.sickleBattleState);
    }

    private Transform GetPlayer()
    {
        return sicklezeep != null ? sicklezeep.GetPlayerTarget() : null;
    }

    private float DistanceToPlayer()
    {
        Transform target = GetPlayer();
        if (target == null)
            return float.MaxValue;

        return Mathf.Abs(target.position.x - enemy.transform.position.x);
    }

    private int DirectionToPlayer()
    {
        Transform target = GetPlayer();
        if (target == null)
            return 0;

        return target.position.x > enemy.transform.position.x ? 1 : -1;
    }
}
