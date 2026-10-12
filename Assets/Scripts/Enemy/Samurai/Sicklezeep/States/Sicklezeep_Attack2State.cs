using UnityEngine;

public class Sicklezeep_Attack2State : EnemyState
{
    private Enemy_Sicklezeep sicklezeep;

    public Sicklezeep_Attack2State(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        sicklezeep = enemy as Enemy_Sicklezeep;
    }

    public override void Enter()
    {
        base.Enter();
        sicklezeep?.IncrementCombo();
        SyncAttackSpeed();

        int dir = DirectionToPlayer();
        if (dir != 0)
            enemy.HandleFlip(dir);

        if (sicklezeep != null)
        {
            enemy.SetVelocity(sicklezeep.ComboLungeSpeed * enemy.facingDir, rb.linearVelocity.y);
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
        if (sicklezeep == null)
        {
            stateMachine.ChangeState(enemy.battleState);
            return;
        }

        float dist = DistanceToPlayer();
        int dir = DirectionToPlayer();
        bool playerInFront = (dir == 0 || dir == enemy.facingDir);

        // Nếu Player vẫn đứng trong tầm chém cận chiến (<= 2.0m) và chưa vượt quá combo limit:
        // Chém liên tục! Loop ngược lại đòn 1 mà không hề rơi về Idle!
        if (playerInFront && dist <= sicklezeep.Attack2Distance && sicklezeep.CanContinueCombo())
        {
            stateMachine.ChangeState(sicklezeep.sickleAttack1State);
            return;
        }
        else if (playerInFront && dist >= sicklezeep.SweepMinDistance && dist <= sicklezeep.SweepMaxDistance && sicklezeep.CanSweepDash())
        {
            // Player vừa lùi ra khỏi tầm cận chiến -> Truy kích trượt liêm đao Sweep Dash!
            stateMachine.ChangeState(sicklezeep.sickleSweepDashState);
            return;
        }

        sicklezeep.SetAttackOnCooldown();
        stateMachine.ChangeState(sicklezeep.sickleBattleState);
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetVelocity(0, rb.linearVelocity.y);
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
