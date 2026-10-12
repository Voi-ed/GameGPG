using UnityEngine;

public class Enemy_ArchDemon : Enemy
{
    [Header("Arch Demon - Dark Magic Settings")]
    [SerializeField] private GameObject darkOrbPrefab;
    [SerializeField] private ArchDemon_DarkBreathEmitter darkBreathEmitter;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float hoverHeight = 0.4f;
    [SerializeField] private float spellcastCooldown = 4.2f;
    [SerializeField] private float closeDangerDistance = 2.2f;
    [SerializeField] private float teleportCooldown = 5.5f;
    [SerializeField] private DamageScaleData spellDamageScale;
    private float lastTimeSpellcast = float.NegativeInfinity;
    private float lastTimeTeleport = float.NegativeInfinity;

    public float HoverHeight => hoverHeight;
    public float CloseDangerDistance => closeDangerDistance;
    public bool hasHyperArmor { get; set; } = false;

    public ArchDemon_BattleState archDemonBattleState { get; private set; }
    public ArchDemon_SpellcastState archDemonSpellcastState { get; private set; }
    public ArchDemon_GlideTeleportState archDemonGlideTeleportState { get; private set; }
    public ArchDemon_StunnedState archDemonStunnedState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        // ArchDemon luôn bay lơ lửng, vô hiệu hóa trọng lực
        if (rb != null)
            rb.gravityScale = 0f;

        if (darkBreathEmitter == null && firePoint != null)
        {
            darkBreathEmitter = firePoint.GetComponent<ArchDemon_DarkBreathEmitter>();
            if (darkBreathEmitter == null)
                darkBreathEmitter = firePoint.gameObject.AddComponent<ArchDemon_DarkBreathEmitter>();
        }

        if (darkBreathEmitter != null)
        {
            darkBreathEmitter.Setup(this, spellDamageScale, WhatIsPlayer);
        }

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "idle"); // Dùng idle lơ lửng cho move
        archDemonStunnedState = new ArchDemon_StunnedState(this, stateMachine, "hurt");
        stunnedState = archDemonStunnedState;
        deadState = new Enemy_DeadState(this, stateMachine, "death");

        archDemonBattleState = new ArchDemon_BattleState(this, stateMachine, "idle");
        battleState = archDemonBattleState;

        archDemonSpellcastState = new ArchDemon_SpellcastState(this, stateMachine, "attack");
        attackState = archDemonSpellcastState;

        archDemonGlideTeleportState = new ArchDemon_GlideTeleportState(this, stateMachine, "idle");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
        UpdateHoverHeight();
    }

    private void UpdateHoverHeight()
    {
        if (stateMachine.currentState == deadState || (health != null && health.isDead))
            return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, whatIsGround);
        if (hit.collider != null)
        {
            // CapsuleCollider2D nửa chiều cao là 1.0f -> chân cách sàn chính xác là hoverHeight
            float targetY = hit.point.y + 1.0f + hoverHeight;
            float diff = targetY - transform.position.y;
            float vy = Mathf.Clamp(diff * 3.5f, -2.5f, 2.5f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, vy);
        }
    }

    public void StartDarkBreath()
    {
        darkBreathEmitter?.StartBreath();
    }

    public void StopDarkBreath()
    {
        darkBreathEmitter?.StopBreath();
    }

    public void ForceKillDarkBreath()
    {
        darkBreathEmitter?.ForceKillBreath();
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        ForceKillDarkBreath();
        if (rb != null)
            rb.gravityScale = 1f; // Khi chết rơi xuống đất
    }

    public bool CanSpellcast() => Time.time > lastTimeSpellcast + spellcastCooldown;
    public void SetSpellcastOnCooldown() => lastTimeSpellcast = Time.time;

    public bool CanTeleport() => Time.time > lastTimeTeleport + teleportCooldown;
    public void SetTeleportOnCooldown() => lastTimeTeleport = Time.time;

    public Transform GetPlayerTarget()
    {
        if (Player.instance != null)
            return Player.instance.transform;

        return GetPlayerReference();
    }

    public void FireDarkOrbAtPlayer()
    {
        if (darkOrbPrefab == null)
            return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Transform target = GetPlayerTarget();
        Vector2 dir = target != null ? (target.position - spawnPos).normalized : (Vector2.right * facingDir);

        GameObject orbObj = Instantiate(darkOrbPrefab, spawnPos, Quaternion.identity);
        if (orbObj.TryGetComponent<ArchDemon_DarkOrb>(out var orb))
        {
            orb.Launch(this, dir, spellDamageScale, WhatIsPlayer, whatIsGround);
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, closeDangerDistance);
    }
}
