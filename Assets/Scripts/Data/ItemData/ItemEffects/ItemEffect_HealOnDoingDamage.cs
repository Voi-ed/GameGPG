using UnityEngine;


[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Heal on doing damage", fileName = "Item effect data - Heal on doing phys damage")]

public class ItemEffect_HealOnDoingDamage : ItemEffect_DataSO
{
    [SerializeField] private float percentHealedOnAttack = .2f;


    [Header("VFX")]
    [SerializeField] private GameObject lifestealVfxPrefab;

    private GameObject GetVfxPrefab() => lifestealVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        player.combat.OnDoingPhysicalDamage += HealOnDoingDamage;
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        player.combat.OnDoingPhysicalDamage -= HealOnDoingDamage;
        player = null;
    }

    private void HealOnDoingDamage(float damage)
    {
        if (damage <= 0 || player == null || player.health == null) return;
        player.health.IncreaseHealth(damage * percentHealedOnAttack);

        GameObject prefab = GetVfxPrefab();
        if (prefab != null)
        {
            GameObject vfx = InstantiateVfx(prefab, player.transform.position + Vector3.up * 0.5f, Quaternion.identity);
            DestroyVfx(vfx, 2f);
        }
    }
}
