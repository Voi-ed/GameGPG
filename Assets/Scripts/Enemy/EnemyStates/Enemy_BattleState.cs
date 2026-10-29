using UnityEngine;

public class Enemy_BattleState : EnemyState
{
    protected Transform player;
    protected Transform lastTarget;
    protected float lastTimeWasInBattle;
    protected float lastTimeAttacked = float.NegativeInfinity;
    private static float lastRoarTime = -10f;

    public Enemy_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }


    public override void Enter()
    {
        base.Enter();
        UpdateBattleTimer();

        if (player == null)
            player = enemy.GetPlayerReference();

        if (Time.time - lastRoarTime > 3.0f && Random.value < 0.7f && AudioManager.instance != null)
        {
            lastRoarTime = Time.time;
            AudioSource source = enemy.GetComponentInChildren<AudioSource>();
            if (source != null)
                AudioManager.instance.PlaySFX("monster_roar", source, 15f);
            else
                AudioManager.instance.PlayGlobalSFX("monster_roar");
        }

        if (ShouldRetreat())
        {
            ShortRetreat();
        }
    }

    protected void ShortRetreat()
    {
        float x = (enemy.retreatVelocity.x * enemy.activeSlowMultiplier) * -DirectionToPlayer();
        float y = enemy.retreatVelocity.y;

        rb.linearVelocity = new Vector2(x, y);
        enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        base.Update();

        if (enemy.PlayerDetected())
        {
            UpdateTargetIfNeeded();
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
            stateMachine.ChangeState(enemy.idleState);

        if (WithinAttackRange() && enemy.PlayerDetected())
        {
            if (CanAttack())
            {
                lastTimeAttacked = Time.time;
                stateMachine.ChangeState(enemy.attackState);
            }
            else
            {
                // Đã vào tầm đánh nhưng đang chờ hồi chiêu: Dừng lại giữ cự ly offset, không húc thẳng vào người chơi
                enemy.SetVelocity(0, rb.linearVelocity.y);
            }
        }
        else
        {
            float xVeloicty = enemy.canChasePlayer ? enemy.GetBattleMoveSpeed(): 0.0001f;
            enemy.SetVelocity(xVeloicty * DirectionToPlayer(), rb.linearVelocity.y);
        }
        
    }

    protected bool CanAttack() => Time.time > lastTimeAttacked + enemy.GetAttackCooldown();

    protected void UpdateTargetIfNeeded()
    {
        if (enemy.PlayerDetected() == false)
            return;

        Transform newTarget = enemy.PlayerDetected().transform;

        if (newTarget != lastTarget)
        {
            lastTarget = newTarget;
            player = newTarget;
        }
    }

    protected void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;

    protected bool BattleTimeIsOver() => Time.time > lastTimeWasInBattle + enemy.battleTimeDuration;

    protected virtual bool WithinAttackRange() => DistanceToPlayer() < enemy.GetAttackDistance();
    protected virtual bool ShouldRetreat() => DistanceToPlayer() < enemy.minRetreatDistance;

    protected float DistanceToPlayer()
    {
        if (player == null)
            return float.MaxValue;

        return Mathf.Abs(player.position.x - enemy.transform.position.x);
    }

    protected int DirectionToPlayer()
    {
        if (player == null)
            return 0;

        return player.position.x > enemy.transform.position.x ? 1 : -1;
    }

}
