using UnityEngine;

public class Enemy_MartialHero : Enemy, ICounterable
{
    public bool CanBeCountered => canBeStunned;

    [Header("Martial Hero - Combat Settings")]
    [SerializeField] private float attack1Distance = 1.4f;
    [SerializeField] private float attack2Distance = 1.8f;
    [SerializeField] private float comboLungeSpeed = 1.8f;
    [SerializeField] private Vector2 retreatJumpVelocity = new Vector2(4.5f, 2.5f);
    [SerializeField] private float retreatDuration = 0.35f;
    [SerializeField] private float closeRetreatDistance = 1.3f;
    [SerializeField] private DamageScaleData attack1DamageScale;
    [SerializeField] private DamageScaleData attack2DamageScale;
    private float lastTimeAttacked = float.NegativeInfinity;

    [Header("Boss Skill 2 - Kiếm Khí Trảm (Crescent Sword Wave)")]
    [SerializeField] private GameObject swordWavePrefab;
    [SerializeField] private float swordWaveCooldown = 4.0f;
    [SerializeField] private float swordWaveMinDistance = 3.0f;
    [SerializeField] private DamageScaleData swordWaveDamageScale;
    private float lastTimeSwordWave = float.NegativeInfinity;

    [Header("Boss Skill 3 - Nhất Kiếm Bạt Kích (Flash Step Dash)")]
    [SerializeField] private GameObject flashStepHitVfxPrefab;
    [SerializeField] private float flashStepCooldown = 6.0f;
    [SerializeField] private float flashStepMinDistance = 3.5f;
    [SerializeField] private float flashStepMaxDistance = 7.0f;
    [SerializeField] private DamageScaleData flashStepDamageScale;
    private float lastTimeFlashStep = float.NegativeInfinity;

    [Header("Boss Phase 2 - Thức Tỉnh Kiếm Ý (< 50% HP)")]
    [SerializeField] private bool isAwakened = false;

    public float Attack1Distance => attack1Distance;
    public float Attack2Distance => attack2Distance;
    public float BattleRunSpeed => isAwakened ? battleMoveSpeed * 1.25f : battleMoveSpeed;
    public float ComboLungeSpeed => comboLungeSpeed;
    public Vector2 RetreatVelocity => isAwakened ? retreatJumpVelocity * 1.15f : retreatJumpVelocity;
    public float RetreatDuration => retreatDuration;
    public float CloseRetreatDistance => closeRetreatDistance;
    public float SwordWaveMinDistance => swordWaveMinDistance;
    public float FlashStepMinDistance => flashStepMinDistance;
    public float FlashStepMaxDistance => flashStepMaxDistance;
    public bool IsAwakened => isAwakened;

    public bool ShouldRetreatFromPlayer()
    {
        Transform target = GetPlayerTarget();
        if (target == null)
            return false;

        float dist = Vector2.Distance(transform.position, target.position);
        return dist <= closeRetreatDistance;
    }

