using UnityEngine;

public class BlackKnight_EnrageState : EnemyState
{
    private Enemy_BlackKnight blackKnight;

    public BlackKnight_EnrageState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        blackKnight = enemy as Enemy_BlackKnight;
    }

    public override void Enter()
    {
        base.Enter();

        blackKnight.hasHyperArmor = true;
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(0, rb.linearVelocity.y);

        if (triggerCalled)
        {
            // After roar/buff, immediately unleash Weapon Art as Phase 2 introduction!
            stateMachine.ChangeState(blackKnight.bkWeaponArtState);
        }
    }

    public void OnEnrageBuff()
    {
        blackKnight.TriggerEnrageNova();
    }

    public override void Exit()
    {
        base.Exit();

        blackKnight.hasHyperArmor = false;
    }
}
