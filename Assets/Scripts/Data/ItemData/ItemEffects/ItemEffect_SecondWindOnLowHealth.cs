using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Second Wind", fileName = "Item effect data - Second Wind")]
public class ItemEffect_SecondWindOnLowHealth : ItemEffect_DataSO
{
    [SerializeField] private float triggerThreshold = 0.3f; // < 30%
    [SerializeField] private float healPercent = 0.25f;
    [SerializeField] private float armorBonus = 40f;
    [SerializeField] private float buffDuration = 10f;
    [SerializeField] private float cooldown = 60f;
    private float lastTimeTriggered = -999f;
    private const string SECOND_WIND_SOURCE = "SecondWindBuff";

    [Header("VFX")]
    [SerializeField] private GameObject secondWindVfxPrefab;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        if (player.health != null)
            player.health.OnTakingDamage += CheckSecondWind;
    }

    public override void Unsubscribe()
    {
        if (player != null && player.health != null)
            player.health.OnTakingDamage -= CheckSecondWind;
        base.Unsubscribe();
    }

    private void CheckSecondWind()
    {
        if (player == null || player.health == null || player.stats == null)
            return;

        if (Time.time < lastTimeTriggered + cooldown)
            return;

        if (player.health.GetHealthPercent() <= triggerThreshold)
        {
            lastTimeTriggered = Time.time;
            float healAmount = player.stats.GetMaxHealth() * healPercent;
            player.health.IncreaseHealth(healAmount);

            BuffEffectData[] buffs = new BuffEffectData[]
            {
                new BuffEffectData { type = StatType.Armor, value = armorBonus }
            };
            player.stats.ApplyBuff(buffs, buffDuration, SECOND_WIND_SOURCE);

            // Sleek code-driven green heal burst feedback (xanh lá cây)
            player.vfx?.PlayHealVfx();

            // Optional fallback prefab if explicitly configured
            if (secondWindVfxPrefab != null)
            {
                GameObject vfx = InstantiateVfx(secondWindVfxPrefab, player.transform.position, Quaternion.identity);
                DestroyVfx(vfx, 1.5f);
            }
        }
    }
}


