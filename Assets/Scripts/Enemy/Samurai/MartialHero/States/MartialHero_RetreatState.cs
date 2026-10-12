using UnityEngine;

public class MartialHero_RetreatState : EnemyState
{
    private Enemy_MartialHero martialHero;

    public MartialHero_RetreatState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        martialHero = enemy as Enemy_MartialHero;
    }

    public override void Enter()
    {
        base.Enter();

        if (martialHero == null)
            return;

        stateTimer = martialHero.RetreatDuration;

        // Luôn quay mặt hướng về phía Player khi bật lùi
        int dir = DirectionToPlayer();
        if (dir != 0)
            enemy.HandleFlip(dir);

        // Bật nhảy lùi vật lý ra sau
        float x = martialHero.RetreatVelocity.x * -enemy.facingDir;
        float y = martialHero.RetreatVelocity.y;
        rb.linearVelocity = new Vector2(x, y);
    }

    public override void Update()
    {
        base.Update();

        // Luôn gườm mắt nhìn Player trong suốt cú nhảy lùi
        int dir = DirectionToPlayer();
        if (dir != 0)
            enemy.HandleFlip(dir);

        // Khi hết thời gian bật lùi (và đã tiếp đất) -> Quay về BattleState
        if (stateTimer <= 0)
        {
            stateMachine.ChangeState(martialHero.martialHeroBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (martialHero != null)
        {
            martialHero.SetAttackOnCooldown();
            enemy.SetVelocity(0, rb.linearVelocity.y);
        }
    }

    private Transform GetPlayer()
    {
        return martialHero != null ? martialHero.GetPlayerTarget() : null;
    }

    private int DirectionToPlayer()
    {
        Transform target = GetPlayer();
        if (target == null)
            return 0;

        return target.position.x > enemy.transform.position.x ? 1 : -1;
    }
}
