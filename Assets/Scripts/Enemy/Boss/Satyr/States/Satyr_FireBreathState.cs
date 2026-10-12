using UnityEngine;

public class Satyr_FireBreathState : EnemyState
{
    private Enemy_Satyr satyr;
    private float breathTimer = 0f;
    private bool isBreathing = false;
    private bool breathExtinguished = false;
    private const float postBreathRecoveryDuration = 0.45f; // Thời gian đứng yên thu thế sau khi tắt lửa mới được phép Dash

    public Satyr_FireBreathState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        satyr = enemy as Enemy_Satyr;
    }

    public override void Enter()
    {
        base.Enter();

        // Grant Hyper Armor during active fire breath so flame cannot be interrupted
        if (satyr != null)
            satyr.hasHyperArmor = true;

        enemy.SetVelocity(0f, 0f);

        // Turn towards player before locking direction
        Transform player = satyr.GetPlayerTarget();
        if (player != null)
        {
            int dir = player.position.x > enemy.transform.position.x ? 1 : -1;
            enemy.HandleFlip(dir);
        }

        breathTimer = 0f;
        isBreathing = false;
        breathExtinguished = false;
    }

    public void OnFireBreathStartTrigger()
    {
        if (isBreathing || breathExtinguished)
            return;

        isBreathing = true;
        satyr?.StartFireBreathEmitter();
    }

    public void OnFireBreathEndTrigger()
    {
        ExtinguishFlame();
    }

    private void ExtinguishFlame()
    {
        if (breathExtinguished)
            return;

        breathExtinguished = true;
        isBreathing = false;

        // Tắt effect phun lửa và hitbox triệt để ngay lập tức
        satyr?.StopFireBreathEmitter();

        // Tắt siêu giáp khi lửa đã tắt để người chơi có cửa sổ vàng đánh trả
        if (satyr != null)
            satyr.hasHyperArmor = false;
    }

    public override void Update()
    {
        base.Update();

        // Đứng yên hoàn toàn cả trong lúc phun lửa lẫn lúc thu thế hồi phục
        enemy.SetVelocity(0f, enemy.rb.linearVelocity.y);
        breathTimer += Time.deltaTime;

        // Lửa do event FireBreathStartTrigger (0.42s) trong anim kích hoạt sau nhịp gồng.
        // Fallback 0.5s chỉ dùng khi event bị rớt, không được chạy trước event.
        if (!isBreathing && !breathExtinguished && breathTimer >= 0.5f)
        {
            OnFireBreathStartTrigger();
        }

        // Khi hết thời gian phun lửa: TẮT EFFECT LỬA NGAY, bước vào giai đoạn thu thế (Recovery)
        if (!breathExtinguished && breathTimer >= satyr.FireBreathDuration)
        {
            ExtinguishFlame();
        }

        // Đứng yên thu thế thêm 0.45s sau khi lửa đã tắt hẳn rồi mới cho phép thoát state để Dash!
        if (breathTimer >= satyr.FireBreathDuration + postBreathRecoveryDuration)
        {
            satyr?.SetFireBreathOnCooldown();
            stateMachine.ChangeState(satyr.satyrBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        ExtinguishFlame();

        if (satyr != null)
            satyr.hasHyperArmor = false;

        // Cưỡng chế dọn sạch mọi tàn dư effect phòng ngừa lỗi
        satyr?.ForceKillFireBreathEmitter();
        satyr?.SetFireBreathOnCooldown();
    }
}
