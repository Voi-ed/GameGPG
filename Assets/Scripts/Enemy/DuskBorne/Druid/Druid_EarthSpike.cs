using UnityEngine;

public class Druid_EarthSpike : MonoBehaviour
{
    [Header("Spike Settings")]
    [SerializeField] private float damageRadius = 1.2f;
    [SerializeField] private LayerMask whatIsTarget;
    [SerializeField] private float eruptionDelay = 0.45f;
    [SerializeField] private float lifetime = 1.0f;

    private Enemy owner;
    private DamageScaleData damageScale;
    private bool hasErupted = false;
    private float timer = 0f;

    public void Setup(Enemy caster, DamageScaleData damageData, LayerMask targetMask)
    {
        this.owner = caster;
        this.damageScale = damageData;
        if (targetMask.value != 0)
            this.whatIsTarget = targetMask;
        else if (caster != null)
            this.whatIsTarget = caster.WhatIsPlayer;

        timer = eruptionDelay;
        hasErupted = false;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!hasErupted)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                EruptSpikes();
            }
        }
    }

    private void EruptSpikes()
    {
        hasErupted = true;

        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, damageRadius, whatIsTarget);
        var damagedTargets = new System.Collections.Generic.HashSet<IDamageable>();

        foreach (var target in hitTargets)
        {
            if (target.TryGetComponent<IDamageable>(out var damageable))
            {
                if (damagedTargets.Add(damageable))
                {
                    if (owner != null && owner.combat != null)
                    {
                        owner.combat.PerformAttackOnTarget(target.transform, damageScale);
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
