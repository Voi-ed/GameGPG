using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Set - Inferno Overlord", fileName = "Item effect data - Set Inferno Overlord")]
public class ItemEffect_SetInfernoOverlord : ItemEffect_DataSO
{
    [SerializeField] private int hitRequirement = 3;
    [SerializeField] private float eruptionDamage = 70f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float cooldown = 2.5f;
    [SerializeField] private LayerMask whatIsEnemy;

    private int currentHitCount = 0;
    private float lastTriggerTime = -999f;
    private float lastHitCountTime = -999f;

    [Header("VFX")]
    [SerializeField] private GameObject impactBurstVfxPrefab;

    private GameObject GetImpactPrefab() => impactBurstVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        currentHitCount = 0;
        lastTriggerTime = -999f;
        lastHitCountTime = -999f;
        if (player.combat != null)
            player.combat.OnDoingDamage += HandleAttackHit;
    }

    public override void Unsubscribe()
    {
        if (player != null && player.combat != null)
            player.combat.OnDoingDamage -= HandleAttackHit;
        base.Unsubscribe();
    }

    private void HandleAttackHit(AttackData attackData, Transform target)
    {
        if (player == null || Time.time < lastTriggerTime + cooldown)
            return;

        // Chỉ tính 1 hit cho mỗi đòn vung kiếm
        if (Time.time - lastHitCountTime > 0.15f)
        {
            lastHitCountTime = Time.time;
            currentHitCount++;
        }

        if (currentHitCount >= hitRequirement)
        {
            currentHitCount = 0;
            lastTriggerTime = Time.time;

            // Fallback vị trí nổ an toàn nếu quái vừa bị kết liễu
            Vector3 center = target != null ? target.position : player.transform.position;

            // 1. Spawn Impact Burst tại tâm vụ nổ
            GameObject burstPrefab = GetImpactPrefab();
            if (burstPrefab != null)
            {
                GameObject burst = InstantiateVfx(burstPrefab, center, Quaternion.identity);
                DestroyVfx(burst, 2f);
            }

            // 2. Deal Area Fire Damage
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius, whatIsEnemy);
            HashSet<GameObject> processedEntities = new HashSet<GameObject>();

            foreach (var col in colliders)
            {
                if (col == null) continue;
                GameObject rootObj = col.transform.root.gameObject;
                if (processedEntities.Contains(rootObj))
                    continue;
                processedEntities.Add(rootObj);

                IDamageable damageable = col.GetComponent<IDamageable>();
                Entity_Health health = col.GetComponent<Entity_Health>();
                bool wasAlive = health == null || !health.isDead;

                if (damageable != null && wasAlive)
                {
                    damageable.TakeDamage(0, eruptionDamage, ElementType.Fire, player.transform);
                }
            }
        }
    }
}
