using UnityEngine;

public class Enemy_KnightMageAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_KnightMage enemyKnightMage;

    protected override void Awake()
    {
        base.Awake();
        enemyKnightMage = GetComponentInParent<Enemy_KnightMage>();
    }

    private void TeleportTrigger()
    {
        enemyKnightMage?.SetTeleportTrigger(true);
    }

    private void ThornsDamageTrigger()
    {
        enemyKnightMage?.PerformThornsDamage();
    }
}
