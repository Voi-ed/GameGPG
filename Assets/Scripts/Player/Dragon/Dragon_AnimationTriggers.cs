using UnityEngine;

/// <summary>
/// Gắn trên Child GameObject "Animator" của Dragon (hoặc Dragon_Avatar của Player).
/// Kế thừa trực tiếp từ Entity_AnimationTriggers để tuân thủ 100% kiến trúc chuẩn của dự án.
/// Bắt Animation Events AttackTrigger và CurrentStateTrigger từ animation clips.
/// </summary>
public class Dragon_AnimationTriggers : Entity_AnimationTriggers
{
    private Player_DragonForm dragonForm;
    private Dragon_Controller dragonController;

    protected override void Awake()
    {
        base.Awake();
        dragonForm = GetComponentInParent<Player_DragonForm>();
        dragonController = GetComponentInParent<Dragon_Controller>();
    }

    /// <summary>
    /// Được gọi bởi Animation Event 'AttackTrigger' tại frame cào vuốt (Frame 5) hoặc rồng gầm nộ (Frame 17).
    /// </summary>
    protected override void AttackTrigger()
    {
        if (dragonForm != null && dragonForm.IsDragonForm)
        {
            if (dragonForm.IsCastingFireball)
                dragonForm.ExecuteDragonFireball();
            else if (dragonForm.IsCastingSkill)
                dragonForm.ExecuteDragonSkill();
            else
                dragonForm.ExecuteClawAttack();
        }
        else if (dragonController != null)
        {
            dragonController.AttackTrigger();
        }
        else
        {
            base.AttackTrigger();
        }
    }

    /// <summary>
    /// Được gọi bởi Animation Event 'CurrentStateTrigger' tại frame thu đòn đánh về (Frame 9).
    /// </summary>
    protected override void CurrentStateTrigger()
    {
        if (dragonForm != null && dragonForm.IsDragonForm)
        {
            dragonForm.OnAttackAnimationCompleted();
        }
        else if (dragonController != null)
        {
            dragonController.AnimationTrigger();
        }
        else
        {
            base.CurrentStateTrigger();
        }
    }
}
