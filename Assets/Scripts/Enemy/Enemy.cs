using System.Collections;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Quest Info")]
    public string questTargetId;


    public Entity_Stats stats { get; private set; }
    public Enemy_Health health { get; private set; }
    public Entity_Combat combat { get; private set; }
    public Entity_VFX vfx { get; private set; }
    public Enemy_IdleState idleState;
    public Enemy_MoveState moveState;
    public Enemy_AttackState attackState;
    public Enemy_BattleState battleState;
    public Enemy_DeadState deadState;
    public Enemy_StunnedState stunnedState;

    [Header("Battle details")]
    public float battleMoveSpeed = 3;
    public float attackDistance = 2;
    public float attackCooldown = .5f;
    public bool canChasePlayer = true;
    [Space]
    public float battleTimeDuration = 5;
    public float minRetreatDistance = 1;
    public Vector2 retreatVelocity;

    [Header("Stunned state details")]
    public float stunnedDuration = 1;
    public Vector2 stunnedVelocity = new Vector2(7, 7);
    [SerializeField] protected bool canBeStunned;

    [Header("Movement details")]
    public float idleTime = 2;
    public float moveSpeed = 1.4f;
    [Range(0,2)]
    public float moveAnimSpeedMultiplier = 1;

    [Header("Player detection")]
    [SerializeField] private LayerMask whatIsPlayer;
    public LayerMask WhatIsPlayer => whatIsPlayer;
    [SerializeField] private Transform playerCheck;
    [SerializeField] private float playerCheckDistance = 10;
    public Transform player { get; private set; }
    public float activeSlowMultiplier { get; private set; } = 1;

    [Header("Randomized Offset Details")]
    public float attackDistanceOffset { get; private set; }
    public float moveSpeedVariation { get; private set; } = 1f;
    public float attackSpeedVariation { get; private set; } = 1f;
    public float attackCooldownVariation { get; private set; } = 1f;

    public float GetMoveSpeed() => moveSpeed * activeSlowMultiplier * moveSpeedVariation;
    public float GetBattleMoveSpeed() => battleMoveSpeed * activeSlowMultiplier * moveSpeedVariation;
    public float GetAttackDistance() => Mathf.Max(0.8f, attackDistance + attackDistanceOffset);
    public float GetAttackCooldown() => Mathf.Max(0.2f, (attackCooldown * attackCooldownVariation) + Random.Range(-0.06f, 0.1f));
    public float GetAttackSpeedMultiplier()
    {
        float baseSpeed = (stats != null && stats.offense != null) ? stats.offense.attackSpeed.GetValue() : 1f;
        return baseSpeed * attackSpeedVariation;
    }

    public bool isTimeFrozen { get; private set; }
    private float preFreezeAnimSpeed = 1f;
    private float preFreezeGravityScale = 1f;

    protected override void Awake()
    {
        base.Awake();
        health = GetComponent<Enemy_Health>();
        stats = GetComponent<Entity_Stats>();
        combat = GetComponent<Entity_Combat>();
        vfx = GetComponent<Entity_VFX>();
    }

    protected override void Start()
    {
        base.Start();

        // Độ lệch cự ly, tốc độ di chuyển và tốc độ ra đòn ngẫu nhiên nhẹ giữa các con quái
        attackDistanceOffset = Random.Range(-0.3f, 0.45f);
        moveSpeedVariation = Random.Range(0.92f, 1.08f);
        attackSpeedVariation = Random.Range(0.88f, 1.15f);
        attackCooldownVariation = Random.Range(0.85f, 1.25f);

        // Đảm bảo độ sâu hình ảnh hiển thị trước sau không bị chập nháy
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder += Mathf.Abs(GetInstanceID()) % 5;
        }
    }

    protected override void Update()
    {
        if (isTimeFrozen)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        base.Update();
    }

    public void FreezeInTime(bool freeze)
    {
        isTimeFrozen = freeze;

        if (freeze)
        {
            StopSlowDown();

            preFreezeAnimSpeed = 1f;
            preFreezeGravityScale = rb.gravityScale;

            anim.speed = 0f;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            anim.speed = preFreezeAnimSpeed > 0 ? preFreezeAnimSpeed : 1f;
            rb.gravityScale = preFreezeGravityScale > 0 ? preFreezeGravityScale : 1f;
            activeSlowMultiplier = 1f;
        }
    }


    public void MakeUntargetable(bool canBeTargeted)
    {
        if(canBeTargeted == false)
            gameObject.layer = LayerMask.NameToLayer("Untargetable");
        else
            gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    public virtual void SpecialAttack()
    {

    }

    protected override IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {

        activeSlowMultiplier = 1 - slowMultiplier;


        anim.speed = anim.speed * activeSlowMultiplier;

        yield return new WaitForSeconds(duration);
        StopSlowDown();
    }

    public override void StopSlowDown()
    {
        activeSlowMultiplier = 1;
        anim.speed = 1;
        base.StopSlowDown();
    }


    public void EnableCounterWindow(bool enable) => canBeStunned = enable;

    public virtual void ApplyStun(float duration)
    {
        if (health != null && health.isDead)
            return;

        if (stunnedState == null || stateMachine == null)
            return;

        stunnedDuration = duration;
        stateMachine.ChangeState(stunnedState);
    }

    public static event System.Action<Enemy> OnEnemyDeath;

    public override void EntityDeath()
    {
        base.EntityDeath();
        OnEnemyDeath?.Invoke(this);
        stateMachine.ChangeState(deadState);
    }

    private void HandlePlayerDeath()
    {
        stateMachine.ChangeState(idleState);
    }

    public virtual void TryEnterBattleState(Transform player)
    {
        if (stateMachine.currentState == battleState)
            return;

        if (stateMachine.currentState == attackState)
            return;

        this.player = player;
        stateMachine.ChangeState(battleState);
    }

    public void DestroyGameObjectWithDelay(float delay = 10)
    {
        Destroy(gameObject, delay);
    }

    public Transform GetPlayerReference()
    {
        if (player == null)
            player = PlayerDetected().transform;

        return player;
    }

    public RaycastHit2D PlayerDetected()
    {
        RaycastHit2D hit =
            Physics2D.Raycast(playerCheck.position, Vector2.right * facingDir, playerCheckDistance, whatIsPlayer | whatIsGround);

        if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
            return default;

        return hit;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * playerCheckDistance), playerCheck.position.y));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * attackDistance), playerCheck.position.y));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * minRetreatDistance), playerCheck.position.y));

    }

    protected virtual void OnEnable()
    {
        Player.OnPlayerDeath += HandlePlayerDeath;
    }

    protected virtual void OnDisable()
    {
       Player.OnPlayerDeath -= HandlePlayerDeath;  
    }
}
