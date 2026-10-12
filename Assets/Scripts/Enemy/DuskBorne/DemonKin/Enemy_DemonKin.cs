using UnityEngine;

public class Enemy_DemonKin : Enemy
{
    [Header("Demon Kin - Combat Settings")]
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private DamageScaleData smashDamageScale;
    private float lastTimeAttacked = float.NegativeInfinity;

    [Header("Skill - Bull Rush Charge (Húc Càn Áp Sát)")]
    [SerializeField] private float chargeSpeed = 7.5f;
    [SerializeField] private float chargeDuration = 0.5f;
    [SerializeField] private float chargeCooldown = 5.0f;
    [SerializeField] private float chargeMinDistance = 3.0f;
    [SerializeField] private float chargeMaxDistance = 6.5f;
    private float lastTimeCharge = float.NegativeInfinity;

    [Header("Enrage Phase (< 50% HP)")]
    [SerializeField] private bool isEnraged = false;

    public float AttackRange => attackRange;
    public float ChargeSpeed => isEnraged ? chargeSpeed * 1.3f : chargeSpeed;
    public float ChargeDuration => chargeDuration;
    public float ChargeMinDistance => chargeMinDistance;
    public float ChargeMaxDistance => chargeMaxDistance;
    public bool IsEnraged => isEnraged;
    public bool hasHyperArmor { get; set; } = false;

    public DemonKin_BattleState demonKinBattleState { get; private set; }
    public DemonKin_AttackState demonKinAttackState { get; private set; }
    public DemonKin_ChargeState demonKinChargeState { get; private set; }
    public DemonKin_StunnedState demonKinStunnedState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "run");
        demonKinStunnedState = new DemonKin_StunnedState(this, stateMachine, "hurt");
        stunnedState = demonKinStunnedState;
        deadState = new Enemy_DeadState(this, stateMachine, "death");

        demonKinBattleState = new DemonKin_BattleState(this, stateMachine, "idle");
        battleState = demonKinBattleState;

        demonKinAttackState = new DemonKin_AttackState(this, stateMachine, "attack");
        attackState = demonKinAttackState;

        demonKinChargeState = new DemonKin_ChargeState(this, stateMachine, "run");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public void CheckEnrageCondition()
    {
        if (health != null && stats != null && !isEnraged)
        {
            float maxHp = stats.GetMaxHealth();
            if (maxHp > 0 && health.GetCurrentHealth() / maxHp <= 0.5f)
            {
                isEnraged = true;
            }
        }
    }

    public bool CanAttack() => Time.time > lastTimeAttacked + (isEnraged ? attackCooldown * 0.7f : attackCooldown);
    public void SetAttackOnCooldown() => lastTimeAttacked = Time.time;

    public bool CanCharge() => Time.time > lastTimeCharge + chargeCooldown;
    public void SetChargeOnCooldown() => lastTimeCharge = Time.time;

    public Transform GetPlayerTarget()
    {
        if (Player.instance != null)
            return Player.instance.transform;

        return GetPlayerReference();
    }

    public void PerformSmashDamage()
    {
        if (combat == null)
            return;

        var prevScale = combat.basicAttackScale;
        if (smashDamageScale != null)
            combat.basicAttackScale = smashDamageScale;

        combat.PerformAttack();
        combat.basicAttackScale = prevScale;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeMinDistance);
        Gizmos.DrawWireSphere(transform.position, chargeMaxDistance);
    }
}
