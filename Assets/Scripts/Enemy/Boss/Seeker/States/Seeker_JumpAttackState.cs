using UnityEngine;

public class Seeker_JumpAttackState : EnemyState
{
    private Enemy_Boss_Seeker seeker;
    private bool isPlunging = false;
    private bool hasLanded = false;
    private float landTimer = 0f;
    private int dirToPlayer = 1;

    public Seeker_JumpAttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        seeker = enemy as Enemy_Boss_Seeker;
    }

    public override void Enter()
    {
        base.Enter();

        seeker.hasHyperArmor = true;
        isPlunging = false;
        hasLanded = false;
        landTimer = 0.25f;

        Transform player = seeker.GetPlayerTarget();
        if (player != null)
        {
            dirToPlayer = (player.position.x > enemy.transform.position.x) ? 1 : -1;
        }

        enemy.HandleFlip(dirToPlayer);

        // Bật nhảy vọt lên không trung
        enemy.SetVelocity(dirToPlayer * 5f, seeker.LeapUpVelocity);
        stateTimer = 0.45f;
    }

    public override void Update()
    {
        base.Update();

        if (!isPlunging)
        {
            // Đang bay lên, chờ đạt đỉnh
            if (stateTimer <= 0f || rb.linearVelocity.y <= 0f)
            {
                isPlunging = true;

                // Cập nhật lại vị trí người chơi trước khi bổ kiếm xuống
                Transform player = seeker.GetPlayerTarget();
                if (player != null)
                {
                    dirToPlayer = (player.position.x > enemy.transform.position.x) ? 1 : -1;
                    enemy.HandleFlip(dirToPlayer);
                }

                // Lao thẳng xuống mặt đất
                enemy.SetVelocity(dirToPlayer * 6f, seeker.PlungeDownVelocity);
            }
        }
        else if (!hasLanded)
        {
            // Đang lao xuống, kiểm tra tiếp đất
            enemy.SetVelocity(dirToPlayer * 6f, seeker.PlungeDownVelocity);

            if (enemy.groundDetected && rb.linearVelocity.y <= 0.1f)
            {
                hasLanded = true;
                enemy.SetVelocity(0f, 0f);

                // Gây chấn động Hư Không diện rộng
                seeker.PerformPlungeImpact();
            }
        }
        else
        {
            // Đã tiếp đất, khựng lại một khoảnh khắc rồi chuyển trạng thái
            enemy.SetVelocity(0f, 0f);
            landTimer -= Time.deltaTime;
            if (landTimer <= 0f)
            {
                stateMachine.ChangeState(seeker.seekerBattleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();

        seeker.hasHyperArmor = false;
        seeker.RecordJumpAttackTime();
        enemy.SetVelocity(0f, rb.linearVelocity.y);
    }
}
