using UnityEngine;

public class Enemy_Health : Entity_Health
{
    private Enemy enemy;
    private Player_QuestManager questManager;

    protected override void Start()
    {
        base.Start();

        enemy = GetComponent<Enemy>();
        questManager = Player.instance.questManager;
    }

    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (canTakeDamage == false || isDead) 
            return false;

        if (TryGetComponent<Enemy_ChronoStasisReceiver>(out var stasisReceiver) && stasisReceiver.isActiveAndEnabled)
        {
            if (AttackEvaded())
                return false;

            Entity_Stats attackerStats = damageDealer != null ? damageDealer.GetComponent<Entity_Stats>() : null;
            float armorReduction = attackerStats != null ? attackerStats.GetArmorReduction() : 0;
            float mitigation = entityStats != null ? entityStats.GetArmorMitigation(armorReduction) : 0;
            float resistance = entityStats != null ? entityStats.GetElementalResistance(element) : 0;

            float physicalDamageTaken = damage * (1 - mitigation);
            float elementalDamageTaken = elementalDamage * (1 - resistance);

            stasisReceiver.RecordDamage(physicalDamageTaken, elementalDamageTaken, element, damageDealer);
            NotifyTakingDamage(physicalDamageTaken + elementalDamageTaken);

            if (damageDealer != null && damageDealer.GetComponent<Player>() != null)
                enemy.TryEnterBattleState(damageDealer);

            return true;
        }

        bool wasHit = base.TakeDamage(damage, elementalDamage, element, damageDealer);

        if (wasHit == false)
            return false;

        if (damageDealer != null && damageDealer.GetComponent<Player>() != null)
            enemy.TryEnterBattleState(damageDealer);

        return true;    
    }

    protected override void Die()
    {
        base.Die();

        questManager.AddProgress(enemy.questTargetId);
    }
}
