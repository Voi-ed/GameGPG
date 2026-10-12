using UnityEngine;

public class Enemy_SeveredFangAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_SeveredFang severedFang;

    protected override void Awake()
    {
        base.Awake();
        severedFang = GetComponentInParent<Enemy_SeveredFang>();
    }

    private void SlashDamageTrigger()
    {
        severedFang?.PerformSlashDamage();
    }

    private void IaidoDamageTrigger()
    {
        severedFang?.PerformIaidoDamage();
    }

    private void OpenCounterWindowTrigger()
    {
        severedFang?.SetCounterWindow(true);
    }

    private void CloseCounterWindowTrigger()
    {
        severedFang?.SetCounterWindow(false);
    }
}
