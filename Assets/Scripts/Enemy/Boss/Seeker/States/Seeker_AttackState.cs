using UnityEngine;

public class Seeker_AttackState : EnemyState
{
    private Enemy_Boss_Seeker seeker;
    private int comboCount = 0;
    private const int MaxComboCount = 3; // Chuỗi 3 nhát chém liên hoàn Void Slash
    private float comboNextSwingTimer = 0f;
    private bool isWaitingNextSwing = false;

    public Seeker_AttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        seeker = enemy as Enemy_Boss_Seeker;
    }

    public override void Enter()
    {
        base.Enter();

        comboCount = 1;
        isWaitingNextSwing = false;
        PerformSlashLunge();
    }

    private void PerformSlashLunge()
    {
        triggerCalled = false;
        isWaitingNextSwing = false;

        Transform player = seeker.GetPlayerTarget();
        if (player != null)
        {
            int dir = (player.position.x > enemy.transform.position.x) ? 1 : -1;
            enemy.HandleFlip(dir);

            float distX = Mathf.Abs(player.position.x - enemy.transform.position.x);
            // Nhích nhẹ áp sát nếu cự ly còn xa, nếu đã sát mục tiêu thì đứng yên chém
            if (distX > 1.4f)
            {
                enemy.SetVelocity(dir * 1.6f, rb.linearVelocity.y);
            }
            else
            {
                enemy.SetVelocity(0f, rb.linearVelocity.y);
            }
        }
        else
        {
            enemy.SetVelocity(0f, rb.linearVelocity.y);
        }
    }

    public override void Update()
    {
        base.Update();

        // Giảm tốc độ trượt khi đang vung kiếm
        enemy.SetVelocity(Mathf.MoveTowards(rb.linearVelocity.x, 0f, 10f * Time.deltaTime), rb.linearVelocity.y);

        // Khi kết thúc nhát chém hiện tại
        if (triggerCalled && !isWaitingNextSwing)
        {
            Transform player = seeker.GetPlayerTarget();
            float distX = player != null ? Mathf.Abs(player.position.x - enemy.transform.position.x) : float.MaxValue;

            // Nếu người chơi vẫn trong tầm đánh và chưa hoàn thành chuỗi 3 nhát chém
            if (comboCount < MaxComboCount && distX <= seeker.AttackRange + 0.4f)
            {
                isWaitingNextSwing = true;
                comboNextSwingTimer = 0.08f; // Nhịp nối combo chớp nhoáng
            }
            else
            {
                // Đã chém xong trọn vẹn chuỗi combo (hoặc người chơi đã né thoát) -> sang trạng thái nghỉ/hồi chiêu
                seeker.RecordAttackTime();
                stateMachine.ChangeState(seeker.seekerBattleState);
            }
        }

        // Chờ nhịp cực ngắn để vung tiếp nhát chém thứ 2, thứ 3
        if (isWaitingNextSwing)
        {
            comboNextSwingTimer -= Time.deltaTime;
            if (comboNextSwingTimer <= 0f)
            {
                comboCount++;
                isWaitingNextSwing = false;

                // Reset và chơi lại anim chém cho đòn tiếp theo
                anim.Play("attack", 0, 0f);
                PerformSlashLunge();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetVelocity(0f, rb.linearVelocity.y);
        seeker.RecordAttackTime();
    }
}
