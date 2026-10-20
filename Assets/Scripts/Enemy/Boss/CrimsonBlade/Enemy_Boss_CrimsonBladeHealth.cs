using UnityEngine;

/// <summary>
/// Quản lý máu, thế thủ phản đòn và chuyển Phase cho Boss The Crimson Blade
/// </summary>
public class Enemy_Boss_CrimsonBladeHealth : Enemy_Health
{
    private Enemy_Boss_CrimsonBlade crimsonBlade;
    private bool phase2Triggered = false;

    protected override void Awake()
    {
        base.Awake();
        crimsonBlade = GetComponent<Enemy_Boss_CrimsonBlade>();
    }

    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (!canTakeDamage || isDead)
            return false;

        // Nếu đang trong thế thủ phản đòn Blood Guard
        if (crimsonBlade != null && crimsonBlade.isParryGuarding)
        {
            // Chặn đứng đòn đánh và đẩy lùi kẻ tấn công
            if (damageDealer != null)
            {
                Entity attacker = damageDealer.GetComponent<Entity>();
                if (attacker != null)
                {
                    int dir = (attacker.transform.position.x > transform.position.x) ? 1 : -1;
                    attacker.ReciveKnockback(new Vector2(dir * 5f, 2f), 0.25f);
                }
            }

            // Kích hoạt phản đòn lập tức
            crimsonBlade.stateMachine.ChangeState(crimsonBlade.crimsonComboAttackState);
            return false;
        }

        // Nếu đang có Hyper Armor (ví dụ đang lướt Blink Strike)
        if (crimsonBlade != null && crimsonBlade.hasHyperArmor)
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
        if (!phase2Triggered && crimsonBlade != null && currentHealth <= maxHealth * 0.5f)
        {
            phase2Triggered = true;
            crimsonBlade.TriggerPhase2();
        }
    }
}
