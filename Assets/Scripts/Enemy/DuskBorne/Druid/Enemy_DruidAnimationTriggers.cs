using UnityEngine;

public class Enemy_DruidAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_Druid druid;

    protected override void Awake()
    {
        base.Awake();
        druid = GetComponentInParent<Enemy_Druid>();
    }

    private void CastSpikeTrigger()
    {
        druid?.SpawnEarthSpikeUnderPlayer();
    }
}
