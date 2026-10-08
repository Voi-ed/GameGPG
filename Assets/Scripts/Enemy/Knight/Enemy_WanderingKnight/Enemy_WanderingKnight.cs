using UnityEngine;

public class Enemy_WanderingKnight : Enemy, ICounterable
{
    public bool CanBeCountered => canBeStunned;

    [Header("Wandering Knight States")]
    public WanderingKnight_BattleState wkBattleState { get; private set; }
    public WanderingKnight_ComboAttackState wkComboAttackState { get; private set; }
    public WanderingKnight_ThrustAttackState wkThrustState { get; private set; }
    public WanderingKnight_AirSlashState wkAirSlashState { get; private set; }
    public WanderingKnight_RollState wkRollState { get; private set; }
    public WanderingKnight_HealState wkHealState { get; private set; }
    public WanderingKnight_PrayState wkPrayState { get; private set; }
    public WanderingKnight_DeadState wkDeadState { get; private set; }

    [Header("Combo Attack Details")]
    public int maxComboHits = 3;
    public float comboStepSpeed = 2.8f;
    public DamageScaleData comboAttackScale;

    [Header("Thrust Attack Details")]
    public float thrustSpeed = 8.5f;
    public float thrustDistance = 4.5f;
    public float thrustCooldown = 4.5f;
    public DamageScaleData thrustAttackScale;
    private float lastThrustTime = float.NegativeInfinity;

    [Header("Air Slash Attack Details")]
    public float airSlashJumpForce = 6.5f;
    public float airSlashHorizontalSpeed = 5.5f;
    public float airSlashDistance = 6.5f;
    public float airSlashCooldown = 6.5f;
    public DamageScaleData airSlashAttackScale;
    private float lastAirSlashTime = float.NegativeInfinity;

    [Header("Roll Details")]
    public float rollSpeed = 6.2f;
    public float rollDuration = 0.42f;
    public float rollCooldown = 3.5f;
    public bool isRolling { get; set; }
    public int rollDir { get; private set; } = 1;
    private float lastRollTime = float.NegativeInfinity;

    [Header("Survival - Heal Flask")]
    [Range(0.1f, 0.5f)] public float healHpThreshold = 0.35f;
    [Range(0.1f, 0.5f)] public float healAmountPercent = 0.25f;
    public int maxHealCharges = 2;
    public int remainingHealCharges { get; private set; } = 2;
    public float healCooldown = 20f;
    private float lastHealTime = float.NegativeInfinity;

    [Header("Survival - Pray Buff")]
    public float prayCooldown = 16f;
    public float prayBuffDuration = 9f;
    public float prayAttackBonus = 15f;
    public float praySpeedBonus = 0.2f;
    public bool isPrayBuffActive { get; private set; }
    private float prayBuffEndTime = float.NegativeInfinity;
    private float lastPrayTime = float.NegativeInfinity;

    public Enemy_VFX enemyVfx { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        enemyVfx = GetComponent<Enemy_VFX>();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");
        wkDeadState = new WanderingKnight_DeadState(this, stateMachine, "idle");
        deadState = wkDeadState;

        wkBattleState = new WanderingKnight_BattleState(this, stateMachine, "battle");
        wkComboAttackState = new WanderingKnight_ComboAttackState(this, stateMachine, "comboAttack");
        wkThrustState = new WanderingKnight_ThrustAttackState(this, stateMachine, "thrust");
        wkAirSlashState = new WanderingKnight_AirSlashState(this, stateMachine, "airSlash");
        wkRollState = new WanderingKnight_RollState(this, stateMachine, "roll");
        wkHealState = new WanderingKnight_HealState(this, stateMachine, "heal");
        wkPrayState = new WanderingKnight_PrayState(this, stateMachine, "pray");

        battleState = wkBattleState;
        remainingHealCharges = maxHealCharges;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();

        // Expire Pray buff
        if (isPrayBuffActive && Time.time > prayBuffEndTime)
        {
            RemovePrayBuff();
        }
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    // Cooldown and availability checks
    public bool CanRoll() => Time.time > lastRollTime + rollCooldown;
    public bool CanThrust() => Time.time > lastThrustTime + thrustCooldown;
    public bool CanAirSlash() => Time.time > lastAirSlashTime + airSlashCooldown;
    public bool CanHeal() => remainingHealCharges > 0 && Time.time > lastHealTime + healCooldown;
    public bool CanPray() => !isPrayBuffActive && Time.time > lastPrayTime + prayCooldown;

    public void SetRollOnCooldown() => lastRollTime = Time.time;
    public void SetThrustOnCooldown() => lastThrustTime = Time.time;
    public void SetAirSlashOnCooldown() => lastAirSlashTime = Time.time;
    public void SetHealOnCooldown() => lastHealTime = Time.time;
    public void SetPrayOnCooldown() => lastPrayTime = Time.time;

    public void SetRollDirection(int dir) => rollDir = dir >= 0 ? 1 : -1;

    // Pray buff management
    public void ApplyPrayBuff()
    {
        if (isPrayBuffActive)
            return;

        isPrayBuffActive = true;
        prayBuffEndTime = Time.time + prayBuffDuration;

        stats?.offense.damage.AddModifier(prayAttackBonus, "WanderingKnight_Pray");
        stats?.offense.attackSpeed.AddModifier(praySpeedBonus, "WanderingKnight_Pray");
        vfx?.DoImageEchoEffect(prayBuffDuration);
        sfx?.PlayAttackHit();
    }

    public void RemovePrayBuff()
    {
        if (!isPrayBuffActive)
            return;

        isPrayBuffActive = false;
        stats?.offense.damage.RemoveModifier("WanderingKnight_Pray");
        stats?.offense.attackSpeed.RemoveModifier("WanderingKnight_Pray");
    }

    // Heal execution
    public void ConsumeHealCharge()
    {
        if (remainingHealCharges <= 0)
            return;

        remainingHealCharges--;
        float healAmount = (stats != null ? stats.GetMaxHealth() : 100f) * healAmountPercent;
        health?.IncreaseHealth(healAmount);
        vfx?.CreateOnHitVFX(transform, false, ElementType.None);
    }

    // Combat helper
    public void PerformAttackWithScale(DamageScaleData scaleData)
    {
        var prevScale = combat.basicAttackScale;
        if (scaleData != null)
            combat.basicAttackScale = scaleData;

        combat.PerformAttack();
        combat.basicAttackScale = prevScale;
    }

    // Animation Event Callbacks
    public void OnComboHitTrigger() => wkComboAttackState?.OnComboHit();
    public void OnThrustHitTrigger() => wkThrustState?.OnThrustHit();
    public void OnAirSlashHitTrigger() => wkAirSlashState?.OnAirSlashHit();
    public void OnHealTrigger() => wkHealState?.OnHealTrigger();
    public void OnPrayBuffTrigger() => wkPrayState?.OnPrayBuffTrigger();
    public void OnRollEndTrigger() => CurrentStateAnimationTrigger();
}
