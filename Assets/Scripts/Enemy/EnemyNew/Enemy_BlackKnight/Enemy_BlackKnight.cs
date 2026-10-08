using UnityEngine;

public class Enemy_BlackKnight : Enemy, ICounterable
{
    public bool CanBeCountered => canBeStunned;

    [Header("Black Knight States")]
    public BlackKnight_BattleState bkBattleState { get; private set; }
    public BlackKnight_ComboAttackState bkComboAttackState { get; private set; }
    public BlackKnight_HeavyAttackState bkHeavyAttackState { get; private set; }
    public BlackKnight_ParryState bkParryState { get; private set; }
    public BlackKnight_RollState bkRollState { get; private set; }
    public BlackKnight_EnrageState bkEnrageState { get; private set; }
    public BlackKnight_WeaponArtState bkWeaponArtState { get; private set; }
    public BlackKnight_SpellCastState bkSpellCastState { get; private set; }
    public BlackKnight_DeadState bkDeadState { get; private set; }

    [Header("Combo Attack Details")]
    public int minComboHits = 2;
    public int maxComboHits = 4;
    public float comboStepSpeed = 2.5f;
    public float comboMaxFollowDistance = 3.5f;
    [Range(0, 1)] public float branchToHeavyOnFleeChance = 0.6f;
    [Range(0, 1)] public float branchToSpinOnFlankChance = 0.6f;
    public DamageScaleData comboAttackScale;

    [Header("Heavy Attack Details")]
    public float heavyEngagementDistance = 5.5f;
    public float thrustSpeed = 8f;
    public float slamHorizontalSpeed = 4f;
    public float slamLeapForce = 5f;
    public float heavyAttackCooldown = 4f;
    public DamageScaleData heavyAttackScale;
    public int selectedHeavyAttackIndex { get; private set; } = 1;
    private float lastHeavyAttackTime = float.NegativeInfinity;

    [Header("Parry Details")]
    [Range(0, 1)] public float parryChance = 0.35f;
    public float parryDuration = 0.8f;
    public float parryCooldown = 6f;
    public DamageScaleData riposteDamageScale;
    public bool isParrying { get; set; }
    private float lastParryTime = float.NegativeInfinity;

    [Header("Roll Details")]
    public float rollSpeed = 6.5f;
    public float rollDuration = 0.45f;
    public float rollCooldown = 3.5f;
    public bool isRolling { get; set; }
    public int rollDir { get; private set; } = 1;
    private float lastRollTime = float.NegativeInfinity;

    [Header("Phase 2 (Enrage & Weapon Art)")]
    [Range(0.1f, 0.9f)] public float enrageHpThreshold = 0.5f;
    public float enrageSpeedMultiplier = 1.25f;
    public float enrageDefenseMultiplier = 0.8f;
    public float enrageFireDamageBonus = 20f;
    public float enrageAttackSpeedBonus = 0.15f;
    public float weaponArtCooldown = 9f;
    public float weaponArtStepSpeed = 3f;
    public DamageScaleData weaponArtDamageScale;
    public bool isEnraged { get; private set; }
    public bool hasHyperArmor { get; set; }
    private float lastWeaponArtTime = float.NegativeInfinity;

    public Enemy_BlackKnightAbility ability { get; private set; }
    public Enemy_VFX enemyVfx { get; private set; }

    [Header("Ground Anchor")]
    [SerializeField] private Transform groundPos;
    public Transform GroundPos => groundPos != null ? groundPos : transform;

    public LayerMask whatIsTarget => ability != null ? ability.WhatIsTarget : LayerMask.GetMask("Player");

