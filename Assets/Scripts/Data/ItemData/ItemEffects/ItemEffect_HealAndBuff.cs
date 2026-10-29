using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Heal and Buff effect", fileName = "Item effect data - Heal and Buff")]
public class ItemEffect_HealAndBuff : ItemEffect_DataSO
{
    [Header("Heal details")]
    [Range(0f, 1f)]
    [SerializeField] private float healPercent = 0.25f;

    [Header("Buff details")]
    [SerializeField] private BuffEffectData[] buffsToApply;
    [SerializeField] private float duration = 60f;
    [SerializeField] private string source = Guid.NewGuid().ToString();
    [SerializeField] private GameObject buffVfxPrefab;

    public override bool CanBeUsed(Player player)
    {
        if (player.stats.CanApplyBuffOf(source))
        {
            this.player = player;
            return true;
        }
        else
        {
            Debug.Log("Same buff effect cannot be applied twice!");
            return false;
        }
    }

    public override void ExecuteEffect()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (healPercent > 0)
        {
            float healAmount = player.stats.GetMaxHealth() * healPercent;
            player.health.IncreaseHealth(healAmount);
            player.vfx?.PlayHealVfx();

            if (AudioManager.instance != null)
                AudioManager.instance.PlayGlobalSFX("potion_drink");
        }
        else if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayGlobalSFX("buff_activate");
        }

        if (buffsToApply != null && buffsToApply.Length > 0 && player != null && player.stats != null)
        {
            if (buffVfxPrefab != null)
                player.vfx?.CreateEffectOf(buffVfxPrefab, player.transform, true);

            Sprite icon = sourceItem != null ? sourceItem.itemIcon : null;
            string buffName = sourceItem != null ? sourceItem.itemName : source;
            player.stats.ApplyBuff(buffsToApply, duration, source, icon, buffName, effectDescription);
        }

        player = null;
    }
}
