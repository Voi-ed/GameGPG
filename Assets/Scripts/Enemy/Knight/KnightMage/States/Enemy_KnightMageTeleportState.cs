using UnityEngine;

public class Enemy_KnightMageTeleportState : EnemyState
{
    private Enemy_KnightMage knightMage;

    public Enemy_KnightMageTeleportState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        knightMage = enemy as Enemy_KnightMage;
    }

    public override void Enter()
    {
        base.Enter();

        knightMage.SetVelocity(0, 0);
        knightMage.MakeUntargetable(false);
        knightMage.SetTeleportTrigger(false);
        knightMage.SetTeleportOnCooldown();
    }

    public override void Update()
    {
        base.Update();

        if (knightMage.teleportTrigger)
        {
            knightMage.transform.position = knightMage.FindTeleportPoint();

            Transform targetPlayer = knightMage.GetPlayerTarget();
            if (targetPlayer != null)
            {
                int dir = targetPlayer.position.x > knightMage.transform.position.x ? 1 : -1;
                knightMage.HandleFlip(dir);
            }

            knightMage.SetTeleportTrigger(false);
        }

        if (triggerCalled)
            stateMachine.ChangeState(knightMage.battleState);
    }

    public override void Exit()
    {
        base.Exit();
        knightMage.MakeUntargetable(true);
    }
}
