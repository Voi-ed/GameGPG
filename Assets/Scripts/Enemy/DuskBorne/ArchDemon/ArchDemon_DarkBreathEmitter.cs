using System.Collections.Generic;
using UnityEngine;

public class ArchDemon_DarkBreathEmitter : MonoBehaviour
{
    [Header("Dark Breath Hitbox Settings")]
    [SerializeField] private Vector2 hitboxSize = new Vector2(4.6f, 2.0f);
    [SerializeField] private Vector2 hitboxOffset = new Vector2(2.3f, 0f);
    [SerializeField] private float hitInterval = 0.2f;
    [SerializeField] private LayerMask whatIsTarget;

    private Enemy_ArchDemon archDemon;
    private DamageScaleData damageScale;
    private bool isEmitting = false;
    private readonly Dictionary<IDamageable, float> lastHitTimes = new Dictionary<IDamageable, float>();

    public void Setup(Enemy_ArchDemon owner, DamageScaleData damageScale, LayerMask targetMask)
    {
        this.archDemon = owner;
        this.damageScale = damageScale;
        this.whatIsTarget = (targetMask.value != 0) ? targetMask : LayerMask.GetMask("Player");
    }

    public void StartBreath()
    {
        isEmitting = true;
        lastHitTimes.Clear();
    }

    public void StopBreath()
    {
        isEmitting = false;
    }

    public void ForceKillBreath()
    {
        isEmitting = false;
    }

    private void Update()
    {
        if (!isEmitting || archDemon == null)
            return;

        int dir = archDemon.facingDir;
        Vector2 origin = transform.position;
        Vector2 center = origin + new Vector2(dir * hitboxOffset.x, hitboxOffset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitboxSize, 0f, whatIsTarget);
        if (hits == null || hits.Length == 0)
            return;

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable == null)
                damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            if (!lastHitTimes.TryGetValue(damageable, out float lastTime) || Time.time >= lastTime + hitInterval)
            {
                lastHitTimes[damageable] = Time.time;

                Transform targetTransform = (damageable as Component)?.transform ?? hit.transform;
                if (archDemon.combat != null)
                {
                    archDemon.combat.PerformAttackOnTarget(targetTransform, damageScale);
                }
                else if (archDemon.stats != null)
                {
                    AttackData atk = archDemon.stats.GetAttackData(damageScale);
                    damageable.TakeDamage(atk.phyiscalDamage, atk.elementalDamage, atk.element, archDemon.transform);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        int dir = archDemon != null ? archDemon.facingDir : 1;
        Vector2 origin = transform.position;
        Vector2 center = origin + new Vector2(dir * hitboxOffset.x, hitboxOffset.y);

        Gizmos.color = new Color(0.8f, 0.1f, 0.9f, 0.5f);
        Gizmos.DrawWireCube(center, hitboxSize);
    }
}
