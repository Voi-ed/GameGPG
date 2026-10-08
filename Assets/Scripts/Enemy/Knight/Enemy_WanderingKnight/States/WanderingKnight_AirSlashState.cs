using UnityEngine;

public class WanderingKnight_AirSlashState : EnemyState
{
    private Enemy_WanderingKnight wanderingKnight;

    public WanderingKnight_AirSlashState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        wanderingKnight = enemy as Enemy_WanderingKnight;
    }

    public override void Enter()
    {
        base.Enter();

        Transform target = enemy.player != null ? enemy.player : enemy.GetPlayerReference();
        float distToTarget = target != null ? Mathf.Abs(target.position.x - enemy.transform.position.x) : wanderingKnight.airSlashHorizontalSpeed;
        float dynamicHorizontal = Mathf.Clamp(distToTarget / 0.7f, 4f, 10f);

        enemy.SetVelocity(dynamicHorizontal * enemy.facingDir, wanderingKnight.airSlashJumpForce);
    }

    public override void Update()
    {
        base.Update();

        // Slow down upon landing
        if (enemy.groundDetected)
        {
            enemy.SetVelocity(Mathf.MoveTowards(rb.linearVelocity.x, 0, 14f * Time.deltaTime), rb.linearVelocity.y);
        }

        if (triggerCalled)
        {
            stateMachine.ChangeState(wanderingKnight.wkBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        wanderingKnight.SetAirSlashOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }

    public void OnAirSlashHit()
    {
        wanderingKnight.PerformAttackWithScale(wanderingKnight.airSlashAttackScale);
        wanderingKnight.sfx?.PlayAttackHit();
    }
}
