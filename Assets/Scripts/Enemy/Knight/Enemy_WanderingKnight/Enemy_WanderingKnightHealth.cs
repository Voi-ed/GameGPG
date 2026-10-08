using UnityEngine;

public class Enemy_WanderingKnightHealth : Enemy_Health
{
    private Enemy_WanderingKnight wanderingKnight;

    protected override void Awake()
    {
        base.Awake();
        wanderingKnight = GetComponent<Enemy_WanderingKnight>();
    }

    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (canTakeDamage == false || isDead)
            return false;

        // Invincible during roll dodge
        if (wanderingKnight != null && wanderingKnight.isRolling)
        {
            return false;
        }

        // Interrupt heal state if struck
        if (wanderingKnight != null && wanderingKnight.stateMachine.currentState == wanderingKnight.wkHealState)
        {
            wanderingKnight.wkHealState.InterruptHeal();
        }

        return base.TakeDamage(damage, elementalDamage, element, damageDealer);
    }
}
