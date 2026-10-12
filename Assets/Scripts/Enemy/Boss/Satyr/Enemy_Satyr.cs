using UnityEngine;

public class Enemy_Satyr : Enemy, ICounterable
{
    public bool CanBeCountered => canBeStunned;

    [Header("Satyr - General & Movement")]
    [SerializeField] private float runSpeed = 2.0f;
    [SerializeField] private float phase2SpeedMultiplier = 1.25f;
    [SerializeField] private float phase2CooldownMultiplier = 0.75f;

    [Header("Skill 1: Demonic Maw Fire Breath (Basic Attack)")]
    [SerializeField] private GameObject fireBreathVfxPrefab;
    [SerializeField] private float fireBreathCooldown = 0.7f;
    [SerializeField] private float fireBreathDuration = 1.6f;
    [SerializeField] private DamageScaleData fireBreathDamageScale;

    [Header("Skill 2: Void Spirit Bomb (Phase 2 Ultimate)")]
    [SerializeField] private Satyr_SpiritBombProjectile spiritBombPrefab;
    [SerializeField] private float spiritBombCooldown = 12.0f;
    [SerializeField] private float spiritBombChargeDuration = 1.0f;
    [SerializeField] private float spiritBombFlyHeight = 2.0f;
    [SerializeField] private DamageScaleData spiritBombDamageScale;

    [Header("Skill 3: Shadow Dash (Continuous Mobility)")]
    [SerializeField] private float dashSpeed = 14.0f;
    [SerializeField] private float dashDuration = 0.32f;
    [SerializeField] private float dashCooldown = 0.35f;
    [SerializeField] private int dashesPerCombo = 2;          // Số lần dash liên tiếp trước khi phải nghỉ
    [SerializeField] private float comboRestDuration = 0.8f;  // Thời gian nghỉ sau combo dash (cửa sổ phản công cho người chơi)

    [Header("Anchors & Detection")]
    [SerializeField] private Transform mouthAnchor;
    [SerializeField] private Transform headAnchor;
    [SerializeField] private Transform groundPos;

    // Timers
    private float lastTimeFireBreath = float.NegativeInfinity;
    private float lastTimeSpiritBomb = float.NegativeInfinity;
    private float lastTimeDash = float.NegativeInfinity;
    private float restUntilTime = float.NegativeInfinity;
    private int dashComboCount = 0;

    // State & Phase Management
    public bool isPhase2 { get; private set; } = false;
    public bool hasHyperArmor { get; set; } = false;

    // Sub-components
    public Satyr_FireBreathEmitter fireBreathEmitter { get; private set; }
    public Satyr_SpiritBombProjectile currentSpiritBomb { get; private set; }
    public Enemy_SatyrHealth satyrHealth { get; private set; }

    // FSM States
    public Satyr_BattleState satyrBattleState { get; private set; }
    public Satyr_FireBreathState satyrFireBreathState { get; private set; }
    public Satyr_SpiritBombState satyrSpiritBombState { get; private set; }
    public Satyr_DashState satyrDashState { get; private set; }
    public Satyr_PhaseTransitionState satyrPhaseTransitionState { get; private set; }
    public Satyr_StunnedState satyrStunnedState { get; private set; }
    public Satyr_DeadState satyrDeadState { get; private set; }

    // Properties
    public float RunSpeed => (isPhase2 ? runSpeed * phase2SpeedMultiplier : runSpeed) * activeSlowMultiplier;
    public float BattleMoveSpeed => (isPhase2 ? battleMoveSpeed * phase2SpeedMultiplier : battleMoveSpeed) * activeSlowMultiplier;
    public float SatyrBattleMoveSpeed => BattleMoveSpeed;
    public float DashSpeed => (isPhase2 ? dashSpeed * phase2SpeedMultiplier : dashSpeed) * activeSlowMultiplier;
    public float DashDuration => dashDuration;
    public float FireBreathDuration => fireBreathDuration;
    public float SpiritBombChargeDuration => spiritBombChargeDuration;
    public float SpiritBombFlyHeight => spiritBombFlyHeight;
    public Transform MouthAnchor => mouthAnchor != null ? mouthAnchor : transform;
    public Transform HeadAnchor => headAnchor != null ? headAnchor : transform;
    public Transform GroundPosition => groundPos != null ? groundPos : transform;

    protected override void Awake()
    {
        base.Awake();

        satyrHealth = GetComponent<Enemy_SatyrHealth>();

        // Setup Emitter at mouth
        if (mouthAnchor != null)
        {
            fireBreathEmitter = mouthAnchor.GetComponent<Satyr_FireBreathEmitter>();
            if (fireBreathEmitter == null)
                fireBreathEmitter = mouthAnchor.gameObject.AddComponent<Satyr_FireBreathEmitter>();

            fireBreathEmitter.Setup(this, fireBreathDamageScale, WhatIsPlayer);
        }

        // Initialize States
        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "run");

