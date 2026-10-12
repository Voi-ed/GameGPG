using UnityEngine;

public class ArchDemon_SpellcastState : Enemy_AttackState
{
    private Enemy_ArchDemon archDemon;
    private const float fallbackDuration = 1.35f;
    private bool isBreathing = false;

    public ArchDemon_SpellcastState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        archDemon = enemy as Enemy_ArchDemon;
    }

    public override void Enter()
    {
        base.Enter();

        isBreathing = false;
        enemy.SetVelocity(0f, 0f);

        Transform player = archDemon != null ? archDemon.GetPlayerTarget() : null;
        if (player != null)
        {
            int dir = player.position.x > enemy.transform.position.x ? 1 : -1;
            enemy.HandleFlip(dir);
        }

        // Bật Siêu Giáp trong lúc phun hắc hỏa tương tự Satyr
        if (archDemon != null)
            archDemon.hasHyperArmor = true;

        stateTimer = fallbackDuration;
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(0f, 0f);

        // Fallback hẹn giờ phòng ngừa animation event bị rớt khung hình
        float elapsed = fallbackDuration - stateTimer;
        if (!isBreathing && elapsed >= 0.58f && elapsed < 1.0f)
        {
            isBreathing = true;
            archDemon?.StartDarkBreath();
        }
        else if (isBreathing && elapsed >= 1.0f)
        {
            isBreathing = false;
            archDemon?.StopDarkBreath();
        }

        if (triggerCalled || stateTimer <= 0f)
        {
            archDemon?.StopDarkBreath();
            archDemon?.SetSpellcastOnCooldown();
            stateMachine.ChangeState(archDemon.archDemonBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        isBreathing = false;
        archDemon?.ForceKillDarkBreath();

        if (archDemon != null)
            archDemon.hasHyperArmor = false;

        archDemon?.SetSpellcastOnCooldown();
    }
}
