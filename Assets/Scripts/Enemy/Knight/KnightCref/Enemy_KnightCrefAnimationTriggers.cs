using UnityEngine;

public class Enemy_KnightCrefAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_KnightCref knightCref;

    protected override void Awake()
    {
        base.Awake();
        knightCref = GetComponentInParent<Enemy_KnightCref>();
    }

    private void AttackDamageTrigger()
    {
        knightCref?.PerformAttackDamage();
    }

    private void HeavySlamTrigger()
    {
        knightCref?.PerformHeavySlamDamage();
    }
}
