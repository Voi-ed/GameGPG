using UnityEngine;

public class BlackKnight_WeaponArtState : EnemyState
{
    private Enemy_BlackKnight blackKnight;

    public BlackKnight_WeaponArtState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        blackKnight = enemy as Enemy_BlackKnight;
    }

    public override void Enter()
    {
        base.Enter();

        blackKnight.hasHyperArmor = true;
        enemy.SetVelocity(0, rb.linearVelocity.y);

        // Turn to face player when initiating Ultimate
        Transform target = enemy.player != null ? enemy.player : enemy.GetPlayerReference();
        if (target != null)
        {
            int dirToPlayer = target.position.x > enemy.transform.position.x ? 1 : -1;
            if (dirToPlayer != 0 && dirToPlayer != enemy.facingDir)
            {
                enemy.Flip();
            }
        }

        // 1. Telegraph Warning Alert (Red exclamation mark above Boss)
        blackKnight.EnableAttackAlert(true);

        // 2. Summon 5 Infernal Fire Orbs orbiting above Boss
        blackKnight.ability?.CastInfernalOrbsUltimate();
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(0, rb.linearVelocity.y);

        if (triggerCalled)
        {
            stateMachine.ChangeState(blackKnight.bkBattleState);
        }
    }

    public void OnWeaponArtHit()
    {
        // Melee slash to protect Boss while casting
        blackKnight.PerformWeaponArtAttack();

        // Enraged Weapon Art sweeps a larger fiery perimeter
        if (blackKnight.isEnraged)
        {
            blackKnight.PerformSurroundingAttack(blackKnight.weaponArtDamageScale, 3.5f);
        }
    }

    public override void Exit()
    {
        base.Exit();

        blackKnight.hasHyperArmor = false;
        blackKnight.EnableAttackAlert(false);
        blackKnight.SetWeaponArtOnCooldown();
        enemy.SetVelocity(0, rb.linearVelocity.y);
    }
}
