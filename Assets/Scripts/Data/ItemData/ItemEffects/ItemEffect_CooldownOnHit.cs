using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/Cooldown on Hit", fileName = "Item effect data - Cooldown on Hit")]
public class ItemEffect_CooldownOnHit : ItemEffect_DataSO
{
    [SerializeField] private float cooldownReduction = 0.5f;
    [SerializeField] private float internalCooldown = 0.25f;
    private float lastTimeReduced = -999f;

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        if (player.combat != null)
            player.combat.OnDoingDamage += HandleDamageHit;
    }

    public override void Unsubscribe()
    {
        if (player != null && player.combat != null)
            player.combat.OnDoingDamage -= HandleDamageHit;
        base.Unsubscribe();
    }

    private void HandleDamageHit(AttackData attackData, Transform target)
    {
        if (Time.time < lastTimeReduced + internalCooldown)
            return;

        lastTimeReduced = Time.time;
        if (player != null && player.skillManager != null)
        {
            player.skillManager.ReduceAllSkillCooldownBy(cooldownReduction);
        }
    }
}
