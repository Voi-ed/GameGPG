using UnityEngine;

public class Enemy_Boss_CrimsonBladeAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_Boss_CrimsonBlade crimsonBlade;

    protected override void Awake()
    {
        base.Awake();
        crimsonBlade = GetComponentInParent<Enemy_Boss_CrimsonBlade>();
    }

    protected override void AttackTrigger()
    {
        if (crimsonBlade != null && crimsonBlade.stateMachine.currentState == crimsonBlade.crimsonComboAttackState)
        {
            crimsonBlade.PerformComboSlash();
        }
        else
        {
            base.AttackTrigger();
        }
    }

    /// <summary>
    /// Event riêng biệt cho chiêu lướt chém Blink Strike trong animation crimsonBlinkSlash
    /// </summary>
    public void BlinkSlashTrigger()
    {
        crimsonBlade?.PerformBlinkSlash();
    }
}
