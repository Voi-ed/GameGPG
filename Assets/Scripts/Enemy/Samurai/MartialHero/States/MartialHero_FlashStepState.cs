using UnityEngine;

public class MartialHero_FlashStepState : EnemyState
{
    private Enemy_MartialHero martialHero;
    private float windupDuration = 0.22f;
    private float dashSpeed = 16f;
    private float dashDuration = 0.22f;
    private bool isDashing;
    private bool hasDealtDamage;
    private float subTimer;
    private Transform playerTarget;

    public MartialHero_FlashStepState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        martialHero = enemy as Enemy_MartialHero;
    }

    public override void Enter()
    {
        base.Enter();

        isDashing = false;
        hasDealtDamage = false;
        subTimer = windupDuration;
        enemy.SetVelocity(0, rb.linearVelocity.y);

        playerTarget = martialHero.GetPlayerTarget();
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
                subTimer = dashDuration;
                enemy.vfx?.DoImageEchoEffect(dashDuration);
            }
        }
        else
        {
            enemy.SetVelocity(dashSpeed * enemy.facingDir, rb.linearVelocity.y);
            subTimer -= Time.deltaTime;

            if (!hasDealtDamage && playerTarget != null)
            {
                float dist = Vector2.Distance(enemy.transform.position, playerTarget.position);
                if (dist <= 1.8f)
                {
                    hasDealtDamage = true;
                    martialHero.PerformFlashStepHit(playerTarget);
                }
            }

            if (subTimer <= 0)
            {
                enemy.SetVelocity(0, rb.linearVelocity.y);
                if (!hasDealtDamage && playerTarget != null)
                {
                    float dist = Vector2.Distance(enemy.transform.position, playerTarget.position);
                    if (dist <= 2.5f)
                    {
                        hasDealtDamage = true;
                        martialHero.PerformFlashStepHit(playerTarget);
                    }
                }

                if (martialHero != null && martialHero.ShouldRetreatFromPlayer())
                    stateMachine.ChangeState(martialHero.martialHeroRetreatState);
                else
                    stateMachine.ChangeState(martialHero.martialHeroBattleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.vfx?.StopImageEchoEffect();
        martialHero.SetFlashStepOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }
}
