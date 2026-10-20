using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Thorns on Taking Damage", fileName = "Item effect data - Thorns")]
public class ItemEffect_ThornsOnTakingDamage : ItemEffect_DataSO
{
    [Range(0f, 1f)]
    [SerializeField] private float reflectPercent = 0.35f;
    [SerializeField] private float reflectRadius = 2.5f;
    [SerializeField] private LayerMask whatIsEnemy;

    [Header("VFX")]
    [SerializeField] private GameObject thornsVfxPrefab;

    private GameObject GetVfxPrefab() => thornsVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        if (player.health != null)
            player.health.OnTakingDamage += HandleDamageReflect;
    }

    public override void Unsubscribe()
    {
        if (player != null && player.health != null)
            player.health.OnTakingDamage -= HandleDamageReflect;
        base.Unsubscribe();
    }

    private void HandleDamageReflect()
    {
        if (player == null || player.health == null)
            return;

        float dmgTaken = player.health.lastDamageTaken;
        if (dmgTaken <= 0)
            return;

        float reflectDmg = dmgTaken * reflectPercent;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, reflectRadius, whatIsEnemy);
        
        if (enemies.Length > 0)
        {
            GameObject prefab = GetVfxPrefab();
            if (prefab != null)
            {
                GameObject vfx = InstantiateVfx(prefab, player.transform.position, Quaternion.identity);
                DestroyVfx(vfx, 2f);
            }
        }

        foreach (var col in enemies)
        {
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(reflectDmg, 0, ElementType.None, player.transform);
            }
        }
    }
}
