using UnityEngine;

public class Enemy_SicklezeepAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_Sicklezeep sicklezeep;

    protected override void Awake()
    {
        base.Awake();
        sicklezeep = GetComponentInParent<Enemy_Sicklezeep>();
    }

    private void Attack1DamageTrigger() => sicklezeep?.PerformAttack1Damage();
    private void Attack2DamageTrigger() => sicklezeep?.PerformAttack2Damage();
    private void SweepDamageTrigger() => sicklezeep?.PerformSweepDamage();
    private void ComboLungeTrigger() => sicklezeep?.LungeForward();
}
