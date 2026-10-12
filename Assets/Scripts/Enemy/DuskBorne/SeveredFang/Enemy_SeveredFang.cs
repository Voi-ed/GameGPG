using UnityEngine;

public class Enemy_SeveredFang : Enemy, ICounterable
{
    public bool CanBeCountered => isCounterWindowOpen;

    [Header("Severed Fang - Combat Settings")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private DamageScaleData slashDamageScale;
    private float lastTimeAttacked = float.NegativeInfinity;

    [Header("Skill - Iaido Quick-Draw (Thế Tra Kiếm & Lướt Chém)")]
    [SerializeField] private float iaidoCooldown = 4.5f;
    [SerializeField] private float iaidoMinDistance = 2.5f;
    [SerializeField] private float iaidoMaxDistance = 7.0f;
    [SerializeField] private float sheatheDuration = 0.85f;
    [SerializeField] private float dashSpeed = 14.0f;
    [SerializeField] private float dashDuration = 0.22f;
    [SerializeField] private DamageScaleData iaidoDamageScale;
    private float lastTimeIaido = float.NegativeInfinity;

    [Header("Alert & Counter Settings")]
    [SerializeField] private bool isCounterWindowOpen = false;

    public float AttackRange => attackRange;
    public float IaidoMinDistance => iaidoMinDistance;
    public float IaidoMaxDistance => iaidoMaxDistance;
    public float SheatheDuration => sheatheDuration;
    public float DashSpeed => dashSpeed;
    public float DashDuration => dashDuration;

    public SeveredFang_BattleState severedFangBattleState { get; private set; }
    public SeveredFang_AttackState severedFangAttackState { get; private set; }
    public SeveredFang_IaidoState severedFangIaidoState { get; private set; }
    public SeveredFang_StunnedState severedFangStunnedState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "run");
        severedFangStunnedState = new SeveredFang_StunnedState(this, stateMachine, "hurt");
        stunnedState = severedFangStunnedState;
        deadState = new Enemy_DeadState(this, stateMachine, "death");

        severedFangBattleState = new SeveredFang_BattleState(this, stateMachine, "idle");
        battleState = severedFangBattleState;

        severedFangAttackState = new SeveredFang_AttackState(this, stateMachine, "attack");
        attackState = severedFangAttackState;

        severedFangIaidoState = new SeveredFang_IaidoState(this, stateMachine, "sheathe");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public bool CanAttack() => Time.time > lastTimeAttacked + attackCooldown;
    public void SetAttackOnCooldown() => lastTimeAttacked = Time.time;

    public bool CanIaido() => Time.time > lastTimeIaido + iaidoCooldown;
    public void SetIaidoOnCooldown() => lastTimeIaido = Time.time;

    public Transform GetPlayerTarget()
    {
        if (Player.instance != null)
            return Player.instance.transform;

        return GetPlayerReference();
    }

    public void EnableAttackAlert(bool enable)
    {
        Enemy_VFX enemyVfx = vfx as Enemy_VFX ?? GetComponent<Enemy_VFX>();
        if (enemyVfx != null)
        {
            enemyVfx.EnableAttackAlert(enable);
        }

        Transform alertTransform = transform.Find("AttackAlert_Simple") ?? transform.Find("AttackAlert");
        if (alertTransform != null)
        {
            if (enable)
            {
                alertTransform.localPosition = new Vector3(0f, 1.35f, 0f);
                if (alertTransform.TryGetComponent<SpriteRenderer>(out var sr))
                {
                    sr.sortingOrder = 30;
                }
            }
        }
    }

    public void SetCounterWindow(bool enable)
    {
        isCounterWindowOpen = enable;
        EnableAttackAlert(enable);
    }

    public void HandleCounter()
    {
        if (!CanBeCountered)
            return;

        SetCounterWindow(false);
        stateMachine.ChangeState(severedFangStunnedState);
    }

    public void PerformSlashDamage()
    {
        PerformAttackWithScale(slashDamageScale);
    }

    public void PerformIaidoDamage()
    {
        PerformAttackWithScale(iaidoDamageScale);
    }

    private void PerformAttackWithScale(DamageScaleData scaleData)
    {
        if (combat == null)
            return;

        var prevScale = combat.basicAttackScale;
        if (scaleData != null)
            combat.basicAttackScale = scaleData;

        combat.PerformAttack();
        combat.basicAttackScale = prevScale;
    }

    public override void EntityDeath()
    {
        SetCounterWindow(false);
        base.EntityDeath();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, iaidoMinDistance);
        Gizmos.DrawWireSphere(transform.position, iaidoMaxDistance);
    }
}
