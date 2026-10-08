using UnityEngine;

public class BlackKnight_ParryState : EnemyState
{
    private Enemy_BlackKnight blackKnight;
    public bool isRiposting { get; private set; }
    public bool isWhiffing { get; private set; }
    private Transform riposteTarget;

    public BlackKnight_ParryState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        blackKnight = enemy as Enemy_BlackKnight;
    }

    public override void Enter()
    {
        base.Enter();

        blackKnight.isParrying = true;
        isRiposting = false;
        isWhiffing = false;
        riposteTarget = null;
        stateTimer = blackKnight.parryDuration;

        enemy.SetVelocity(0, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        if (isRiposting || isWhiffing)
        {
            if (triggerCalled)
            {
                stateMachine.ChangeState(blackKnight.bkBattleState);
            }
            return;
        }

        // Active parry stance timeout
        if (stateTimer <= 0)
        {
            // Whiffed parry: staggered vulnerable window for player
            blackKnight.isParrying = false;
            isWhiffing = true;
            triggerCalled = false;
            anim.SetTrigger("parryMiss");
        }
    }

    public void TriggerRiposte(Transform attacker)
    {
        blackKnight.isParrying = false;
        isRiposting = true;
        triggerCalled = false;

        // Resolve actual attacker transform (player or root)
        Transform actualTarget = attacker != null ? (attacker.GetComponentInParent<Player>()?.transform ?? attacker) : null;
        riposteTarget = actualTarget;

        // Face attacker
        if (actualTarget != null)
        {
            float dir = actualTarget.position.x - enemy.transform.position.x;
            enemy.HandleFlip(dir);

            // Stagger / Knockback the attacking entity who struck the parry
            Entity attackerEntity = actualTarget.GetComponent<Entity>();
            if (attackerEntity != null)
            {
                int knockDir = actualTarget.position.x > enemy.transform.position.x ? 1 : -1;
                attackerEntity.ReciveKnockback(new Vector2(knockDir * 5f, 2f), 0.35f);
            }
        }

        // Distance check for ranged vs melee parry
        float distToAttacker = actualTarget != null ? Mathf.Abs(actualTarget.position.x - enemy.transform.position.x) : 0f;

        // If attacker is out of melee reach (> 2.8m), deflect projectile and punish with gap-closing Thrust!
        if (distToAttacker > 2.8f && blackKnight.CanHeavyAttack())
        {
            blackKnight.SetHeavyAttackIndex(1); // Thrust gap-closer
            stateMachine.ChangeState(blackKnight.bkHeavyAttackState);
            return;
        }

        anim.SetTrigger("parryRiposte");
    }

    public void OnRiposteHit()
    {
        if (riposteTarget != null)
        {
            blackKnight.PerformAttackOnTarget(riposteTarget, blackKnight.riposteDamageScale);
        }
        else
        {
            blackKnight.PerformAttackWithScale(blackKnight.riposteDamageScale);
        }
    }

    public override void Exit()
    {
        base.Exit();

        blackKnight.isParrying = false;
        isRiposting = false;
        isWhiffing = false;
        riposteTarget = null;
        blackKnight.SetParryOnCooldown();
    }
}
