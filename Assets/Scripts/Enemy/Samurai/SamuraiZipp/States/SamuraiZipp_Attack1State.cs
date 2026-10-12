using UnityEngine;

public class SamuraiZipp_Attack1State : Enemy_AttackState
{
    private Enemy_SamuraiZipp samuraiZipp;

    public SamuraiZipp_Attack1State(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        samuraiZipp = enemy as Enemy_SamuraiZipp;
    }

    public override void Enter()
    {
        base.Enter();
        samuraiZipp?.IncrementCombo();
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
        if (samuraiZipp == null)
        {
            stateMachine.ChangeState(enemy.battleState);
            return;
        }

        float dist = DistanceToPlayer();
        int dir = DirectionToPlayer();
        bool playerInFront = (dir == 0 || dir == enemy.facingDir);

        // Nếu Player vẫn đứng phía trước trong tầm (<= 2.2m) -> Nối tiếp combo Phách Không Trảm (Attack 2)!
        if (playerInFront && dist <= 2.2f)
        {
            stateMachine.ChangeState(samuraiZipp.samuraiAttack2State);
            return;
        }
        else if (playerInFront && dist > 2.0f && dist <= samuraiZipp.ThrustMaxDistance && samuraiZipp.CanThrust())
        {
            stateMachine.ChangeState(samuraiZipp.samuraiThrustState);
            return;
        }

        samuraiZipp.SetAttackOnCooldown();
        stateMachine.ChangeState(samuraiZipp.samuraiBattleState);
    }

    private Transform GetPlayer()
    {
        return samuraiZipp != null ? samuraiZipp.GetPlayerTarget() : null;
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
