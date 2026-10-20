using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Frost Slow on Hit", fileName = "Item effect data - Frost Slow")]
public class ItemEffect_FrostSlowOnHit : ItemEffect_DataSO
{
    [SerializeField] private float iceDamage = 15f;
    [SerializeField] private float internalCooldown = 1f;
    private float lastProcTime = -999f;
    [Header("VFX")]
    [SerializeField] private GameObject iceVfxPrefab;

    private GameObject GetVfxPrefab() => iceVfxPrefab;

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
        if (Time.time < lastProcTime + internalCooldown || target == null || player == null)
            return;

        lastProcTime = Time.time;
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(0, iceDamage, ElementType.Ice, player.transform);

            GameObject prefab = GetVfxPrefab();
            if (prefab != null)
            {
                GameObject vfx = InstantiateVfx(prefab, target.position, Quaternion.identity);
                DestroyVfx(vfx, 2f);
            }
        }
    }
}
