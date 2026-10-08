using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Armor Shred on Hit", fileName = "Item effect data - Armor Shred")]
public class ItemEffect_ArmorShredOnHit : ItemEffect_DataSO
{
    [SerializeField] private float bonusDamagePercent = 0.25f;

    [Header("VFX")]
    [SerializeField] private GameObject shredVfxPrefab;

    private GameObject GetVfxPrefab() => shredVfxPrefab;

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
        if (target == null || player == null)
            return;

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null && attackData.phyiscalDamage > 0)
        {
            float bonusPierceDamage = attackData.phyiscalDamage * bonusDamagePercent;
            damageable.TakeDamage(bonusPierceDamage, 0, ElementType.None, player.transform);

            GameObject prefab = GetVfxPrefab();
            if (prefab != null)
            {
                GameObject vfx = Instantiate(prefab, target.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }
        }
    }
}
