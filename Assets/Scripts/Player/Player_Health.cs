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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            Die();
    }

    protected override void Die()
    {
        if (canPreventDeath != null && canPreventDeath.Invoke())
        {
            OnDeathPrevented?.Invoke();
            return;
        }

        base.Die();
        player.ui.OpenDeathScreenUI();
    }
}