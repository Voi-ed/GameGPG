using UnityEngine;

public class MartialHero_AttackState : Enemy_AttackState
{
    private Enemy_MartialHero martialHero;

    public MartialHero_AttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        martialHero = enemy as Enemy_MartialHero;
    }

    public override void Enter()
    {
        base.Enter();
        martialHero?.IncrementCombo();
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
        if (martialHero == null)
        {
            stateMachine.ChangeState(enemy.battleState);
            return;
        }

        float dist = DistanceToPlayer();
        int dir = DirectionToPlayer();
        bool playerInFront = (dir == 0 || dir == enemy.facingDir);

        // Nếu Player ở phía trước (trong phạm vi 2.8m sau cú chém 1) -> lập tức lướt lên chém tiếp Đòn 2 luôn!
        if (playerInFront && dist <= 2.8f)
        {
            stateMachine.ChangeState(martialHero.martialHeroComboState);
            return;
        }

        // Nếu Player đã né ra xa hoặc ra sau lưng: chỉ lùi nếu quá sát
        if (martialHero.ShouldRetreatFromPlayer())
        {
            stateMachine.ChangeState(martialHero.martialHeroRetreatState);
        }
        else
        {
            martialHero.SetAttackOnCooldown();
            stateMachine.ChangeState(martialHero.martialHeroBattleState);
        }
    }

    private Transform GetPlayer()
    {
        return martialHero != null ? martialHero.GetPlayerTarget() : null;
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
