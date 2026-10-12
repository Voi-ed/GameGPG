using UnityEngine;

public class Enemy_SamuraiZippAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_SamuraiZipp samuraiZipp;

    protected override void Awake()
    {
        base.Awake();
        samuraiZipp = GetComponentInParent<Enemy_SamuraiZipp>();
    }

    private void Attack1DamageTrigger() => samuraiZipp?.PerformAttack1Damage();
    private void Attack2DamageTrigger() => samuraiZipp?.PerformAttack2Damage();
    private void ThrustDamageTrigger() => samuraiZipp?.PerformThrustDamage();
    private void ComboLungeTrigger() => samuraiZipp?.LungeForward();
}
