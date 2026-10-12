using UnityEngine;

public class Enemy_MartialHeroAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_MartialHero martialHero;

    protected override void Awake()
    {
        base.Awake();
        martialHero = GetComponentInParent<Enemy_MartialHero>();
    }

    private void Attack1DamageTrigger()
    {
        martialHero?.PerformAttack1Damage();
    }

    private void Attack2DamageTrigger()
    {
        martialHero?.PerformAttack2Damage();
    }

    private void ComboLungeTrigger()
    {
        martialHero?.LungeForward();
    }
}
