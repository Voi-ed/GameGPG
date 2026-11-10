using System;
using UnityEngine;

public class Player_Health : Entity_Health
{
    private Player player;
    public Func<bool> canPreventDeath;
    public event Action OnDeathPrevented;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }

    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        bool wasHit = base.TakeDamage(damage, elementalDamage, element, damageDealer);
        if (wasHit && isDead == false && AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("player_hurt");
        return wasHit;
    }

    protected override void Die()
    {
        if (canPreventDeath != null && canPreventDeath.Invoke())
        {
            OnDeathPrevented?.Invoke();
            return;
        }

        base.Die();

        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("player_die");

        player.ui.OpenDeathScreenUI();
    }
}