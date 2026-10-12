using UnityEngine;

public class Enemy_SamuraiZipp : Enemy, ICounterable
{
    public bool CanBeCountered => canBeStunned;

    [Header("Samurai Zipp - Combat Settings")]
    [SerializeField] private float attack1Distance = 1.5f;
    [SerializeField] private float attack2Distance = 1.6f;
    [SerializeField] private float thrustMinDistance = 2.2f;
    [SerializeField] private float thrustMaxDistance = 4.5f;
    [SerializeField] private float thrustSpeed = 14f;
    [SerializeField] private float thrustDuration = 0.22f;
    [SerializeField] private float thrustCooldown = 5f;
    [SerializeField] private float comboLungeSpeed = 2f;

    [Header("Damage Scaling")]
    [SerializeField] private DamageScaleData attack1DamageScale;
    [SerializeField] private DamageScaleData attack2DamageScale;
    [SerializeField] private DamageScaleData thrustDamageScale;

    [Header("Combo Chain Settings")]
    [SerializeField] private int maxComboChains = 3;

    private float lastTimeAttacked = float.NegativeInfinity;
    private float lastTimeThrust = float.NegativeInfinity;
    public int currentComboChain { get; private set; } = 0;

    public float Attack1Distance => attack1Distance;
    public float Attack2Distance => attack2Distance;
    public float ThrustMinDistance => thrustMinDistance;
    public float ThrustMaxDistance => thrustMaxDistance;
    public float ThrustSpeed => thrustSpeed;
    public float ThrustDuration => thrustDuration;
    public float ComboLungeSpeed => comboLungeSpeed;

    public SamuraiZipp_BattleState samuraiBattleState { get; private set; }
    public SamuraiZipp_Attack1State samuraiAttack1State { get; private set; }
    public SamuraiZipp_Attack2State samuraiAttack2State { get; private set; }
    public SamuraiZipp_ThrustState samuraiThrustState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "run");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");
        deadState = new Enemy_DeadState(this, stateMachine, "death");

        samuraiBattleState = new SamuraiZipp_BattleState(this, stateMachine, "idle");
        battleState = samuraiBattleState;

        samuraiAttack1State = new SamuraiZipp_Attack1State(this, stateMachine, "attack1");
        attackState = samuraiAttack1State;

        samuraiAttack2State = new SamuraiZipp_Attack2State(this, stateMachine, "attack2");
        samuraiThrustState = new SamuraiZipp_ThrustState(this, stateMachine, "thrust");
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

    public bool CanThrust() => Time.time > lastTimeThrust + thrustCooldown;
    public void SetThrustOnCooldown() => lastTimeThrust = Time.time;

    public Transform GetPlayerTarget()
    {
        if (Player.instance != null)
            return Player.instance.transform;

        return GetPlayerReference();
    }

    public void PerformAttack1Damage() => PerformAttackWithScale(attack1DamageScale);
    public void PerformAttack2Damage() => PerformAttackWithScale(attack2DamageScale);
    public void PerformThrustDamage() => PerformAttackWithScale(thrustDamageScale);

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
        Gizmos.DrawWireSphere(transform.position, thrustMaxDistance);
    }
}
