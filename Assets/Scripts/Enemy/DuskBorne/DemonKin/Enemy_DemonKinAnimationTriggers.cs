using UnityEngine;

public class Enemy_DemonKinAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_DemonKin demonKin;

    protected override void Awake()
    {
        base.Awake();
        demonKin = GetComponentInParent<Enemy_DemonKin>();
    }

    private void SmashDamageTrigger()
    {
        demonKin?.PerformSmashDamage();
    }
}
