using UnityEngine;

public class Satyr_DashState : EnemyState
{
    private Enemy_Satyr satyr;
    private float dashTimer = 0f;
    private int dashDirection = 1;

    public Satyr_DashState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        satyr = enemy as Enemy_Satyr;
    }

    public void SetDashDirection(int dir)
    {
        dashDirection = dir != 0 ? dir : enemy.facingDir;
    }

    public override void Enter()
    {
        base.Enter();

        // Ensure any fire breath VFX and hitbox are strictly cleared before dashing
        satyr?.ForceKillFireBreathEmitter();

        dashTimer = 0f;
        enemy.HandleFlip(dashDirection);

        // Allow passing smoothly through player without collision snagging
        enemy.MakeUntargetable(false);
        enemy.SetVelocity(satyr.DashSpeed * dashDirection, enemy.rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        dashTimer += Time.deltaTime;
        enemy.SetVelocity(satyr.DashSpeed * dashDirection, enemy.rb.linearVelocity.y);

        if (dashTimer >= satyr.DashDuration || triggerCalled || enemy.wallDetected)
        {
            stateMachine.ChangeState(satyr.satyrBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        enemy.MakeUntargetable(true);
        enemy.SetVelocity(0f, enemy.rb.linearVelocity.y);
        satyr.SetDashOnCooldown();
    }
}
