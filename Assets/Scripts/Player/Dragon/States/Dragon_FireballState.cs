using UnityEngine;

/// <summary>
/// Trạng thái thi triển kỹ năng "Hắc Hỏa Cầu" (Left Shift) cho Dragon_Controller:
/// - Pha 1: Rồng hạ rạp thân nhanh xuống đất để ghì vững bệ đỡ, tụ hỏa khí tại miệng (Vfx_Dragon_FireCharge).
/// - Pha 2: Gồng nén năng lượng, sau đó khạc bắn quả cầu lửa tại AttackTrigger.
/// - Pha 3: Lực giật lùi (Recoil) đẩy nhẹ thân rồng về phía sau tạo cảm giác uy lực cực lớn.
/// - Pha 4: Rồng đẩy người đứng thẳng dậy hùng dũng và chuyển mượt về idleState.
/// </summary>
public class Dragon_FireballState : DragonState
{
    private float recoilTimer = 0f;
    private const float RECOIL_DURATION = 0.16f;
    private const float RECOIL_VELOCITY = 3.8f;
    private bool recoilTriggered = false;

    public Dragon_FireballState(Dragon_Controller dragon, StateMachine stateMachine, string animBoolName)
        : base(dragon, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        recoilTriggered = false;
        recoilTimer = 0f;

        // Ghim vận tốc X về 0 để Rồng hạ thân tụ khí vững chãi
        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        anim.SetBool("isCastingSkill", true);
    }

    public override void Update()
    {
        base.Update();

        // Xử lý lực giật lùi (Recoil)
        if (recoilTriggered)
        {
            recoilTimer -= Time.deltaTime;
            if (recoilTimer <= 0f && rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                recoilTriggered = false;
            }
        }
        else
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // Khi kết thúc chuỗi đứng dậy (CurrentStateTrigger) -> chuyển về Idle
        if (triggerCalled)
        {
            stateMachine.ChangeState(dragon.idleState);
        }
    }

    /// <summary>
    /// Được gọi bởi Dragon_Controller khi AttackTrigger kích hoạt để tạo độ giật lùi cho Rồng
    /// </summary>
    public void ApplyRecoil()
    {
        if (rb != null)
        {
            recoilTriggered = true;
            recoilTimer = RECOIL_DURATION;
            rb.linearVelocity = new Vector2(-dragon.facingDir * RECOIL_VELOCITY, rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        recoilTriggered = false;
        anim.SetBool("isCastingSkill", false);
    }
}
