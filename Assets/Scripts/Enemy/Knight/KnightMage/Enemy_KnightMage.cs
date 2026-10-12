using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_KnightMage : Enemy, ICounterable
{
    public bool CanBeCountered => canBeStunned;

    public Enemy_KnightMageBattleState knightMageBattleState { get; private set; }
    public Enemy_KnightMageSpellCastState knightMageSpellCastState { get; private set; }
    public Enemy_KnightMageThornsState knightMageThornsState { get; private set; }
    public Enemy_KnightMageTeleportState knightMageTeleportState { get; private set; }

    [Header("Knight Mage - Spell Cast")]
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spellStartPosition;
    [SerializeField] private int amountToCast = 2;
    [SerializeField] private float spellCastInterval = 0.35f;
    [SerializeField] private float spellStateCooldown = 3.5f;
    public DamageScaleData spellDamageScale;
    public bool spellCastPerformed { get; private set; }
    private float lastTimeCastSpell = float.NegativeInfinity;
    private Coroutine spellCastCo;

    [Header("Knight Mage - Thorns Skill")]
    [SerializeField] private float thornsCooldown = 5f;
    [SerializeField] private float thornsTriggerDistance = 3f;
    [SerializeField] private Transform thornsCheckCenter;
    [SerializeField] private Vector2 thornsAreaSize = new Vector2(4.5f, 1.5f);
    [SerializeField] private LayerMask whatIsTarget;
    public DamageScaleData thornsDamageScale;
    private float lastTimeUsedThorns = float.NegativeInfinity;

    [Header("Knight Mage - Teleport")]
    [SerializeField] private float teleportCooldown = 6f;
    [SerializeField] private float teleportMinDistance = 4f;
    [SerializeField] private float teleportMaxDistance = 7f;
    [SerializeField] private Transform behindCollisionCheck;
    public bool teleportTrigger { get; private set; }
    private float lastTimeTeleported = float.NegativeInfinity;

    [Header("Knight Mage - Detection")]
    [SerializeField] private float detectionRadius = 10f;
    public float DetectionRadius => detectionRadius;

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        deadState = new Enemy_DeadState(this, stateMachine, "idle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");

        knightMageSpellCastState = new Enemy_KnightMageSpellCastState(this, stateMachine, "spellCast");
        knightMageThornsState = new Enemy_KnightMageThornsState(this, stateMachine, "thorns");
        knightMageTeleportState = new Enemy_KnightMageTeleportState(this, stateMachine, "teleport");
        knightMageBattleState = new Enemy_KnightMageBattleState(this, stateMachine, "battle");

        battleState = knightMageBattleState;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();

        // Chủ động dò Player ở cự ly gần để xoay người và vào Battle State
        if (stateMachine.currentState == idleState || stateMachine.currentState == moveState)
        {
            Transform targetPlayer = GetPlayerTarget();
            if (targetPlayer != null && Vector2.Distance(transform.position, targetPlayer.position) <= detectionRadius)
            {
                int dir = targetPlayer.position.x > transform.position.x ? 1 : -1;
                HandleFlip(dir);
                TryEnterBattleState(targetPlayer);
            }
        }
    }

    public Transform GetPlayerTarget()
    {
        if (player != null)
            return player;

        if (Player.instance != null)
            return Player.instance.transform;

        RaycastHit2D hit = PlayerDetected();
        if (hit.collider != null)
            return hit.transform;

        return null;
    }

    public override void TryEnterBattleState(Transform targetPlayer)
    {
        if (stateMachine.currentState == knightMageBattleState ||
            stateMachine.currentState == knightMageSpellCastState ||
            stateMachine.currentState == knightMageThornsState ||
            stateMachine.currentState == knightMageTeleportState ||
            stateMachine.currentState == deadState)
            return;

        base.TryEnterBattleState(targetPlayer);
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    #region Spell Cast Logic
    public bool CanCastSpell() => Time.time > lastTimeCastSpell + spellStateCooldown;
    public void SetSpellOnCooldown() => lastTimeCastSpell = Time.time;
    public void SetSpellCastPerformed(bool performed) => spellCastPerformed = performed;

    public override void SpecialAttack()
    {
        if (spellCastCo != null)
            StopCoroutine(spellCastCo);

        spellCastCo = StartCoroutine(CastSpellCo());
    }

    public void StopSpellCast()
    {
        if (spellCastCo != null)
        {
            StopCoroutine(spellCastCo);
            spellCastCo = null;
        }
    }

    private IEnumerator CastSpellCo()
    {
        for (int i = 0; i < amountToCast; i++)
        {
            if (spellPrefab != null)
            {
                Vector3 spawnPos = spellStartPosition != null ? spellStartPosition.position : transform.position;
                GameObject spellObj = Instantiate(spellPrefab, spawnPos, Quaternion.identity);

                Enemy_KnightMageProjectile projectile = spellObj.GetComponent<Enemy_KnightMageProjectile>();
                Transform targetPlayer = GetPlayerTarget();
                if (projectile != null && targetPlayer != null)
                {
                    projectile.SetupProjectile(targetPlayer, combat, spellDamageScale);
                }
            }

            if (i < amountToCast - 1)
                yield return new WaitForSeconds(spellCastInterval);
        }

        SetSpellCastPerformed(true);
        spellCastCo = null;
    }
    #endregion

    #region Thorns Logic
    public bool CanUseThorns() => Time.time > lastTimeUsedThorns + thornsCooldown;
    public void SetThornsOnCooldown() => lastTimeUsedThorns = Time.time;
    public float GetThornsTriggerDistance() => thornsTriggerDistance;

    public void PerformThornsDamage()
    {
        Vector3 center = thornsCheckCenter != null ? thornsCheckCenter.position : transform.position;
        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(center, thornsAreaSize, 0f, whatIsTarget);

        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
        foreach (var hit in hitTargets)
        {
            if (hit.transform == transform)
                continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null && damagedTargets.Add(damageable))
            {
                combat?.PerformAttackOnTarget(hit.transform, thornsDamageScale);
            }
        }
    }
    #endregion

    #region Teleport Logic
    public bool CanTeleport() => Time.time > lastTimeTeleported + teleportCooldown;
    public void SetTeleportOnCooldown() => lastTimeTeleported = Time.time;
    public void SetTeleportTrigger(bool triggerStatus) => teleportTrigger = triggerStatus;

    public Vector3 FindTeleportPoint()
    {
        Transform targetPlayer = GetPlayerTarget();

        if (targetPlayer == null)
            return transform.position;

        int playerDirection = targetPlayer.position.x > transform.position.x ? 1 : -1;
        // Ưu tiên dịch chuyển ra xa player (ngược hướng player đang đứng)
        float preferredX = transform.position.x - (playerDirection * Random.Range(teleportMinDistance, teleportMaxDistance));

        // Bắn Raycast từ trên xuống để tìm bề mặt đất
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(preferredX, transform.position.y + 2f), Vector2.down, 5f, whatIsGround);

        if (hit.collider != null)
        {
            float halfHeight = col != null ? col.bounds.extents.y : 0.5f;
            return new Vector3(hit.point.x, hit.point.y + halfHeight, transform.position.z);
        }

        // Nếu hướng ngược bị rơi ra ngoài map, thử dịch chuyển theo hướng đối diện (sau lưng player)
        float alternativeX = targetPlayer.position.x + (playerDirection * Random.Range(teleportMinDistance, teleportMaxDistance));
        RaycastHit2D altHit = Physics2D.Raycast(new Vector2(alternativeX, transform.position.y + 2f), Vector2.down, 5f, whatIsGround);

        if (altHit.collider != null)
        {
            float halfHeight = col != null ? col.bounds.extents.y : 0.5f;
            return new Vector3(altHit.point.x, altHit.point.y + halfHeight, transform.position.z);
        }

        return transform.position;
    }
    #endregion

    public bool CantMoveBackwards()
    {
        if (behindCollisionCheck == null)
            return false;

        bool detectedWall = Physics2D.Raycast(behindCollisionCheck.position, Vector2.right * -facingDir, 1.5f, whatIsGround);
        bool noGround = Physics2D.Raycast(behindCollisionCheck.position, Vector2.down, 1.5f, whatIsGround) == false;

        return noGround || detectedWall;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Thorns area gizmo
        Gizmos.color = Color.magenta;
        Vector3 thornsCenter = thornsCheckCenter != null ? thornsCheckCenter.position : transform.position;
        Gizmos.DrawWireCube(thornsCenter, thornsAreaSize);

        // Spell spawn position
        if (spellStartPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spellStartPosition.position, 0.2f);
        }

        // Behind collision check gizmo
        if (behindCollisionCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(behindCollisionCheck.position,
                new Vector3(behindCollisionCheck.position.x + (1.5f * -facingDir), behindCollisionCheck.position.y));

            Gizmos.DrawLine(behindCollisionCheck.position,
                new Vector3(behindCollisionCheck.position.x, behindCollisionCheck.position.y - 1.5f));
        }
    }
}
