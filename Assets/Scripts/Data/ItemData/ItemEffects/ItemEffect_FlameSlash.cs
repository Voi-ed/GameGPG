using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Flame Slash", fileName = "Item effect data - Flame Slash")]
public class ItemEffect_FlameSlash : ItemEffect_DataSO
{
    [SerializeField] private float bonusFireDamage = 18f;
    [SerializeField] private float procChance = 0.4f;
    [SerializeField] private float cooldown = 0.5f;
    private float lastTriggerTime = -999f;

    [Header("VFX")]
    [SerializeField] private GameObject flameSlashVfxPrefab;

    private GameObject GetVfxPrefab() => flameSlashVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        lastTriggerTime = -999f;
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
        if (target == null || player == null || Time.time < lastTriggerTime + cooldown)
            return;

        if (Random.value > procChance)
            return;

        lastTriggerTime = Time.time;
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(0, bonusFireDamage, ElementType.Fire, player.transform);

            GameObject prefab = GetVfxPrefab();
            if (prefab != null)
            {
                GameObject vfx = InstantiateVfx(prefab, target.position, Quaternion.identity);
                Renderer[] renderers = vfx.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    r.sortingLayerName = "Default";
                    r.sortingOrder = 50;
                }
                DestroyVfx(vfx, 1.5f);
            }
        }
    }
}
