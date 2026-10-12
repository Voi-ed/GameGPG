using UnityEngine;

public class Enemy_ArchDemonAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_ArchDemon archDemon;

    protected override void Awake()
    {
        base.Awake();
        archDemon = GetComponentInParent<Enemy_ArchDemon>();
    }

    public void DarkBreathStartTrigger()
    {
        archDemon?.StartDarkBreath();
    }

    public void DarkBreathEndTrigger()
    {
        archDemon?.StopDarkBreath();
    }

    private void FireDarkOrbTrigger()
    {
        archDemon?.FireDarkOrbAtPlayer();
    }
}
