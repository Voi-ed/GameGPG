using UnityEngine;

public class Enemy_Sicklezeep : Enemy, ICounterable
{
    public bool CanBeCountered => canBeStunned;

    [Header("Sicklezeep - Combat Settings")]
    [SerializeField] private float attack1Distance = 1.5f;
    [SerializeField] private float attack2Distance = 2.0f;
    [SerializeField] private float comboLungeSpeed = 2.2f;

    [Header("Sweep Dash Skill (Low Scythe Reap)")]
    [SerializeField] private float sweepMinDistance = 2.2f;
    [SerializeField] private float sweepMaxDistance = 5.5f;
    [SerializeField] private float sweepSpeed = 10f;
    [SerializeField] private float sweepDuration = 0.25f;
    [SerializeField] private float sweepCooldown = 4.5f;

    [Header("Damage Scaling")]
    [SerializeField] private DamageScaleData attack1DamageScale;
    [SerializeField] private DamageScaleData attack2DamageScale;
    [SerializeField] private DamageScaleData sweepDamageScale;

    [Header("Combo Chain Settings")]
    [SerializeField] private int maxComboChains = 3;

    private float lastTimeAttacked = float.NegativeInfinity;
    private float lastTimeSweep = float.NegativeInfinity;
    public int currentComboChain { get; private set; } = 0;

    public float Attack1Distance => attack1Distance;
    public float Attack2Distance => attack2Distance;
    public float SweepMinDistance => sweepMinDistance;
    public float SweepMaxDistance => sweepMaxDistance;
    public float SweepSpeed => sweepSpeed;
    public float SweepDuration => sweepDuration;
    public float ComboLungeSpeed => comboLungeSpeed;

    public Sicklezeep_BattleState sickleBattleState { get; private set; }
    public Sicklezeep_Attack1State sickleAttack1State { get; private set; }
    public Sicklezeep_Attack2State sickleAttack2State { get; private set; }
    public Sicklezeep_SweepDashState sickleSweepDashState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "run");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");
        deadState = new Enemy_DeadState(this, stateMachine, "death");

        sickleBattleState = new Sicklezeep_BattleState(this, stateMachine, "idle");
        battleState = sickleBattleState;

        sickleAttack1State = new Sicklezeep_Attack1State(this, stateMachine, "attack1");
        attackState = sickleAttack1State;

        sickleAttack2State = new Sicklezeep_Attack2State(this, stateMachine, "attack2");
        sickleSweepDashState = new Sicklezeep_SweepDashState(this, stateMachine, "sweepAttack");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public bool CanAttack() => Time.time > lastTimeAttacked + attackCooldown;
    public void SetAttackOnCooldown()
    {
        lastTimeAttacked = Time.time;
        ResetCombo();
    }

    public void IncrementCombo() => currentComboChain++;
    public void ResetCombo() => currentComboChain = 0;
    public bool CanContinueCombo() => currentComboChain < maxComboChains;

    public bool CanSweepDash() => Time.time > lastTimeSweep + sweepCooldown;
    public void SetSweepDashOnCooldown() => lastTimeSweep = Time.time;

    public Transform GetPlayerTarget()
    {
        if (Player.instance != null)
            return Player.instance.transform;

        return GetPlayerReference();
    }

    public void PerformAttack1Damage() => PerformAttackWithScale(attack1DamageScale);
    public void PerformAttack2Damage() => PerformAttackWithScale(attack2DamageScale);
    public void PerformSweepDamage() => PerformAttackWithScale(sweepDamageScale);

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

    public void LungeForward(float speed = -1f)
    {
        float spd = speed > 0 ? speed : comboLungeSpeed;
        SetVelocity(spd * facingDir, rb.linearVelocity.y);
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attack1Distance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attack2Distance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, sweepMaxDistance);
    }
}
