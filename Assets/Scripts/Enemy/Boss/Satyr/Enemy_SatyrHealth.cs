using UnityEngine;

public class Enemy_SatyrHealth : Enemy_Health
{
    private Enemy_Satyr satyr;
    private bool phase2Triggered = false;

    protected override void Awake()
    {
        base.Awake();
        satyr = GetComponent<Enemy_Satyr>();
    }

    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (canTakeDamage == false || isDead)
            return false;

        // Hyper armor during slide charge or uninterruptible skills (Fire Breath, Spirit Bomb)
        if (satyr != null && satyr.hasHyperArmor)
        {
            if (AttackEvaded())
                return false;

            Entity_Stats attackerStats = damageDealer != null ? damageDealer.GetComponent<Entity_Stats>() : null;
            float armorReduction = attackerStats != null ? attackerStats.GetArmorReduction() : 0;
            float mitigation = entityStats != null ? entityStats.GetArmorMitigation(armorReduction) : 0;
            float resistance = entityStats != null ? entityStats.GetElementalResistance(element) : 0;

            float physicalDamageTaken = damage * (1 - mitigation);
            float elementalDamageTaken = elementalDamage * (1 - resistance);

            float totalDamage = physicalDamageTaken + elementalDamageTaken;
            ReduceHealth(totalDamage);
            NotifyTakingDamage(totalDamage);

            CheckPhase2Transition();
            return true;
        }

        bool damaged = base.TakeDamage(damage, elementalDamage, element, damageDealer);
        if (damaged)
        {
            CheckPhase2Transition();
        }

        return damaged;
    }

    private void CheckPhase2Transition()
    {
        if (isDead || currentHealth <= 0)
            return;

        float maxHealth = entityStats != null ? entityStats.GetMaxHealth() : 100f;
        if (!phase2Triggered && satyr != null && currentHealth <= maxHealth * 0.5f)
        {
            phase2Triggered = true;
            satyr.TriggerPhase2();
        }
    }
}
