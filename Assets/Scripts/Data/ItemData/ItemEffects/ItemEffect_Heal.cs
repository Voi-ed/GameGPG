using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Heal effect", fileName = "Item effect data - Heal")]

public class ItemEffect_Heal : ItemEffect_DataSO
{
    [SerializeField] private float healPercent = .1f;
    [SerializeField] private GameObject healVfxPrefab;

    public override void ExecuteEffect()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player == null) return;

        float healAmount = player.stats.GetMaxHealth() * healPercent;

        player.health.IncreaseHealth(healAmount);
        if (healVfxPrefab != null)
        {
            player.vfx?.CreateEffectOf(healVfxPrefab, player.transform, true);
        }
        else
        {
            player.vfx?.PlayHealVfx();
        }
    }
}

