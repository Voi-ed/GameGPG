using UnityEngine;

public class Enemy_MageBattleState : Enemy_BattleState
{
    private Enemy_Mage enemyMage;
    private float lastTimeUsedRetreat = float.NegativeInfinity;

    public Enemy_MageBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyMage = enemy as Enemy_Mage;
    }

    public override void Enter()
    {
        if (player == null && enemyMage != null)
            player = enemyMage.GetPlayerTarget();

        base.Enter();

        enemy.MakeUntargetable(true);

        if (ShouldRetreat())
        {
            if (CanUseRetreatAbility())
                Retreat();
            else
                ShortRetreat();
        }
    }

    public override void Update()
    {
        if (player == null && enemyMage != null)
            player = enemyMage.GetPlayerTarget();

        // 1. Khi người chơi áp sát trong cự ly nguy hiểm (< 2.5m) và chiêu né lùi đã sẵn sàng: Lùi bước né đòn và phản công bắn phép
        if (ShouldRetreat() && CanUseRetreatAbility())
        {
            Retreat();
            return;
        }

        // 2. Khi ở tầm xa (3.5m - 9.5m), nhìn thấy người chơi và hết hồi chiêu phép: Niệm chú bắn 3 quả cầu lửa
        float dist = DistanceToPlayer();
        if (dist >= 3.5f && dist <= 9.5f && enemyMage != null && enemyMage.CanCastSpell() && enemy.PlayerDetected())
        {
            enemyMage.lastTimeCastSpell = Time.time;
            lastTimeAttacked = Time.time;
            enemy.SetVelocity(0, rb.linearVelocity.y);
            stateMachine.ChangeState(enemyMage.mageSpellCastState);
            return;
        }

        base.Update();
    }

    protected override bool ShouldRetreat()
    {
        if (player == null)
            return false;

        return DistanceToPlayer() < Mathf.Max(enemy.minRetreatDistance, 2.5f);
    }

    private void Retreat()
    {
        lastTimeUsedRetreat = Time.time;
        stateMachine.ChangeState(enemyMage.mageRetreatState);
    }

    private bool CanUseRetreatAbility() => Time.time > lastTimeUsedRetreat + enemyMage.retreatCooldown;
}
