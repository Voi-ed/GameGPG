using System.Collections;
using UnityEngine;

public class Enemy_Reaper : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_ReaperAttackState reaperAttackState { get; private set; }
    public Enemy_ReaperBattleState reaperBattleState { get; private set; }
    public Enemy_ReaperTeleportState reaperTeleportState { get; private set; }
    public Enemy_ReaperSpellCastState reaperSpellCastState { get; private set; }

    [Header("Reaper specifics")]
    public float maxBattleIdleTime = 5;

    [Header("Reaper Spellcast")]
    [SerializeField] private DamageScaleData spellDamageScale;
    [SerializeField] private GameObject spellCastPrefab;
    [SerializeField] private int amountToCast = 6;
    [SerializeField] private float spellCastRate = 1.2f;
    [SerializeField] private float spellCastStateCooldown = 10;
    [SerializeField] private Vector2 playerOffsetPrediction;
    private float lastTimeCastedSpells = float.NegativeInfinity;
    public bool spellCastPreformed { get; private set; }
    private Player playerScript;

    [Header("Reapert Teleport")]
    [SerializeField] private BoxCollider2D arenaBounds;
    [SerializeField] private float offsetCenterY = 1.725f;
    [SerializeField] private float chanceToTeleport = .25f;
    [SerializeField] private float minTeleportDistanceFromPlayer = 3.5f;
    private float defaultTeleportChance;
    public bool teleporTrigger { get; private set; }
    private Coroutine castSpellCoroutine;

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        deadState = new Enemy_DeadState(this, stateMachine, "idle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");

        reaperBattleState = new Enemy_ReaperBattleState(this, stateMachine, "battle");
        reaperAttackState = new Enemy_ReaperAttackState(this, stateMachine, "attack");
        reaperTeleportState = new Enemy_ReaperTeleportState(this, stateMachine, "teleport");
        reaperSpellCastState = new Enemy_ReaperSpellCastState(this, stateMachine, "spellCast");

        battleState = reaperBattleState;
    }

    protected override void Start()
    {
        base.Start();

        if (arenaBounds != null)
            arenaBounds.transform.parent = null;

        defaultTeleportChance = chanceToTeleport;

        stateMachine.Initialize(idleState);
    }

    public override void TryEnterBattleState(Transform player)
    {
        // Không ngắt ngang khi Reaper đang thực hiện các đòn quan trọng hoặc đang bị choáng/chết
        if (stateMachine.currentState == reaperAttackState ||
            stateMachine.currentState == reaperSpellCastState ||
            stateMachine.currentState == reaperTeleportState ||
            stateMachine.currentState == stunnedState ||
            stateMachine.currentState == deadState)
            return;

        base.TryEnterBattleState(player);
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        StopCastingSpell();
        stateMachine.ChangeState(stunnedState);
    }

    public override void ApplyStun(float duration)
    {
        StopCastingSpell();
        base.ApplyStun(duration);
    }

    public override void EntityDeath()
    {
        StopCastingSpell();
        base.EntityDeath();
    }

    public override void SpecialAttack()
    {
        StopCastingSpell();
        castSpellCoroutine = StartCoroutine(CastSpellCo());
    }

    public void StopCastingSpell()
    {
        if (castSpellCoroutine != null)
        {
            StopCoroutine(castSpellCoroutine);
            castSpellCoroutine = null;
        }
    }

    private IEnumerator CastSpellCo()
    {
        if (playerScript == null)
        {
            Transform p = player != null ? player : (Player.instance != null ? Player.instance.transform : null);
            if (p != null)
                playerScript = p.GetComponent<Player>();
        }

        WaitForSeconds wait = new WaitForSeconds(spellCastRate);

        for (int i = 0; i < amountToCast; i++)
        {
            if (playerScript == null)
                break;

            // Chỉ dự đoán hướng khi người chơi đang di chuyển ngang (tránh nhảy/rơi tự do bị tính là đang chạy)
            bool playerMoving = Mathf.Abs(playerScript.rb.linearVelocity.x) > 0.1f;

            float xOffset = playerMoving ? playerOffsetPrediction.x * playerScript.facingDir : 0;
            Vector3 spellPosition = playerScript.transform.position + new Vector3(xOffset, playerOffsetPrediction.y);

            if (spellCastPrefab != null)
            {
                GameObject spellObj = Instantiate(spellCastPrefab, spellPosition, Quaternion.identity);
                if (spellObj.TryGetComponent<Enemy_ReaperSpell>(out var spell))
                {
                    spell.SetupSpell(combat, spellDamageScale);
                }
            }

            yield return wait;
        }

        SetSpellCastPreformed(true);
        castSpellCoroutine = null;
    }
    public void SetSpellCastPreformed(bool spellCastStatus) => spellCastPreformed = spellCastStatus;
    public bool CanDoSpellCast() => Time.time > lastTimeCastedSpells + spellCastStateCooldown;
    public void SetSpellCastOnCooldown() => lastTimeCastedSpells = Time.time;

    public bool ShouldTeleport()
    {
        if (Random.value < chanceToTeleport)
        {
            chanceToTeleport = defaultTeleportChance;
            return true;
        }

        chanceToTeleport = chanceToTeleport + .05f;
        return false;
    }

    public void SetTeleportTrigger(bool triggerStatus) => teleporTrigger = triggerStatus;

    public Vector3 FindTeleportPoint()
    {
        if (arenaBounds == null)
            return transform.position;

        int maxAttampts = 10;
        float bossWithColliderHalf = col != null ? col.bounds.size.x / 2 : 1f;
        Transform playerTarget = player != null ? player : (Player.instance != null ? Player.instance.transform : null);

        for (int i = 0; i < maxAttampts; i++)
        {
            float randomX = Random.Range(arenaBounds.bounds.min.x + bossWithColliderHalf,
                                         arenaBounds.bounds.max.x - bossWithColliderHalf);

            // Tránh teleport xuất hiện quá sát hoặc chồng đè lên người player
            if (playerTarget != null && Mathf.Abs(randomX - playerTarget.position.x) < minTeleportDistanceFromPlayer)
                continue;

            Vector2 raycastPoint = new Vector2(randomX, arenaBounds.bounds.max.y);

            RaycastHit2D hit = Physics2D.Raycast(raycastPoint, Vector2.down, Mathf.Infinity, whatIsGround);

            if (hit.collider != null)
                return hit.point + new Vector2(0, offsetCenterY);
        }

        return transform.position;
    }
}
