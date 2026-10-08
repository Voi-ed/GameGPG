using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Shadow Dodge", fileName = "Item effect data - Shadow Dodge")]
public class ItemEffect_ShadowDodge : ItemEffect_DataSO
{
    [SerializeField] private float duration = 4f;
    private bool hasShadowStrikeBuff = false;
    private float buffExpireTime = 0f;
    [Header("VFX")]
    [SerializeField] private GameObject dodgeVfxPrefab;

    private GameObject GetVfxPrefab() => dodgeVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        if (player.health != null)
            player.health.OnEvasion += HandleEvasion;
        if (player.combat != null)
            player.combat.OnDoingDamage += HandleAttackHit;
    }

    public override void Unsubscribe()
    {
        RemoveBuff();
        if (player != null && player.health != null)
            player.health.OnEvasion -= HandleEvasion;
        if (player != null && player.combat != null)
            player.combat.OnDoingDamage -= HandleAttackHit;
        base.Unsubscribe();
    }

    private void HandleEvasion()
    {
        if (player == null || player.stats == null)
            return;

        GameObject prefab = GetVfxPrefab();
        if (prefab != null)
        {
            GameObject vfx = Instantiate(prefab, player.transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        if (!hasShadowStrikeBuff)
        {
            player.stats.offense.critChance.AddModifier(1f, "ShadowDodge");
            hasShadowStrikeBuff = true;
        }

        buffExpireTime = Time.time + duration;
    }

    private void HandleAttackHit(AttackData attackData, Transform target)
    {
        if (hasShadowStrikeBuff)
        {
            RemoveBuff();
        }
    }

    private void RemoveBuff()
    {
        if (hasShadowStrikeBuff && player != null && player.stats != null)
        {
            player.stats.offense.critChance.RemoveModifier("ShadowDodge");
            hasShadowStrikeBuff = false;
        }
    }
}
