using UnityEngine;

public class Enemy_WanderingKnightAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_WanderingKnight wanderingKnight;

    protected override void Awake()
    {
        base.Awake();
        wanderingKnight = GetComponentInParent<Enemy_WanderingKnight>();
    }

    private void ComboHitTrigger()
    {
        wanderingKnight?.OnComboHitTrigger();
    }

    private void ThrustHitTrigger()
    {
        wanderingKnight?.OnThrustHitTrigger();
    }

    private void AirSlashHitTrigger()
    {
        wanderingKnight?.OnAirSlashHitTrigger();
    }

    private void HealTrigger()
    {
        wanderingKnight?.OnHealTrigger();
    }

    private void PrayBuffTrigger()
    {
        wanderingKnight?.OnPrayBuffTrigger();
    }

    private void RollEndTrigger()
    {
        wanderingKnight?.OnRollEndTrigger();
    }

    private void AnimationTrigger()
    {
        wanderingKnight?.CurrentStateAnimationTrigger();
    }
}
