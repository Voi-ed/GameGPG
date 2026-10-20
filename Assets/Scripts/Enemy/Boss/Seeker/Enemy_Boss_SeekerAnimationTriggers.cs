using UnityEngine;

/// <summary>
/// Bắt các Animation Event từ Animation Clips của Boss The Void Seeker
/// </summary>
public class Enemy_Boss_SeekerAnimationTriggers : Enemy_AnimationTriggers
{
    private Enemy_Boss_Seeker seeker;

    protected override void Awake()
    {
        base.Awake();
        seeker = GetComponentInParent<Enemy_Boss_Seeker>();
    }

    private void AttackTrigger()
    {
        if (seeker != null)
        {
            seeker.PerformVoidSlash();
        }
    }
}
