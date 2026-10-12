using UnityEngine;

public class SamuraiZipp_Attack2State : EnemyState
{
    private Enemy_SamuraiZipp samuraiZipp;

    public SamuraiZipp_Attack2State(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        samuraiZipp = enemy as Enemy_SamuraiZipp;
    }

    public override void Enter()
    {
        base.Enter();
        samuraiZipp?.IncrementCombo();
        SyncAttackSpeed();

        int dir = DirectionToPlayer();
        if (dir != 0)
            enemy.HandleFlip(dir);

        if (samuraiZipp != null)
        {
            enemy.SetVelocity(samuraiZipp.ComboLungeSpeed * enemy.facingDir, rb.linearVelocity.y);
        }
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(Mathf.MoveTowards(rb.linearVelocity.x, 0, 10f * Time.deltaTime), rb.linearVelocity.y);

        if (triggerCalled)
        {
            EvaluateComboDecision();
        }
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

        // Nếu Player vẫn đứng trong tầm chém cận chiến (<= 2.2m) và chưa vượt quá combo limit:
        // Chém liên tục! Loop ngược lại đòn 1 mà không hề rơi về Idle!
        if (playerInFront && dist <= 2.2f && samuraiZipp.CanContinueCombo())
        {
            stateMachine.ChangeState(samuraiZipp.samuraiAttack1State);
            return;
        }
        else if (playerInFront && dist > 2.0f && dist <= samuraiZipp.ThrustMaxDistance && samuraiZipp.CanThrust())
        {
            // Player vừa lùi ra khỏi tầm cận chiến -> Truy kích phi thân đâm Thrust!
            stateMachine.ChangeState(samuraiZipp.samuraiThrustState);
            return;
        }

        samuraiZipp.SetAttackOnCooldown();
        stateMachine.ChangeState(samuraiZipp.samuraiBattleState);
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetVelocity(0, rb.linearVelocity.y);
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