        satyrBattleState = new Satyr_BattleState(this, stateMachine, "battle");
        battleState = satyrBattleState;

        satyrStunnedState = new Satyr_StunnedState(this, stateMachine, "hurt");
        stunnedState = satyrStunnedState;

        satyrDeadState = new Satyr_DeadState(this, stateMachine, "death");
        deadState = satyrDeadState;

        satyrFireBreathState = new Satyr_FireBreathState(this, stateMachine, "demonicMaw");
        satyrSpiritBombState = new Satyr_SpiritBombState(this, stateMachine, "float");
        satyrDashState = new Satyr_DashState(this, stateMachine, "shadowDash");
        satyrPhaseTransitionState = new Satyr_PhaseTransitionState(this, stateMachine, "hurt");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    // Cooldown checks (reduced by multiplier in Phase 2)
    private float GetCooldown(float baseCooldown) => isPhase2 ? baseCooldown * phase2CooldownMultiplier : baseCooldown;

    public bool CanFireBreath() => Time.time > lastTimeFireBreath + GetCooldown(fireBreathCooldown);
    public void SetFireBreathOnCooldown() => lastTimeFireBreath = Time.time;

    public bool CanSpiritBomb() => isPhase2 && Time.time > lastTimeSpiritBomb + GetCooldown(spiritBombCooldown);
    public void SetSpiritBombOnCooldown() => lastTimeSpiritBomb = Time.time;

    public bool IsResting => Time.time < restUntilTime;

    public bool CanDash() => !IsResting && Time.time > lastTimeDash + GetCooldown(dashCooldown);

    public void SetDashOnCooldown()
    {
        lastTimeDash = Time.time;
        dashComboCount++;

        if (dashComboCount >= dashesPerCombo)
        {
            dashComboCount = 0;
            restUntilTime = Time.time + GetCooldown(comboRestDuration);
        }
    }



    public Transform GetPlayerTarget()
    {
        if (Player.instance != null)
            return Player.instance.transform;

        return GetPlayerReference();
    }

    public void PerformAttackWithScale(DamageScaleData scaleData)
    {
        if (combat == null)
            return;

        var prevScale = combat.basicAttackScale;
        if (scaleData != null)
            combat.basicAttackScale = scaleData;

        combat.PerformAttack();
        combat.basicAttackScale = prevScale;
    }

    public void PerformAttackOnTarget(Transform target, DamageScaleData scaleData)
    {
        if (combat == null || target == null)
            return;

        var prevScale = combat.basicAttackScale;
        if (scaleData != null)
            combat.basicAttackScale = scaleData;

        combat.PerformAttackOnTarget(target, scaleData);
        combat.basicAttackScale = prevScale;
    }

    // Skill 1: Fire Breath
    public void StartFireBreathEmitter()
    {
        if (fireBreathEmitter != null)
            fireBreathEmitter.StartBreath(fireBreathVfxPrefab);
    }

    public void StopFireBreathEmitter()
    {
        if (fireBreathEmitter != null)
            fireBreathEmitter.StopBreath();
    }

    public void ForceKillFireBreathEmitter()
    {
        if (fireBreathEmitter != null)
            fireBreathEmitter.ForceKillBreath();
    }

    // Skill 2: Spirit Bomb
    public void StartSpiritBombCharge()
    {
        if (spiritBombPrefab == null)
            return;

        currentSpiritBomb = Instantiate(spiritBombPrefab, HeadAnchor.position, Quaternion.identity);
        currentSpiritBomb.Charge(this, HeadAnchor, spiritBombChargeDuration, spiritBombDamageScale);
    }

    public void LaunchSpiritBomb(Vector3 targetFloorPos)
    {
        if (currentSpiritBomb != null)
        {
            currentSpiritBomb.Launch(targetFloorPos);
            currentSpiritBomb = null;
        }
    }

    public void InterruptSpiritBomb()
    {
        if (currentSpiritBomb != null)
        {
            currentSpiritBomb.Interrupt();
            currentSpiritBomb = null;
        }

        stateMachine.ChangeState(satyrStunnedState);
    }



    // Phase 2 Transition
    public void TriggerPhase2()
    {
        if (isPhase2)
            return;

        stateMachine.ChangeState(satyrPhaseTransitionState);
    }

    public void ActivatePhase2()
    {
        isPhase2 = true;
    }

    public void HandleCounter()
    {
        if (!CanBeCountered)
            return;

        stateMachine.ChangeState(satyrStunnedState);
    }

    public override void EntityDeath()
    {
        if (currentSpiritBomb != null)
        {
            currentSpiritBomb.InterruptAndExplode();
            currentSpiritBomb = null;
        }

        StopFireBreathEmitter();
        base.EntityDeath();
    }
}