    protected override void Awake()
    {
        base.Awake();

        ability = GetComponentInChildren<Enemy_BlackKnightAbility>();
        enemyVfx = GetComponent<Enemy_VFX>();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");
        bkDeadState = new BlackKnight_DeadState(this, stateMachine, "dead");
        deadState = bkDeadState;

        bkBattleState = new BlackKnight_BattleState(this, stateMachine, "battle");
        bkComboAttackState = new BlackKnight_ComboAttackState(this, stateMachine, "comboAttack");
        bkHeavyAttackState = new BlackKnight_HeavyAttackState(this, stateMachine, "heavyAttack");
        bkParryState = new BlackKnight_ParryState(this, stateMachine, "parry");
        bkRollState = new BlackKnight_RollState(this, stateMachine, "roll");
        bkEnrageState = new BlackKnight_EnrageState(this, stateMachine, "enrage");
        bkWeaponArtState = new BlackKnight_WeaponArtState(this, stateMachine, "weaponArt");
        bkSpellCastState = new BlackKnight_SpellCastState(this, stateMachine, "enrage");

        battleState = bkBattleState;

        if (groundPos == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "GroundPos")
                {
                    groundPos = child;
                    break;
                }
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    // Cooldown and availability checks
    public bool CanParry() => Time.time > lastParryTime + parryCooldown;
    public bool CanRoll() => Time.time > lastRollTime + rollCooldown;
    public bool CanHeavyAttack() => Time.time > lastHeavyAttackTime + heavyAttackCooldown;
    public bool CanWeaponArt() => Time.time > lastWeaponArtTime + weaponArtCooldown;
    public bool CanHellfire() => ability != null && ability.CanHellfire();
    public bool CanSwordWave() => ability != null && ability.CanSwordWave();

    public void SetParryOnCooldown() => lastParryTime = Time.time;
    public void SetRollOnCooldown() => lastRollTime = Time.time;
    public void SetHeavyAttackOnCooldown() => lastHeavyAttackTime = Time.time;
    public void SetWeaponArtOnCooldown() => lastWeaponArtTime = Time.time;
    public void SetHellfireOnCooldown() => ability?.SetHellfireOnCooldown();
    public void SetSwordWaveOnCooldown() => ability?.SetSwordWaveOnCooldown();

    public void SetHeavyAttackIndex(int index) => selectedHeavyAttackIndex = Mathf.Clamp(index, 1, 3);
    public void SetRollDirection(int dir) => rollDir = dir >= 0 ? 1 : -1;

    // Ability execution delegates
    public bool isCastingHellfire => ability != null && ability.isCastingHellfire;
    public float hellfireChannelDuration => ability != null ? ability.hellfireChannelDuration : 1.8f;
    public void StartHellfireSpell() => ability?.StartHellfireSpell();
    public void CastSwordWave() => ability?.CastSwordWave();
    public void CastInfernalOrbsUltimate() => ability?.CastInfernalOrbsUltimate();
    public void TriggerLeapSlamExplosion() => ability?.TriggerLeapSlamExplosion();
    public void TriggerEnrageNova() => ability?.TriggerEnrageNova();
    public void EnableAttackAlert(bool enable) => enemyVfx?.EnableAttackAlert(enable);

    // Parry Riposte Trigger from Health
    public void TriggerRiposteCounter(Transform attacker)
    {
        if (stateMachine.currentState == bkParryState)
        {
            bkParryState.TriggerRiposte(attacker);
        }
    }

    // Phase 2 activation
    public void ActivatePhase2()
    {
        if (isEnraged)
            return;

        isEnraged = true;
        battleMoveSpeed *= enrageSpeedMultiplier;

        // Enhance stats with infernal flame power
        stats?.offense.fireDamage.AddModifier(enrageFireDamageBonus, "BlackKnight_Enrage");
        stats?.offense.attackSpeed.AddModifier(enrageAttackSpeedBonus, "BlackKnight_Enrage");
        stats?.defense.armor.AddModifier(10f, "BlackKnight_Enrage");

        // Roar impact and after-image echo effect
        vfx?.DoImageEchoEffect(1.5f);
        sfx?.PlayAttackHit();
    }

    // Attack execution helpers
    public void PerformAttackWithScale(DamageScaleData scaleData)
    {
        var prevScale = combat.basicAttackScale;
        if (scaleData != null)
            combat.basicAttackScale = scaleData;

        combat.PerformAttack();
        combat.basicAttackScale = prevScale;
    }

    public void PerformAttackOnTarget(Transform target, DamageScaleData scaleData)
    {
        var prevScale = combat.basicAttackScale;
        if (scaleData != null)
            combat.basicAttackScale = scaleData;

        combat.PerformAttackOnTarget(target, scaleData);
        combat.basicAttackScale = prevScale;
    }

    public void PerformSurroundingAttack(DamageScaleData scaleData, float radius, Vector3? origin = null)
    {
        bool targetGotHit = false;
        LayerMask targetMask = whatIsTarget.value != 0 ? whatIsTarget : LayerMask.GetMask("Player");
        Vector3 center = origin ?? transform.position;
        Collider2D[] targets = Physics2D.OverlapCircleAll(center, radius, targetMask);

        foreach (var hitCol in targets)
        {
            IDamageable damageable = hitCol.GetComponent<IDamageable>();
            if (damageable == null)
                continue;

            AttackData attackData = stats.GetAttackData(scaleData ?? combat.basicAttackScale);
            Entity_StatusHandler statusHandler = hitCol.GetComponent<Entity_StatusHandler>();

            float physicalDamage = attackData.phyiscalDamage;
            float elementalDamage = attackData.elementalDamage;
            ElementType element = attackData.element;

            targetGotHit = damageable.TakeDamage(physicalDamage, elementalDamage, element, transform);

            if (element != ElementType.None)
                statusHandler?.ApplyStatusEffect(element, attackData.effectData);

            if (targetGotHit)
            {
                vfx?.CreateOnHitVFX(hitCol.transform, attackData.isCrit, element);
                sfx?.PlayAttackHit();
            }
        }

        if (targetGotHit == false)
            sfx?.PlayAttackMiss();
    }

    public void PerformWeaponArtAttack()
    {
        // Wide forward slash with amplified damage scale
        PerformAttackWithScale(weaponArtDamageScale);
    }

    // Animation Event Callbacks
    public void OnComboHitTrigger() => bkComboAttackState?.OnComboHit();
    public void OnHeavyHitTrigger() => bkHeavyAttackState?.OnHeavyHit();
    public void OnRiposteHitTrigger() => bkParryState?.OnRiposteHit();
    public void OnWeaponArtHitTrigger() => bkWeaponArtState?.OnWeaponArtHit();
    public void OnEnrageBuffTrigger() => bkEnrageState?.OnEnrageBuff();
    public void OnRollEndTrigger() => CurrentStateAnimationTrigger();
    public void OnParryWindowStartTrigger() => isParrying = true;
    public void OnParryWindowEndTrigger() => isParrying = false;
}
