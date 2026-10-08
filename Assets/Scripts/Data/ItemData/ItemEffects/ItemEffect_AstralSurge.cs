using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Astral Surge", fileName = "Item effect data - Astral Surge")]
public class ItemEffect_AstralSurge : ItemEffect_DataSO
{
    [SerializeField] private int hitRequirement = 3;
    [SerializeField] private float surgeDamage = 35f;
    [SerializeField] private float splashRadius = 2.5f;
    [SerializeField] private LayerMask whatIsEnemy;
    private int currentHitCount = 0;
    [Header("VFX")]
    [SerializeField] private GameObject astralVfxPrefab;

    private GameObject GetVfxPrefab() => astralVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        currentHitCount = 0;
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

        currentHitCount++;
        if (currentHitCount >= hitRequirement)
        {
            currentHitCount = 0;
            Vector3 targetPos = target.position;

            GameObject prefab = GetVfxPrefab();
            if (prefab != null)
            {
                GameObject vfx = Instantiate(prefab, targetPos, Quaternion.identity);
                Destroy(vfx, 2f);
            }

            Collider2D[] colliders = Physics2D.OverlapCircleAll(targetPos, splashRadius, whatIsEnemy);
            foreach (var col in colliders)
            {
                IDamageable damageable = col.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(0, surgeDamage, ElementType.Lightning, player.transform);
                }
            }
        }
    }
}
