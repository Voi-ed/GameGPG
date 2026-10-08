using UnityEngine;

public class Enemy_BlackKnightAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_BlackKnight blackKnight;

    protected override void Awake()
    {
        base.Awake();
        blackKnight = GetComponentInParent<Enemy_BlackKnight>();
    }

    private void ComboHitTrigger()
    {
        blackKnight?.OnComboHitTrigger();
    }

    private void HeavyHitTrigger()
    {
        blackKnight?.OnHeavyHitTrigger();
    }

    private void RiposteHitTrigger()
    {
        blackKnight?.OnRiposteHitTrigger();
    }

    private void WeaponArtHitTrigger()
    {
        blackKnight?.OnWeaponArtHitTrigger();
    }

    private void EnrageBuffTrigger()
    {
        blackKnight?.OnEnrageBuffTrigger();
    }

    private void RollEndTrigger()
    {
        blackKnight?.OnRollEndTrigger();
    }

    private void ParryWindowStartTrigger()
    {
        blackKnight?.OnParryWindowStartTrigger();
    }

    private void ParryWindowEndTrigger()
    {
        blackKnight?.OnParryWindowEndTrigger();
    }
}
