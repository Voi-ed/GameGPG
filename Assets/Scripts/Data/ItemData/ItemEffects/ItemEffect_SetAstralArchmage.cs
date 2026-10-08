using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Set - Astral Archmage", fileName = "Item effect data - Set Astral Archmage")]
public class ItemEffect_SetAstralArchmage : ItemEffect_DataSO
{
    [SerializeField] private float cataclysmDamage = 50f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float cooldown = 4f;
    [SerializeField] private LayerMask whatIsEnemy;

    private float lastTimeTriggered = -999f;
    [Header("VFX")]
    [SerializeField] private GameObject cataclysmLightningPrefab;

    private GameObject GetLightningPrefab() => cataclysmLightningPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
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
        if (player == null || Time.time < lastTimeTriggered + cooldown)
            return;

        lastTimeTriggered = Time.time;
        Vector3 center = target != null ? target.position : player.transform.position;

        GameObject bolt = GetLightningPrefab();
        if (bolt != null)
        {
            GameObject vfx = InstantiateVfx(bolt, center, Quaternion.identity);
            Renderer[] renderers = vfx.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = "Player";
                r.sortingOrder = 60;
            }
            Destroy(vfx, 2f);
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius, whatIsEnemy);
        foreach (var col in colliders)
        {
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(0, cataclysmDamage, ElementType.Lightning, player.transform);
            }
        }

        // Reduce all cooldowns by 1s
        if (player.skillManager != null)
        {
            player.skillManager.ReduceAllSkillCooldownBy(1f);
        }
    }
}
