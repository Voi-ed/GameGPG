using System.Collections;
using UnityEngine;

/// <summary>
/// Boss The Crimson Blade (Kiếm Sĩ Huyết Sắc)
/// Một đại sư kiếm thuật cổ xưa mang sức mạnh Huyết Tinh.
/// Sở hữu bộ kiếm pháp song kích trảm, kiếm khí xuyên thấu và thế thủ phản đòn chết chóc.
/// </summary>
public class Enemy_Boss_CrimsonBlade : Enemy, ICounterable
{
    public bool CanBeCountered => canBeStunned;

    public void HandleCounter()
    {
        if (!CanBeCountered)
            return;

        stateMachine.ChangeState(crimsonStunnedState);
    }

    [Header("--- Crimson Movement & Phases ---")]
    [SerializeField] private float runSpeed = 4.0f;
    [SerializeField] private float phase2SpeedMultiplier = 1.25f;
    [SerializeField] private float phase2CooldownMultiplier = 0.7f;

    [Header("--- Skill 1: Twin Crimson Slash (Combo Attack) ---")]
    [SerializeField] private DamageScaleData comboSlashDamageScale;
    [SerializeField] private float comboAttackRange = 2.4f;

    [Header("--- Skill 2: Blood Blink Strike (Dash Slash) ---")]
    [SerializeField] private DamageScaleData blinkSlashDamageScale;
    [SerializeField] private float blinkSpeed = 18.0f;
    [SerializeField] private float blinkDuration = 0.35f;
    [SerializeField] private float blinkCooldown = 6.0f;
    [SerializeField] private Transform blinkSlashCheck;
    [SerializeField] private Vector2 blinkSlashBoxSize = new Vector2(3.5f, 2.0f);
    [SerializeField] private Vector2 blinkSlashBoxOffset = new Vector2(1.5f, 0.0f);

    [Header("--- Skill 3: Blood Guard (Parry Stance) ---")]
    [SerializeField] private float parryStanceDuration = 1.2f;
    [SerializeField] private float parryCooldown = 8.0f;

    [Header("--- Checks & Points ---")]
    [SerializeField] private Transform attackCheck;
    [SerializeField] private float attackCheckRadius = 1.5f;

    // Timers
    private float lastTimeAttack = float.NegativeInfinity;
    private float lastTimeBlink = float.NegativeInfinity;
    private float lastTimeParry = float.NegativeInfinity;

    // State & Phase Management
    public bool isPhase2 { get; private set; } = false;
    public bool hasHyperArmor { get; set; } = false;
    public bool isParryGuarding { get; set; } = false;

    // Sub-components
    public Enemy_Boss_CrimsonBladeHealth crimsonHealth { get; private set; }

    // FSM States
    public CrimsonBlade_BattleState crimsonBattleState { get; private set; }
    public CrimsonBlade_ComboAttackState crimsonComboAttackState { get; private set; }
    public CrimsonBlade_BlinkSlashState crimsonBlinkSlashState { get; private set; }
    public CrimsonBlade_ParryStanceState crimsonParryStanceState { get; private set; }
    public CrimsonBlade_StunnedState crimsonStunnedState { get; private set; }
    public CrimsonBlade_DeadState crimsonDeadState { get; private set; }

    // Properties
    public float RunSpeed => (isPhase2 ? runSpeed * phase2SpeedMultiplier : runSpeed) * activeSlowMultiplier;
    public float BlinkSpeed => (isPhase2 ? blinkSpeed * phase2SpeedMultiplier : blinkSpeed) * activeSlowMultiplier;
    public float BlinkDuration => blinkDuration;
    public float ParryStanceDuration => parryStanceDuration;
    public float ComboAttackRange => comboAttackRange;
    public float AttackCheckRadius => attackCheckRadius;
    public DamageScaleData BlinkSlashDamageScale => blinkSlashDamageScale;
    public Transform AttackCheck => attackCheck != null ? attackCheck : transform;
    public Transform BlinkSlashCheck => blinkSlashCheck != null ? blinkSlashCheck : transform;
    public Vector2 BlinkSlashBoxSize => blinkSlashBoxSize;
    public Vector3 BlinkSlashCheckPosition
    {
        get
        {
            Vector3 origin = blinkSlashCheck != null ? blinkSlashCheck.position : transform.position;
            return origin + new Vector3(blinkSlashBoxOffset.x * facingDir, blinkSlashBoxOffset.y, 0f);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        crimsonHealth = GetComponent<Enemy_Boss_CrimsonBladeHealth>();

        attackDistance = comboAttackRange;
        attackCooldown = 0.85f;
        minRetreatDistance = 0f;
        retreatVelocity = Vector2.zero;

        // Khởi tạo các trạng thái FSM
        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");

        crimsonBattleState = new CrimsonBlade_BattleState(this, stateMachine, "battle");
        battleState = crimsonBattleState;

        crimsonComboAttackState = new CrimsonBlade_ComboAttackState(this, stateMachine, "attack");
        crimsonBlinkSlashState = new CrimsonBlade_BlinkSlashState(this, stateMachine, "blink");
        crimsonParryStanceState = new CrimsonBlade_ParryStanceState(this, stateMachine, "parry");
        crimsonStunnedState = new CrimsonBlade_StunnedState(this, stateMachine, "hurt");
        stunnedState = crimsonStunnedState;
        crimsonDeadState = new CrimsonBlade_DeadState(this, stateMachine, "dead");
        deadState = crimsonDeadState;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public void TriggerPhase2()
    {
        if (isPhase2) return;
        isPhase2 = true;

        if (anim != null)
        {
            anim.speed = 1.2f;
        }

        StartCoroutine(Phase2BloodAuraRoutine());
    }

    private IEnumerator Phase2BloodAuraRoutine()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color origColor = sr.color;
            for (int i = 0; i < 4; i++)
            {
                sr.color = new Color(1f, 0.2f, 0.2f, 1f);
                yield return new WaitForSeconds(0.12f);
                sr.color = origColor;
                yield return new WaitForSeconds(0.12f);
            }
        }
    }

