using UnityEngine;

public class Seeker_BattleState : Enemy_BattleState
{
    private Enemy_Boss_Seeker seeker;

    public Seeker_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        seeker = enemy as Enemy_Boss_Seeker;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && seeker != null)
            player = seeker.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

        if (player == null)
        {
            player = seeker.GetPlayerTarget();
            if (player == null)
            {
                stateMachine.ChangeState(enemy.idleState);
                return;
            }
        }

        float distX = Mathf.Abs(player.position.x - enemy.transform.position.x);
        float distance = Vector2.Distance(enemy.transform.position, player.position);
        int dirToPlayer = DirectionToPlayer();
        enemy.HandleFlip(dirToPlayer);

        // 1. Kỹ năng Void Leap & Plunge (tầm xa và trung)
        if (seeker.CanJumpAttack() && distance >= 3.5f && distance <= 10.0f)
        {
            stateMachine.ChangeState(seeker.seekerJumpAttackState);
            return;
        }

        // 2. Kỹ năng Phantom Shadow Step (Dash)
        if (seeker.CanDash())
        {
            if (distance >= 3.0f)
            {
                // Dash áp sát
                seeker.seekerDashState.SetDashDirection(dirToPlayer);
                stateMachine.ChangeState(seeker.seekerDashState);
                return;
            }
            else if (distX < 1.2f)
            {
                // Dash lùi né tránh nếu bị ép sát
                seeker.seekerDashState.SetDashDirection(-dirToPlayer);
                stateMachine.ChangeState(seeker.seekerDashState);
                return;
            }
        }

        // 3. Xử lý khoảng cách quá gần (< 1.2m): tránh đứng đè trùng lên người chơi
        if (distX < 1.2f)
        {
            if (seeker.CanAttack())
            {
                stateMachine.ChangeState(seeker.seekerAttackState);
                return;
            }
            else
            {
                // Nhích lùi nhẹ giữ khoảng cách chiến đấu
                enemy.SetVelocity(-dirToPlayer * (seeker.RunSpeed * 0.4f), rb.linearVelocity.y);
                return;
            }
        }

        // 4. Trong tầm tấn công cận chiến Void Slash
        if (distX <= seeker.AttackRange)
        {
            if (seeker.CanAttack())
            {
                stateMachine.ChangeState(seeker.seekerAttackState);
                return;
            }
            else
            {
                // Đã vào tầm đánh nhưng đang chờ hồi chiêu:
                // Dừng lại giữ cự ly offset, không húc đè thẳng vào người chơi
                enemy.SetVelocity(0f, rb.linearVelocity.y);
                return;
            }
        }

        // 5. Di chuyển tiếp cận người chơi khi ở ngoài tầm đánh
        enemy.SetVelocity(dirToPlayer * seeker.RunSpeed, rb.linearVelocity.y);
    }
}
