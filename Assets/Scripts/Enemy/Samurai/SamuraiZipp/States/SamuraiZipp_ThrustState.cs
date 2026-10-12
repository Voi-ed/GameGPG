using UnityEngine;

public class SamuraiZipp_ThrustState : EnemyState
{
    private Enemy_SamuraiZipp samuraiZipp;
    private float windupDuration = 0.18f;
    private float dashDuration = 0.22f;
    private bool isDashing;
    private bool hasDealtDamage;
    private float subTimer;
    private Transform playerTarget;

    public SamuraiZipp_ThrustState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        samuraiZipp = enemy as Enemy_SamuraiZipp;
    }

    public override void Enter()
    {
        base.Enter();

        isDashing = false;
        hasDealtDamage = false;
        subTimer = windupDuration;
        enemy.SetVelocity(0, rb.linearVelocity.y);

        playerTarget = samuraiZipp != null ? samuraiZipp.GetPlayerTarget() : null;
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
                subTimer = samuraiZipp != null ? samuraiZipp.ThrustDuration : dashDuration;
                enemy.vfx?.DoImageEchoEffect(subTimer);
            }
        }
        else
        {
            float speed = samuraiZipp != null ? samuraiZipp.ThrustSpeed : 14f;
            enemy.SetVelocity(speed * enemy.facingDir, rb.linearVelocity.y);
            subTimer -= Time.deltaTime;

            if (!hasDealtDamage && playerTarget != null)
            {
                float dist = Vector2.Distance(enemy.transform.position, playerTarget.position);
                if (dist <= 1.8f)
                {
                    hasDealtDamage = true;
                    samuraiZipp?.PerformThrustDamage();
                }
            }

            if (subTimer <= 0 || triggerCalled)
            {
                enemy.SetVelocity(0, rb.linearVelocity.y);
                if (!hasDealtDamage && playerTarget != null)
                {
                    float dist = Vector2.Distance(enemy.transform.position, playerTarget.position);
                    if (dist <= 2.2f)
                    {
                        hasDealtDamage = true;
                        samuraiZipp?.PerformThrustDamage();
                    }
                }

                stateMachine.ChangeState(samuraiZipp != null ? samuraiZipp.samuraiBattleState : enemy.battleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.vfx?.StopImageEchoEffect();
        samuraiZipp?.SetThrustOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }
}
