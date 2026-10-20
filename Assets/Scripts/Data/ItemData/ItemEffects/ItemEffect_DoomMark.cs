using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Doom Mark", fileName = "Item effect data - Doom Mark")]
public class ItemEffect_DoomMark : ItemEffect_DataSO
{
    [SerializeField] private float procChance = 0.25f;
    [SerializeField] private float delay = 2.0f;
    [SerializeField] private float detonationDamage = 45f;
    [SerializeField] private float blastRadius = 3f;
    [SerializeField] private LayerMask whatIsEnemy;
    [Header("VFX")]
    [SerializeField] private GameObject skullMarkVfxPrefab;
    [SerializeField] private GameObject explosionVfxPrefab;

    private GameObject GetSkullPrefab() => skullMarkVfxPrefab;
    private GameObject GetExplosionPrefab() => explosionVfxPrefab;

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
        if (target == null || player == null || Random.value > procChance)
            return;

        GameObject skullPrefab = GetSkullPrefab();
        if (skullPrefab != null)
        {
            GameObject skullVfx = InstantiateVfx(skullPrefab, target.position + Vector3.up * 1.2f, Quaternion.identity, target);
            DestroyVfx(skullVfx, delay + 0.5f);
        }

        player.StartCoroutine(DoomExplosionRoutine(target));
    }

    private IEnumerator DoomExplosionRoutine(Transform target)
    {
        yield return new WaitForSeconds(delay);

        Vector3 blastPos = target != null ? target.position : player.transform.position;

        GameObject expPrefab = GetExplosionPrefab();
        if (expPrefab != null)
        {
            GameObject vfx = InstantiateVfx(expPrefab, blastPos, Quaternion.identity);
            Renderer[] renderers = vfx.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = "Player";
                r.sortingOrder = 55;
            }
            DestroyVfx(vfx, 2.5f);
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(blastPos, blastRadius, whatIsEnemy);
        foreach (var col in colliders)
        {
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(detonationDamage, 0, ElementType.Lightning, player.transform);
            }
        }
    }
}
