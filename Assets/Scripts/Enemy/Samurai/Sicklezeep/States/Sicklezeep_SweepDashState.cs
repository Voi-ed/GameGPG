using UnityEngine;

public class Sicklezeep_SweepDashState : EnemyState
{
    private Enemy_Sicklezeep sicklezeep;
    private float windupDuration = 0.12f;
    private float dashDuration = 0.25f;
    private bool isDashing;
    private bool hasDealtDamage;
    private float subTimer;
    private Transform playerTarget;

    public Sicklezeep_SweepDashState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        sicklezeep = enemy as Enemy_Sicklezeep;
    }

    public override void Enter()
    {
        base.Enter();

        isDashing = false;
        hasDealtDamage = false;
        subTimer = windupDuration;
        enemy.SetVelocity(0, rb.linearVelocity.y);

        playerTarget = sicklezeep != null ? sicklezeep.GetPlayerTarget() : null;
        if (playerTarget != null)
        {
            int dir = playerTarget.position.x > enemy.transform.position.x ? 1 : -1;
            if (dir != 0 && dir != enemy.facingDir)
            {
                enemy.Flip();
            }
        }
    }

    public override void Update()
    {
        base.Update();

        if (!isDashing)
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            subTimer -= Time.deltaTime;

            if (subTimer <= 0)
            {
                isDashing = true;
                subTimer = sicklezeep != null ? sicklezeep.SweepDuration : dashDuration;
                enemy.vfx?.DoImageEchoEffect(subTimer);
            }
        }
        else
        {
            float speed = sicklezeep != null ? sicklezeep.SweepSpeed : 10f;
            enemy.SetVelocity(speed * enemy.facingDir, rb.linearVelocity.y);
            subTimer -= Time.deltaTime;

            if (!hasDealtDamage && playerTarget != null)
            {
                float dist = Mathf.Abs(playerTarget.position.x - enemy.transform.position.x);
                if (dist <= 1.8f)
                {
                    hasDealtDamage = true;
                    sicklezeep?.PerformSweepDamage();
                }
            }

            if (subTimer <= 0)
            {
                EndDash();
            }
        }

        if (triggerCalled)
        {
            EndDash();
        }
    }

    private void EndDash()
    {
        enemy.SetVelocity(0, rb.linearVelocity.y);
        sicklezeep?.SetSweepDashOnCooldown();
        stateMachine.ChangeState(sicklezeep != null ? sicklezeep.sickleBattleState : enemy.battleState);
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }
}
