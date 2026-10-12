using UnityEngine;

public class MartialHero_ComboAttackState : EnemyState
{
    private Enemy_MartialHero martialHero;

    public MartialHero_ComboAttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        martialHero = enemy as Enemy_MartialHero;
    }

    public override void Enter()
    {
        base.Enter();
        martialHero?.IncrementCombo();
        SyncAttackSpeed();

        int dir = DirectionToPlayer();
        if (dir != 0)
            enemy.HandleFlip(dir);

        if (martialHero != null)
        {
            enemy.SetVelocity(martialHero.ComboLungeSpeed * enemy.facingDir, rb.linearVelocity.y);
        }
    }

    public override void Update()
    {
        base.Update();

        // Giảm tốc mượt sau cú lướt nhẹ
        enemy.SetVelocity(Mathf.MoveTowards(rb.linearVelocity.x, 0, 10f * Time.deltaTime), rb.linearVelocity.y);

        if (triggerCalled)
        {
            if (martialHero != null)
            {
                float dist = DistanceToPlayer();
                int dir = DirectionToPlayer();
                bool playerInFront = (dir == 0 || dir == enemy.facingDir);

                // Nếu Player vẫn đứng trong tầm chém cận chiến (<= 2.2m) và chưa hết chuỗi combo:
                // Chém liên tục! Loop ngược lại đòn 1 mà không hề rơi về Idle!
                if (playerInFront && dist <= 2.2f && martialHero.CanContinueCombo())
                {
                    stateMachine.ChangeState(martialHero.martialHeroAttackState);
                    return;
                }

                martialHero.SetAttackOnCooldown();

                // Đánh xong combo: Chỉ lùi lại nếu ở quá gần Player
                if (martialHero.ShouldRetreatFromPlayer())
                {
                    stateMachine.ChangeState(martialHero.martialHeroRetreatState);
                }
                else
                {
                    stateMachine.ChangeState(martialHero.martialHeroBattleState);
                }
            }
            else
            {
                stateMachine.ChangeState(enemy.battleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetVelocity(0, rb.linearVelocity.y);
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
