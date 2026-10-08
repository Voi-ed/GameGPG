using UnityEngine;

public class Enemy_BlackKnightHealth : Enemy_Health
{
    private Enemy_BlackKnight blackKnight;

    protected override void Awake()
    {
        base.Awake();
        blackKnight = GetComponent<Enemy_BlackKnight>();
    }

    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (canTakeDamage == false || isDead)
            return false;

        // Invincible roll frames
        if (blackKnight != null && blackKnight.isRolling)
        {
            return false;
        }

        // Active parry stance: deflect incoming attack and trigger riposte counter!
        if (blackKnight != null && blackKnight.isParrying)
        {
            blackKnight.TriggerRiposteCounter(damageDealer);
            return false;
        }

        // Hyper armor during heavy attacks / weapon art / enrage: immune to stagger & knockback
        if (blackKnight != null && blackKnight.hasHyperArmor)
        {
            if (AttackEvaded())
                return false;

            Entity_Stats attackerStats = damageDealer != null ? damageDealer.GetComponent<Entity_Stats>() : null;
            float armorReduction = attackerStats != null ? attackerStats.GetArmorReduction() : 0;
            float mitigation = entityStats != null ? entityStats.GetArmorMitigation(armorReduction) : 0;
            float resistance = entityStats != null ? entityStats.GetElementalResistance(element) : 0;

            float physicalDamageTaken = damage * (1 - mitigation);
            float elementalDamageTaken = elementalDamage * (1 - resistance);

            if (blackKnight.isEnraged)
            {
                physicalDamageTaken *= blackKnight.enrageDefenseMultiplier;
                elementalDamageTaken *= blackKnight.enrageDefenseMultiplier;
            }

            float totalDamage = physicalDamageTaken + elementalDamageTaken;
            ReduceHealth(totalDamage);
            NotifyTakingDamage(totalDamage);

            if (damageDealer != null && damageDealer.GetComponent<Player>() != null)
                blackKnight.TryEnterBattleState(damageDealer);

            return true;
        }

        // Standard damage handling
        if (blackKnight != null && blackKnight.isEnraged)
        {
            damage *= blackKnight.enrageDefenseMultiplier;
            elementalDamage *= blackKnight.enrageDefenseMultiplier;
        }

        return base.TakeDamage(damage, elementalDamage, element, damageDealer);
    }
}
