using UnityEngine;

public class MartialHero_BattleState : Enemy_BattleState
{
    private Enemy_MartialHero martialHero;

    public MartialHero_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        martialHero = enemy as Enemy_MartialHero;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && martialHero != null)
            player = martialHero.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;

        if (player == null && martialHero != null)
            player = martialHero.GetPlayerTarget();

        float distToPlayer = DistanceToPlayer();

        // Luôn duy trì Battle Timer nếu Player còn trong phạm vi chiến đấu (10m)
        if (player != null && distToPlayer <= 10f)
        {
            UpdateBattleTimer();
        }
        else if (enemy.PlayerDetected())
        {
            UpdateTargetIfNeeded();
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        if (player == null || martialHero == null)
            return;

        enemy.HandleFlip(DirectionToPlayer());

        // Cập nhật trạng thái Phase 2 nếu máu < 50%
        martialHero.UpdateBossPhase();

        // 1. Kỹ năng Tầm Trung/Xa: Nhất Kiếm Bạt Kích (Flash Step Dash)
        if (distToPlayer >= martialHero.FlashStepMinDistance && distToPlayer <= martialHero.FlashStepMaxDistance && martialHero.CanFlashStep())
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            stateMachine.ChangeState(martialHero.martialHeroFlashStepState);
            return;
        }

        // 2. Kỹ năng Tầm Xa: Kiếm Khí Trảm (Crescent Sword Wave)
        if (distToPlayer >= martialHero.SwordWaveMinDistance && martialHero.CanSwordWave())
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            stateMachine.ChangeState(martialHero.martialHeroSwordWaveState);
            return;
        }

        // 3. Tấn công cận chiến ngay khi vào tầm chém và đã hồi chiêu xong
        if (distToPlayer <= martialHero.Attack1Distance && martialHero.CanAttack())
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            stateMachine.ChangeState(martialHero.martialHeroAttackState);
            return;
        }

        // 4. Trong tầm chém nhưng đang hồi chiêu (nhịp thở 0.6s):
        // Đứng thủ thế (Idle) đối mặt Player, sẵn sàng cho nhịp vung kiếm tiếp theo
        if (distToPlayer <= martialHero.Attack1Distance)
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            anim.SetBool("run", false);
            anim.SetBool("idle", true);
            return;
        }

        // 5. Ngoài tầm chém -> Chạy áp sát Player
        float runSpeed = martialHero.BattleRunSpeed;
        enemy.SetVelocity(runSpeed * DirectionToPlayer(), rb.linearVelocity.y);
        anim.SetBool("idle", false);
        anim.SetBool("run", true);
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("run", false);
        anim.SetBool("idle", false);
    }
}
