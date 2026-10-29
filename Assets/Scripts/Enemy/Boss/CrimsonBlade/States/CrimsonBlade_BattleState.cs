using UnityEngine;

public class CrimsonBlade_BattleState : Enemy_BattleState
{
    private Enemy_Boss_CrimsonBlade crimsonBlade;

    public CrimsonBlade_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        crimsonBlade = enemy as Enemy_Boss_CrimsonBlade;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null && crimsonBlade != null)
            player = crimsonBlade.GetPlayerTarget();

        if (player != null)
            enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

        if (enemy.PlayerDetected())
            UpdateTargetIfNeeded();

        if (player == null)
        {
            player = crimsonBlade.GetPlayerTarget();
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

        // 1. Kỹ năng Blink Strike (Lướt chém xuyên thấu khi ở cự ly tầm trung/xa)
        if (crimsonBlade.CanBlink() && distance >= 3.0f && distance <= 9.0f)
        {
            stateMachine.ChangeState(crimsonBlade.crimsonBlinkSlashState);
            return;
        }

        // 2. Kỹ năng Blood Guard (Thế thủ phản đòn khi ở cự ly gần)
        if (crimsonBlade.CanParry() && distX <= 2.2f && Random.value < 0.35f)
        {
            stateMachine.ChangeState(crimsonBlade.crimsonParryStanceState);
            return;
        }

        // 3. Trong tầm tấn công cận chiến Song Kích Trảm
        if (distX <= crimsonBlade.ComboAttackRange)
        {
            if (crimsonBlade.CanAttack())
            {
                stateMachine.ChangeState(crimsonBlade.crimsonComboAttackState);
                return;
            }
            else
            {
                // Đang trong tầm đánh chờ hồi chiêu: Đứng vững tại chỗ thủ thế, không lùi bước
                enemy.SetVelocity(0f, rb.linearVelocity.y);
                return;
            }
        }

        // 5. Di chuyển tiếp cận người chơi khi ở ngoài tầm đánh
        enemy.SetVelocity(dirToPlayer * crimsonBlade.RunSpeed, rb.linearVelocity.y);
    }
}
