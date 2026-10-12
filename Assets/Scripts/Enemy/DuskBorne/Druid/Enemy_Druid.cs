using UnityEngine;

public class Enemy_Druid : Enemy
{
    [Header("Druid - Earth Magic Settings")]
    [SerializeField] private GameObject earthSpikePrefab;
    [SerializeField] private float castCooldown = 3.5f;
    [SerializeField] private float minCastDistance = 2.5f;
    [SerializeField] private float maxCastDistance = 9.0f;
    [SerializeField] private float retreatDistance = 2.8f;
    [SerializeField] private DamageScaleData spikeDamageScale;
    private float lastTimeCast = float.NegativeInfinity;

    public float MinCastDistance => minCastDistance;
    public float MaxCastDistance => maxCastDistance;
    public float RetreatDistance => retreatDistance;

    public Druid_BattleState druidBattleState { get; private set; }
    public Druid_CastEarthState druidCastEarthState { get; private set; }
    public Druid_RetreatState druidRetreatState { get; private set; }
    public Druid_StunnedState druidStunnedState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "run");
        druidStunnedState = new Druid_StunnedState(this, stateMachine, "hurt");
        stunnedState = druidStunnedState;
        deadState = new Enemy_DeadState(this, stateMachine, "death");

        druidBattleState = new Druid_BattleState(this, stateMachine, "idle");
        battleState = druidBattleState;

        druidCastEarthState = new Druid_CastEarthState(this, stateMachine, "attack");
        attackState = druidCastEarthState;

        druidRetreatState = new Druid_RetreatState(this, stateMachine, "run");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public bool CanCast() => Time.time > lastTimeCast + castCooldown;
    public void SetCastOnCooldown() => lastTimeCast = Time.time;

    public Transform GetPlayerTarget()
    {
        if (Player.instance != null)
            return Player.instance.transform;

        return GetPlayerReference();
    }

    public void SpawnEarthSpikeUnderPlayer()
    {
        if (earthSpikePrefab == null)
            return;

        Transform target = GetPlayerTarget();
        Vector3 spawnPos = target != null ? target.position : transform.position + Vector3.right * facingDir * 3f;

        // Bắn tia raycast xuống đất để xác định chính xác cao độ mặt sàn
        RaycastHit2D hit = Physics2D.Raycast(spawnPos + Vector3.up * 1f, Vector2.down, 4f, whatIsGround);
        if (hit.collider != null)
        {
            spawnPos.y = hit.point.y + 0.84f;
        }

        GameObject spikeObj = Instantiate(earthSpikePrefab, spawnPos, Quaternion.identity);
        if (spikeObj.TryGetComponent<Druid_EarthSpike>(out var spike))
        {
            spike.Setup(this, spikeDamageScale, WhatIsPlayer);
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minCastDistance);
        Gizmos.DrawWireSphere(transform.position, maxCastDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}
