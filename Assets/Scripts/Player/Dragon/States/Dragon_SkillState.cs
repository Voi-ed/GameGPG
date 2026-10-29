using UnityEngine;

/// <summary>
/// Trạng thái thi triển kỹ năng "Long Nộ Địa Ngục" (Hellfire Roar) cho Dragon_Controller:
/// - Rồng dừng lại, gồng mình và há to miệng gầm thét dữ dội (Dragon_Roar).
/// - Tại frame 17 (AttackTrigger), kích hoạt sinh ra Cột Lửa Địa Ngục Vfx_Dragon_HellfirePillar.
/// - Tại frame 29 (CurrentStateTrigger), rồng thu người về thế đứng 4 chân và chuyển về idleState.
/// </summary>
public class Dragon_SkillState : DragonState
{
    public Dragon_SkillState(Dragon_Controller dragon, StateMachine stateMachine, string animBoolName)
        : base(dragon, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // Ghim vận tốc X về 0 để đứng yên vững chãi khi gầm nộ
        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        anim.SetBool("isCastingSkill", true);
    }

    public override void Update()
    {
        base.Update();

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // Khi kết thúc hoạt ảnh gầm nộ (CurrentStateTrigger) -> chuyển về Idle
        if (triggerCalled)
        {
            stateMachine.ChangeState(dragon.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("isCastingSkill", false);
    }
}
