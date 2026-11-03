using System.Collections;
using UnityEngine;

public class Enemy_Mage : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_MageRetreatState mageRetreatState { get; private set; }
    public Enemy_MageBattleState mageBattleState { get; private set; }
    public Enemy_MageSpellCastState mageSpellCastState { get; private set; }

    [Header("Mage specifics")]
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spellStartPosition;
    [SerializeField] private int amountToCast = 3;
    [SerializeField] private float spellCastCooldown = .3f;
    public bool spellCastPerformed { get; private set; }
    [Space]
    public float spellCooldown = 4.5f;
    public float lastTimeCastSpell { get; set; } = float.NegativeInfinity;
    public bool CanCastSpell() => Time.time > lastTimeCastSpell + spellCooldown;
    [Space]
    public float retreatCooldown = 5;
    public float retreatMaxDistance = 8;
    public float retreatSpeed = 15;
    [SerializeField] private Transform behindCollsionCheck;
    [SerializeField] private bool hasRecoveryAnimation = true;

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        deadState = new Enemy_DeadState(this, stateMachine, "idle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");

        mageSpellCastState = new Enemy_MageSpellCastState(this, stateMachine, "spellCast");
        mageRetreatState = new Enemy_MageRetreatState(this, stateMachine, "battle");
        mageBattleState = new Enemy_MageBattleState(this, stateMachine, "battle");
        battleState = mageBattleState;

        anim.SetBool("hasStunRecovery", hasRecoveryAnimation);

    }

    protected override void Start()
    {
        base.Start();

        // Đảm bảo layer luôn là Enemy để Player luôn tấn công và gây sát thương được
        MakeUntargetable(true);

        // Đảm bảo hitbox đánh cận chiến của gậy phép bao trọn cự ly đánh, không bị chém hụt gió
        if (combat != null)
        {
            combat.TargetCheckRadius = Mathf.Max(combat.TargetCheckRadius, 1.25f);
        }

        stateMachine.Initialize(idleState);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        MakeUntargetable(true);
    }

    public Transform GetPlayerTarget()
    {
        if (player != null)
            return player;

        Transform detected = GetPlayerReference();
        if (detected != null)
            return detected;

        if (Player.instance != null)
            return Player.instance.transform;

        return null;
    }

    public void SetSpellCastPerformed(bool performed) => spellCastPerformed = performed;

    private Coroutine castSpellCoroutine;

    public override void SpecialAttack()
    {
        if (castSpellCoroutine != null)
            StopCoroutine(castSpellCoroutine);

        castSpellCoroutine = StartCoroutine(CastSpellCo());
    }

    public override void TryEnterBattleState(Transform player)
    {
        // Bảo vệ không ngắt ngang khi Mage đang bắn phép, đang lùi né, hoặc đang bị choáng/chết
        if (stateMachine.currentState == mageBattleState ||
            stateMachine.currentState == mageSpellCastState ||
            stateMachine.currentState == mageRetreatState ||
            stateMachine.currentState == attackState ||
            stateMachine.currentState == stunnedState ||
            stateMachine.currentState == deadState)
            return;

        base.TryEnterBattleState(player);
    }

    public void StopCastingSpell()
    {
        if (castSpellCoroutine != null)
        {
            StopCoroutine(castSpellCoroutine);
            castSpellCoroutine = null;
        }
        SetSpellCastPerformed(false);
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

    private IEnumerator CastSpellCo()
    {
        Transform target = GetPlayerTarget();
        if (target == null)
        {
            SetSpellCastPerformed(true);
            castSpellCoroutine = null;
            yield break;
        }

        for (int i = 0; i < amountToCast; i++)
        {
            if (spellPrefab == null)
                break;

            Vector3 spawnPos = spellStartPosition != null ? spellStartPosition.position : transform.position;
            GameObject spellObj = Instantiate(spellPrefab, spawnPos, Quaternion.identity);
            Enemy_MageProjectile projectile = spellObj.GetComponent<Enemy_MageProjectile>();

            if (projectile != null)
            {
                projectile.SetupProjectile(target, combat);
            }

            yield return new WaitForSeconds(spellCastCooldown);
        }

        SetSpellCastPerformed(true);
        castSpellCoroutine = null;
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        StopCastingSpell();
        stateMachine.ChangeState(stunnedState);
    }

    public bool CantMoveBackwards()
    {
        bool detectedWall = Physics2D.Raycast(behindCollsionCheck.position, Vector2.right * -facingDir, 1.5f, whatIsGround);
        bool noGround = Physics2D.Raycast(behindCollsionCheck.position, Vector2.down, 1.5f, whatIsGround) == false;

        return noGround || detectedWall;
    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawLine(behindCollsionCheck.position,
            new Vector3(behindCollsionCheck.position.x + (1.5f * -facingDir), behindCollsionCheck.position.y));

        Gizmos.DrawLine(behindCollsionCheck.position,
            new Vector3(behindCollsionCheck.position.x, behindCollsionCheck.position.y - 1.5f));
    }
}