    public bool CanAttack()
    {
        float cd = isPhase2 ? attackCooldown * phase2CooldownMultiplier : attackCooldown;
        return Time.time >= lastTimeAttack + cd;
    }

    public void RecordAttackTime() => lastTimeAttack = Time.time;

    public bool CanBlink()
    {
        float cd = isPhase2 ? blinkCooldown * phase2CooldownMultiplier : blinkCooldown;
        return Time.time >= lastTimeBlink + cd;
    }

    public void RecordBlinkTime() => lastTimeBlink = Time.time;

    public bool CanParry()
    {
        float cd = isPhase2 ? parryCooldown * phase2CooldownMultiplier : parryCooldown;
        return Time.time >= lastTimeParry + cd;
    }

    public void RecordParryTime() => lastTimeParry = Time.time;

    public Transform GetPlayerTarget()
    {
        if (player != null)
            return player;

        Transform p = GetPlayerReference();
        if (p != null)
            return p;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, 15f, WhatIsPlayer);
        return hit != null ? hit.transform : null;
    }

    public void EnableAttackAlert(bool enable)
    {
        (vfx as Enemy_VFX)?.EnableAttackAlert(enable);
        EnableCounterWindow(enable);
    }

    /// <summary>
    /// Thực hiện đòn chém Song Kích Trảm
    /// </summary>
    public void PerformComboSlash()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(AttackCheck.position, attackCheckRadius, WhatIsPlayer);
        foreach (var col in targets)
        {
            if (col != null)
            {
                DamageScaleData scale = comboSlashDamageScale != null ? comboSlashDamageScale : new DamageScaleData 
                { 
                    phyiscal = 1.3f, 
                    elemental = 0.6f,
                    burnDuratin = 3f,
                    burnDamageScale = 1.2f
                };
                combat.PerformAttackOnTarget(col.transform, scale);
            }
        }
    }

    /// <summary>
    /// Thực hiện đòn chém lướt xuyên thấu (Blink Strike)
    /// </summary>
    public void PerformBlinkSlash(Transform specificTarget = null)
    {
        DamageScaleData scale = blinkSlashDamageScale != null ? blinkSlashDamageScale : new DamageScaleData 
        { 
            phyiscal = 2.0f, 
            elemental = 1.0f,
            burnDuratin = 4f,
            burnDamageScale = 1.5f
        };

        if (specificTarget != null)
        {
            combat.PerformAttackOnTarget(specificTarget, scale);
            return;
        }

        Collider2D[] targets = Physics2D.OverlapBoxAll(BlinkSlashCheckPosition, blinkSlashBoxSize, 0f, WhatIsPlayer);
        foreach (var col in targets)
        {
            if (col != null)
            {
                combat.PerformAttackOnTarget(col.transform, scale);
            }
        }
    }

    public override void SpecialAttack()
    {
        base.SpecialAttack();
        PerformBlinkSlash();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // 1. Vùng kiểm tra đòn chém cơ bản / Combo (Màu đỏ)
        if (attackCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackCheck.position, attackCheckRadius);
        }

        // 2. Vùng kiểm tra va chạm hình chữ nhật của chiêu lướt Blink Slash (Màu xanh Cyan, quét phía trước theo 1 hướng)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(BlinkSlashCheckPosition, blinkSlashBoxSize);
    }
}
