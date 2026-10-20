using System.Collections;
using UnityEngine;

/// <summary>
/// Boss The Void Seeker (Hư Không Sát Thủ)
/// Sát thủ bóng tối thuộc nhóm Boss, sở hữu tốc độ cao, khả năng lướt bóng ma (Shadow Step)
/// và đòn nhảy bổ cắm kiếm chấn động Hư Không.
/// </summary>
public class Enemy_Boss_Seeker : Enemy, ICounterable
{
    public bool CanBeCountered => canBeStunned;

    public void HandleCounter()
    {
        if (!CanBeCountered)
            return;

        stateMachine.ChangeState(seekerStunnedState);
    }

    [Header("--- Seeker Movement & Phases ---")]
    [SerializeField] private float runSpeed = 4.5f;
    [SerializeField] private float phase2SpeedMultiplier = 1.3f;
    [SerializeField] private float phase2CooldownMultiplier = 0.65f;

    [Header("--- Skill 1: Void Slash (Basic Attack) ---")]
    [SerializeField] private DamageScaleData voidSlashDamageScale;
    [SerializeField] private float attackRange = 2.2f;

    [Header("--- Skill 2: Phantom Shadow Step (Dash) ---")]
    [SerializeField] private float dashSpeed = 16.0f;
    [SerializeField] private float dashDuration = 0.22f;
    [SerializeField] private float dashCooldown = 3.0f;

    [Header("--- Skill 3: Void Leap & Plunge ---")]
    [SerializeField] private DamageScaleData leapPlungeDamageScale;
    [SerializeField] private float leapCooldown = 6.5f;
    [SerializeField] private float leapUpVelocity = 15.0f;
    [SerializeField] private float plungeDownVelocity = -22.0f;
    [SerializeField] private float plungeImpactRadius = 3.0f;

    [Header("--- Anchors & Checks ---")]
    [SerializeField] private Transform attackCheck;
    [SerializeField] private float attackCheckRadius = 1.2f;

    // Timers
    private float lastTimeAttack = float.NegativeInfinity;
    private float lastTimeDash = float.NegativeInfinity;
    private float lastTimeLeap = float.NegativeInfinity;

    // State & Phase Management
    public bool isPhase2 { get; private set; } = false;
    public bool hasHyperArmor { get; set; } = false;

    // Sub-components
    public Enemy_Boss_SeekerHealth seekerHealth { get; private set; }

    // FSM States
    public Seeker_BattleState seekerBattleState { get; private set; }
    public Seeker_AttackState seekerAttackState { get; private set; }
    public Seeker_DashState seekerDashState { get; private set; }
    public Seeker_JumpAttackState seekerJumpAttackState { get; private set; }
    public Seeker_StunnedState seekerStunnedState { get; private set; }
    public Seeker_DeadState seekerDeadState { get; private set; }

    // Properties
    public float RunSpeed => (isPhase2 ? runSpeed * phase2SpeedMultiplier : runSpeed) * activeSlowMultiplier;
    public float DashSpeed => (isPhase2 ? dashSpeed * phase2SpeedMultiplier : dashSpeed) * activeSlowMultiplier;
    public float DashDuration => dashDuration;
    public float LeapUpVelocity => leapUpVelocity;
    public float PlungeDownVelocity => plungeDownVelocity;
    public float PlungeImpactRadius => plungeImpactRadius;
    public float AttackRange => attackRange;
    public DamageScaleData VoidSlashDamageScale => voidSlashDamageScale;
    public DamageScaleData LeapPlungeDamageScale => leapPlungeDamageScale;
    public Transform AttackCheck => attackCheck != null ? attackCheck : transform;

    protected override void Awake()
    {
        base.Awake();

        seekerHealth = GetComponent<Enemy_Boss_SeekerHealth>();

        attackDistance = attackRange;
        attackCooldown = 0.85f;
        minRetreatDistance = 0f;
        retreatVelocity = Vector2.zero;

        // Khởi tạo các trạng thái FSM
        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");

        seekerBattleState = new Seeker_BattleState(this, stateMachine, "battle");
        battleState = seekerBattleState;

        seekerAttackState = new Seeker_AttackState(this, stateMachine, "attack");
        seekerDashState = new Seeker_DashState(this, stateMachine, "dash");
        seekerJumpAttackState = new Seeker_JumpAttackState(this, stateMachine, "jump");
        seekerStunnedState = new Seeker_StunnedState(this, stateMachine, "hurt");
        stunnedState = seekerStunnedState;
        seekerDeadState = new Seeker_DeadState(this, stateMachine, "dead");
        deadState = seekerDeadState;
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
            anim.SetFloat("attackSpeedMultiplier", 1.3f);
        }

        // Tạo hiệu ứng chấn động hoặc aura tím Hư Không
        StartCoroutine(Phase2FlashRoutine());
    }

    private IEnumerator Phase2FlashRoutine()
    {
        // Nhấp nháy màu tím sẫm Hư Không khi thức tỉnh Phase 2
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color origColor = sr.color;
            for (int i = 0; i < 4; i++)
            {
                sr.color = new Color(0.7f, 0.2f, 1f, 1f);
                yield return new WaitForSeconds(0.1f);
                sr.color = origColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public bool CanAttack()
    {
        float cd = isPhase2 ? attackCooldown * phase2CooldownMultiplier : attackCooldown;
        return Time.time >= lastTimeAttack + cd;
    }

    public void RecordAttackTime() => lastTimeAttack = Time.time;

    public bool CanDash()
    {
        float cd = isPhase2 ? dashCooldown * phase2CooldownMultiplier : dashCooldown;
        return Time.time >= lastTimeDash + cd;
    }

    public void RecordDashTime() => lastTimeDash = Time.time;

    public bool CanJumpAttack()
    {
        float cd = isPhase2 ? leapCooldown * phase2CooldownMultiplier : leapCooldown;
        return Time.time >= lastTimeLeap + cd;
    }

    public void RecordJumpAttackTime() => lastTimeLeap = Time.time;

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

    /// <summary>
    /// Thực hiện đòn chém Void Slash cơ bản
    /// </summary>
    public void PerformVoidSlash()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(AttackCheck.position, attackCheckRadius, WhatIsPlayer);
        foreach (var col in targets)
        {
            if (col != null)
            {
                DamageScaleData scale = voidSlashDamageScale != null ? voidSlashDamageScale : new DamageScaleData { phyiscal = 1.2f, elemental = 0.5f };
                combat.PerformAttackOnTarget(col.transform, scale);
            }
        }
    }

    /// <summary>
    /// Thực hiện chấn động khi cắm kiếm tiếp đất từ trên không (Void Plunge Impact)
    /// </summary>
    public void PerformPlungeImpact()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, plungeImpactRadius, WhatIsPlayer);
        foreach (var col in targets)
        {
            if (col != null)
            {
                DamageScaleData scale = leapPlungeDamageScale != null ? leapPlungeDamageScale : new DamageScaleData { phyiscal = 1.8f, elemental = 1.0f };
                combat.PerformAttackOnTarget(col.transform, scale);
            }
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (attackCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackCheck.position, attackCheckRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, plungeImpactRadius);
    }
}
