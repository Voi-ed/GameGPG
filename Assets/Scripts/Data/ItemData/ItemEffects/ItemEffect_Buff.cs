using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Buff effect", fileName = "Item effect data - Buff")]

public class ItemEffect_Buff : ItemEffect_DataSO
{
    [SerializeField] private BuffEffectData[] buffsToApply;
    [SerializeField] private float duration;
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
        if (buffVfxPrefab != null && player != null)
        {
            player.vfx?.CreateEffectOf(buffVfxPrefab, player.transform, true);
        }

        player.stats.ApplyBuff(buffsToApply, duration, source);
        player = null;
    }
}
