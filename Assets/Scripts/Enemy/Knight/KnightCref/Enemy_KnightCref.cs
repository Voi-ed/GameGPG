using System.Collections.Generic;
using UnityEngine;

public class Enemy_KnightCref : Enemy, ICounterable
{
    public bool CanBeCountered => canBeStunned;

    [Header("Knight Cref - States")]
    public KnightCref_BattleState crefBattleState { get; private set; }
    public KnightCref_DashState crefDashState { get; private set; }
    public KnightCref_AttackState crefAttackState { get; private set; }
    public KnightCref_HeavySlamState crefHeavySlamState { get; private set; }

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 9f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 4f;
    [SerializeField] private float dashMinDistance = 3.2f;
    [SerializeField] private float dashMaxDistance = 6.0f;
    private float lastTimeDashed = float.NegativeInfinity;

    [Header("Heavy Slam Settings")]
    [SerializeField] private float heavySlamCooldown = 6f;
    [SerializeField] private float heavySlamRadius = 2.2f;
    [SerializeField] private Transform slamCheckCenter;
    [SerializeField] private GameObject groundSlamVfxPrefab;
    [SerializeField] private DamageScaleData heavySlamDamageScale;
    [SerializeField] private LayerMask whatIsTarget;
    private float lastTimeHeavySlam = float.NegativeInfinity;

    [Header("Attack Settings")]
    [SerializeField] private DamageScaleData attackDamageScale;

    public float DashSpeed => dashSpeed;
    public float DashDuration => dashDuration;
    public float DashMinDistance => dashMinDistance;
    public float DashMaxDistance => dashMaxDistance;

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "idle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "idle");
        deadState = new Enemy_DeadState(this, stateMachine, "death");

        crefBattleState = new KnightCref_BattleState(this, stateMachine, "idle");
        battleState = crefBattleState;

        crefDashState = new KnightCref_DashState(this, stateMachine, "idle");
        crefAttackState = new KnightCref_AttackState(this, stateMachine, "attack");
        attackState = crefAttackState;

        crefHeavySlamState = new KnightCref_HeavySlamState(this, stateMachine, "heavy_slam");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public bool CanDash() => Time.time > lastTimeDashed + dashCooldown;
    public void SetDashOnCooldown() => lastTimeDashed = Time.time;

    public bool CanHeavySlam() => Time.time > lastTimeHeavySlam + heavySlamCooldown;
    public void SetHeavySlamOnCooldown() => lastTimeHeavySlam = Time.time;

    public void PerformAttackDamage()
    {
        combat?.PerformAttack();
    }

    public void PerformHeavySlamDamage()
    {
        Vector3 slamPos = slamCheckCenter != null ? slamCheckCenter.position : transform.position;
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(slamPos, heavySlamRadius, whatIsTarget);

        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
        foreach (var hit in hitTargets)
        {
            if (hit.transform == transform)
                continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null && damagedTargets.Add(damageable))
            {
                combat?.PerformAttackOnTarget(hit.transform, heavySlamDamageScale);
            }
        }

        if (groundSlamVfxPrefab != null)
        {
            Instantiate(groundSlamVfxPrefab, slamPos, Quaternion.identity);
        }
    }

    public void HandleCounter()
    {
        if (!CanBeCountered)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (slamCheckCenter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(slamCheckCenter.position, heavySlamRadius);
        }
    }
}
