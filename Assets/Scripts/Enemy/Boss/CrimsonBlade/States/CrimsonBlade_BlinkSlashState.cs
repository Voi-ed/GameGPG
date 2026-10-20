using UnityEngine;

public class CrimsonBlade_BlinkSlashState : EnemyState
{
    private Enemy_Boss_CrimsonBlade crimsonBlade;
    private int blinkDir = 1;
    private bool hasDealtDamage;

    public CrimsonBlade_BlinkSlashState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        crimsonBlade = enemy as Enemy_Boss_CrimsonBlade;
    }

    public override void Enter()
    {
        base.Enter();

        hasDealtDamage = false;
        stateTimer = crimsonBlade.BlinkDuration; // 0.35s
        crimsonBlade.RecordBlinkTime();

        Transform player = crimsonBlade.GetPlayerTarget();
        if (player != null)
        {
            blinkDir = (player.position.x > enemy.transform.position.x) ? 1 : -1;
            enemy.HandleFlip(blinkDir);
        }
        else
        {
            blinkDir = enemy.facingDir;
        }

        // Bật Hyper Armor và chuỗi bóng ma dư ảnh
        crimsonBlade.hasHyperArmor = true;
        enemy.vfx?.DoImageEchoEffect(crimsonBlade.BlinkDuration);

        // PHÓNG VÚT ĐI NGAY LẬP TỨC: Tuyệt đối không đứng yên khựng lại dù chỉ 1 frame
        enemy.SetVelocity(blinkDir * crimsonBlade.BlinkSpeed, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // Duy trì vận tốc lao thần tốc xé gió về phía mục tiêu
        enemy.SetVelocity(blinkDir * crimsonBlade.BlinkSpeed, rb.linearVelocity.y);

        Transform player = crimsonBlade.GetPlayerTarget();
        float distX = player != null ? Mathf.Abs(player.position.x - enemy.transform.position.x) : float.MaxValue;

        // Quét gây sát thương bằng hình chữ nhật phía trước mặt của Blink Slash
        if (!hasDealtDamage)
        {
            Collider2D hit = Physics2D.OverlapBox(crimsonBlade.BlinkSlashCheckPosition, crimsonBlade.BlinkSlashBoxSize, 0f, crimsonBlade.WhatIsPlayer);
            if (hit != null)
            {
                hasDealtDamage = true;
                crimsonBlade.PerformBlinkSlash(hit.transform);
                enemy.vfx?.CreateOnHitVFX(hit.transform, false, ElementType.Fire);
            }
        }

        // Điều kiện hoàn thành cú lao:
        // Đã lao đến sát trước mặt Player (cự ly 1.3m), hoặc chạm tường, hoặc hết thời gian lao, hoặc animation kết thúc
        bool reachedPlayer = (player != null && distX <= 1.3f);
        bool hitWall = enemy.wallDetected;
        bool timeOver = stateTimer <= 0f;

        if (reachedPlayer || hitWall || timeOver || triggerCalled)
        {
            FinishBlinkSlash();
        }
    }

    private void FinishBlinkSlash()
    {
        // Khóa chặt vận tốc ngay tại vị trí vừa lao tới
        enemy.SetVelocity(0f, rb.linearVelocity.y);

        // Đảm bảo quét sát thương lần chót tại vùng hình chữ nhật phía trước nếu chưa gây sát thương
        if (!hasDealtDamage)
        {
            Collider2D hit = Physics2D.OverlapBox(crimsonBlade.BlinkSlashCheckPosition, crimsonBlade.BlinkSlashBoxSize, 0f, crimsonBlade.WhatIsPlayer);
            if (hit != null)
            {
                hasDealtDamage = true;
                crimsonBlade.PerformBlinkSlash(hit.transform);
                enemy.vfx?.CreateOnHitVFX(hit.transform, false, ElementType.Fire);
            }
        }

        // Khi lao đến nơi, nếu đang ở ngay trước mặt mục tiêu và đòn chém sẵn sàng -> Nối combo chém ngay lập tức!
        Transform target = crimsonBlade.GetPlayerTarget();
        if (target != null)
        {
            float dist = Mathf.Abs(target.position.x - enemy.transform.position.x);
            if (dist <= crimsonBlade.ComboAttackRange && crimsonBlade.CanAttack())
            {
                stateMachine.ChangeState(crimsonBlade.crimsonComboAttackState);
                return;
            }
        }

        // Nếu đang hồi chiêu chém: đứng yên thủ thế ngay tại đúng vị trí vừa lao đến trong BattleState
        stateMachine.ChangeState(crimsonBlade.crimsonBattleState);
    }

    public override void Exit()
    {
        base.Exit();
        crimsonBlade.hasHyperArmor = false;
        enemy.SetVelocity(0f, rb.linearVelocity.y);
    }
}
