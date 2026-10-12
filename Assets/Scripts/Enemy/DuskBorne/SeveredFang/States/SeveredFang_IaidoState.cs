using UnityEngine;

public class SeveredFang_IaidoState : EnemyState
{
    private Enemy_SeveredFang severedFang;
    private enum IaidoPhase { Sheathing, Slashing, Recovery }
    private IaidoPhase phase;
    private float phaseTimer;
    private int slashDirection;
    private bool damageApplied = false;

    public SeveredFang_IaidoState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        severedFang = enemy as Enemy_SeveredFang;
    }

    public override void Enter()
    {
        base.Enter();

        phase = IaidoPhase.Sheathing;
        phaseTimer = severedFang.SheatheDuration;
        damageApplied = false;

        enemy.SetVelocity(0f, rb.linearVelocity.y);

        Transform player = severedFang != null ? severedFang.GetPlayerTarget() : null;
        if (player != null)
        {
            slashDirection = player.position.x > enemy.transform.position.x ? 1 : -1;
            enemy.HandleFlip(slashDirection);
        }
        else
        {
            slashDirection = enemy.facingDir;
        }

        // Bật cửa sổ phản đòn (Dấu cảnh báo đỏ trên đầu) trong suốt thời gian tra kiếm
        severedFang?.SetCounterWindow(true);
    }

    public override void Update()
    {
        base.Update();

        phaseTimer -= Time.deltaTime;

        switch (phase)
        {
            case IaidoPhase.Sheathing:
                enemy.SetVelocity(0f, rb.linearVelocity.y);

                if (phaseTimer <= 0f)
                {
                    // Chuyển sang giai đoạn Lướt chém chớp nhoáng (Flash Slash)
                    severedFang?.SetCounterWindow(false);
                    phase = IaidoPhase.Slashing;
                    phaseTimer = severedFang.DashDuration;

                    // Khóa hướng lướt về phía mục tiêu
                    Transform player = severedFang != null ? severedFang.GetPlayerTarget() : null;
                    if (player != null)
                    {
                        slashDirection = player.position.x > enemy.transform.position.x ? 1 : -1;
                        enemy.HandleFlip(slashDirection);
                    }
                }
                break;

            case IaidoPhase.Slashing:
                enemy.SetVelocity(severedFang.DashSpeed * slashDirection, rb.linearVelocity.y);

                if (!damageApplied)
                {
                    damageApplied = true;
                    severedFang?.PerformIaidoDamage();
                }

                if (phaseTimer <= 0f || enemy.wallDetected)
                {
                    // Kết thúc lướt, khựng lại thu thế
                    phase = IaidoPhase.Recovery;
                    phaseTimer = 0.25f;
                    enemy.SetVelocity(0f, rb.linearVelocity.y);
                }
                break;

            case IaidoPhase.Recovery:
                enemy.SetVelocity(0f, rb.linearVelocity.y);

                if (phaseTimer <= 0f)
                {
                    severedFang?.SetIaidoOnCooldown();
                    stateMachine.ChangeState(severedFang.severedFangBattleState);
                }
                break;
        }
    }

    public override void Exit()
    {
        base.Exit();

        severedFang?.SetCounterWindow(false);
        severedFang?.SetIaidoOnCooldown();
    }
}
