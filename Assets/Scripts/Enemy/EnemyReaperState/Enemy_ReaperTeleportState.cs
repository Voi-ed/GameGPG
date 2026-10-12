using UnityEngine;

public class Enemy_ReaperTeleportState : EnemyState
{
    private Enemy_Reaper enemyReaper;
    public Enemy_ReaperTeleportState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyReaper = enemy as Enemy_Reaper;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.SetVelocity(0f, 0f);
        enemyReaper.MakeUntargetable(false);
    }

    public override void Update()
    {
        base.Update();

        if (enemyReaper.teleporTrigger)
        {
            enemyReaper.transform.position = enemyReaper.FindTeleportPoint();

            Transform playerTarget = enemyReaper.player != null ? enemyReaper.player : (Player.instance != null ? Player.instance.transform : null);
            if (playerTarget != null)
            {
                int dir = playerTarget.position.x > enemyReaper.transform.position.x ? 1 : -1;
                enemyReaper.HandleFlip(dir);
            }

            enemyReaper.SetTeleportTrigger(false);
        }

        if (triggerCalled)
        {
            if (enemyReaper.CanDoSpellCast())
                stateMachine.ChangeState(enemyReaper.reaperSpellCastState);
            else
                stateMachine.ChangeState(enemyReaper.reaperBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemyReaper.MakeUntargetable(true);
    }
}
