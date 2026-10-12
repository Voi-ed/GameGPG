using UnityEngine;

public class Enemy_SatyrAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_Satyr satyr;

    protected override void Awake()
    {
        base.Awake();
        satyr = GetComponentInParent<Enemy_Satyr>();
    }

    public void FireBreathStartTrigger()
    {
        if (satyr != null && satyr.satyrFireBreathState != null)
            satyr.satyrFireBreathState.OnFireBreathStartTrigger();
        else
            satyr?.StartFireBreathEmitter();
    }

    public void FireBreathEndTrigger()
    {
        if (satyr != null && satyr.satyrFireBreathState != null)
            satyr.satyrFireBreathState.OnFireBreathEndTrigger();
        else
            satyr?.StopFireBreathEmitter();
    }

    public void EarthSlamImpactTrigger()
    {
        // Skill 3 removed
    }

    public void SlideSpawnFireTrigger()
    {
        // Skill 4 removed
    }

    private void EnableCounterWindow()
    {
        satyr?.EnableCounterWindow(true);
    }

    private void DisableCounterWindow()
    {
        satyr?.EnableCounterWindow(false);
    }
}
