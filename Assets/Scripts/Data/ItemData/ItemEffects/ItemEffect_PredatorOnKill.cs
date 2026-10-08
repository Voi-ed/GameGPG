using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Predator on Kill", fileName = "Item effect data - Predator on Kill")]
public class ItemEffect_PredatorOnKill : ItemEffect_DataSO
{
    [SerializeField] private float healPercent = 0.06f;
    [SerializeField] private float damageBonus = 12f;
    [SerializeField] private float buffDuration = 8f;
    [SerializeField] private float maxDistance = 15f;
    private const string PREDATOR_SOURCE = "PredatorOnKillBuff";

    [Header("VFX")]
    [SerializeField] private GameObject soulEscapeVfxPrefab;

    private GameObject GetVfxPrefab() => soulEscapeVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        Enemy.OnEnemyDeath += HandleEnemyKill;
    }

    public override void Unsubscribe()
    {
        Enemy.OnEnemyDeath -= HandleEnemyKill;
        base.Unsubscribe();
    }

    private void HandleEnemyKill(Enemy deadEnemy)
    {
        if (player == null || deadEnemy == null || player.health == null || player.health.isDead)
            return;

        float dist = Vector2.Distance(player.transform.position, deadEnemy.transform.position);
        if (dist > maxDistance)
            return;

        float healAmount = player.stats.GetMaxHealth() * healPercent;
        player.health.IncreaseHealth(healAmount);

        BuffEffectData[] buffs = new BuffEffectData[]
        {
            new BuffEffectData { type = StatType.Damage, value = damageBonus }
        };
        player.stats.ApplyBuff(buffs, buffDuration, PREDATOR_SOURCE);

        GameObject prefab = GetVfxPrefab();
        if (prefab != null)
        {
            GameObject vfx = Instantiate(prefab, deadEnemy.transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }
    }
}
