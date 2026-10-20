using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Bleed on Crit", fileName = "Item effect data - Bleed on Crit")]
public class ItemEffect_BleedOnCrit : ItemEffect_DataSO
{
    [SerializeField] private float bleedDamagePerTick = 15f;
    [SerializeField] private int totalTicks = 3;
    [SerializeField] private float tickInterval = 1f;

    [Header("VFX")]
    [SerializeField] private GameObject critHitVfxPrefab;
    [SerializeField] private GameObject bleedTickVfxPrefab;

    private GameObject GetCritHitPrefab() => critHitVfxPrefab;
    private GameObject GetBleedTickPrefab() => bleedTickVfxPrefab;

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
        if (!attackData.isCrit || target == null || player == null)
            return;

        GameObject critPrefab = GetCritHitPrefab();
        if (critPrefab != null)
        {
            GameObject vfx = InstantiateVfx(critPrefab, target.position, Quaternion.identity);
            DestroyVfx(vfx, 2f);
        }

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            player.StartCoroutine(ApplyBleedRoutine(target, damageable));
        }
    }

    private IEnumerator ApplyBleedRoutine(Transform target, IDamageable damageable)
    {
        for (int i = 0; i < totalTicks; i++)
        {
            yield return new WaitForSeconds(tickInterval);

            if (target == null || damageable == null || player == null)
                yield break;

            damageable.TakeDamage(bleedDamagePerTick, 0, ElementType.None, player.transform);

            GameObject tickPrefab = GetBleedTickPrefab();
            if (tickPrefab != null && target != null)
            {
                GameObject vfx = InstantiateVfx(tickPrefab, target.position, Quaternion.identity);
                DestroyVfx(vfx, 2f);
            }
        }
    }
}