    public MartialHero_BattleState martialHeroBattleState { get; private set; }
    public MartialHero_AttackState martialHeroAttackState { get; private set; }
    public MartialHero_ComboAttackState martialHeroComboState { get; private set; }
    public MartialHero_RetreatState martialHeroRetreatState { get; private set; }
    public MartialHero_SwordWaveState martialHeroSwordWaveState { get; private set; }
    public MartialHero_FlashStepState martialHeroFlashStepState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "run");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "take_hit");
        deadState = new Enemy_DeadState(this, stateMachine, "death");

        martialHeroBattleState = new MartialHero_BattleState(this, stateMachine, "idle");
        battleState = martialHeroBattleState;

        martialHeroAttackState = new MartialHero_AttackState(this, stateMachine, "attack1");
        attackState = martialHeroAttackState;

        martialHeroComboState = new MartialHero_ComboAttackState(this, stateMachine, "attack2");
        martialHeroRetreatState = new MartialHero_RetreatState(this, stateMachine, "idle");
        martialHeroSwordWaveState = new MartialHero_SwordWaveState(this, stateMachine, "attack2");
        martialHeroFlashStepState = new MartialHero_FlashStepState(this, stateMachine, "run");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    [Header("Combo Chain Settings")]
    [SerializeField] private int maxComboChains = 3;
    public int currentComboChain { get; private set; } = 0;

    public bool CanAttack() => Time.time > lastTimeAttacked + (isAwakened ? attackCooldown * 0.75f : attackCooldown);
    public void SetAttackOnCooldown()
    {
        lastTimeAttacked = Time.time;
        ResetCombo();
    }

    public void IncrementCombo() => currentComboChain++;
    public void ResetCombo() => currentComboChain = 0;
    public bool CanContinueCombo() => currentComboChain < (isAwakened ? maxComboChains + 1 : maxComboChains);

    public bool CanSwordWave() => Time.time > lastTimeSwordWave + (isAwakened ? swordWaveCooldown * 0.65f : swordWaveCooldown);
    public void SetSwordWaveOnCooldown() => lastTimeSwordWave = Time.time;

    public bool CanFlashStep() => Time.time > lastTimeFlashStep + (isAwakened ? flashStepCooldown * 0.75f : flashStepCooldown);
    public void SetFlashStepOnCooldown() => lastTimeFlashStep = Time.time;

    public void UpdateBossPhase()
    {
        if (isAwakened || health == null)
            return;

        if (health.GetHealthPercent() <= 0.5f)
        {
            isAwakened = true;
            // Visual feedback: brief image echo burst
            vfx?.DoImageEchoEffect(0.6f);
        }
    }

    public void CastSwordWave()
    {
        if (swordWavePrefab == null)
            return;

        Vector3 spawnPos = transform.position + new Vector3(facingDir * 0.8f, 0.4f, 0);
        GameObject waveObj = Instantiate(swordWavePrefab, spawnPos, Quaternion.identity);
        MartialHero_SwordWave wave = waveObj.GetComponent<MartialHero_SwordWave>();
        if (wave != null)
        {
            wave.Setup(this, facingDir, swordWaveDamageScale, WhatIsPlayer, whatIsGround);
        }

        // Phase 2: Double Crescent Wave
        if (isAwakened)
        {
            StartCoroutine(CastSecondaryWaveCo());
        }
    }

    private System.Collections.IEnumerator CastSecondaryWaveCo()
    {
        yield return new WaitForSeconds(0.18f);
        if (this != null && swordWavePrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(facingDir * 0.9f, 0.5f, 0);
            GameObject waveObj = Instantiate(swordWavePrefab, spawnPos, Quaternion.identity);
            MartialHero_SwordWave wave = waveObj.GetComponent<MartialHero_SwordWave>();
            if (wave != null)
            {
                wave.Setup(this, facingDir, swordWaveDamageScale, WhatIsPlayer, whatIsGround);
            }
        }
    }

    public void PerformFlashStepHit(Transform target)
    {
        if (target == null)
            return;

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null && stats != null)
        {
            DamageScaleData scale = flashStepDamageScale != null ? flashStepDamageScale : new DamageScaleData { phyiscal = 1.5f };
            AttackData attackData = stats.GetAttackData(scale);
            damageable.TakeDamage(attackData.phyiscalDamage, attackData.elementalDamage, attackData.element, transform);
        }

        if (flashStepHitVfxPrefab != null)
        {
            Vector3 hitPos = target.position + Vector3.up * 0.8f;
            GameObject vfx = Instantiate(flashStepHitVfxPrefab, hitPos, Quaternion.identity);
            Renderer[] renderers = vfx.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.sortingLayerName = "Default";
                r.sortingOrder = 35;
            }
            Destroy(vfx, 1.2f);
        }
    }

    public SkillObject_TimeEcho FindNearbyTimeEcho(float radius = 10f)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, WhatIsPlayer);
        SkillObject_TimeEcho closestEcho = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            SkillObject_TimeEcho echo = col.GetComponent<SkillObject_TimeEcho>();
            if (echo == null)
                continue;

            Entity_Health echoHealth = echo.GetComponent<Entity_Health>();
            if (echoHealth != null && echoHealth.isDead)
                continue;

            Animator echoAnim = echo.GetComponentInChildren<Animator>();
            if (echoAnim != null && !echoAnim.gameObject.activeInHierarchy)
                continue;

            float dist = Vector2.Distance(transform.position, echo.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestEcho = echo;
            }
        }

        return closestEcho;
    }

    public Transform GetPlayerTarget()
    {
        SkillObject_TimeEcho nearbyEcho = FindNearbyTimeEcho(10f);
        if (nearbyEcho != null)
            return nearbyEcho.transform;

        if (Player.instance != null && Player.instance.health != null && !Player.instance.health.isDead)
            return Player.instance.transform;

        return GetPlayerReference();
    }

    public void PerformAttack1Damage()
    {
        PerformAttackWithScale(attack1DamageScale);
    }

    public void PerformAttack2Damage()
    {
        PerformAttackWithScale(attack2DamageScale);
    }

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
    }
}
