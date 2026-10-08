using System.Collections.Generic;
using UnityEngine;

public class SkillObject_SwordFrost : SkillObject_Sword
{
    [Header("Blizzard Vortex Settings (While Stuck)")]
    [SerializeField] private GameObject blizzardVfxPrefab;
    [SerializeField] private GameObject iceBlastVfxPrefab;
    [SerializeField] private float maxStuckDuration = 4.0f;
    [SerializeField] private float vortexRadius = 3.2f;
    [SerializeField] private float frostDamageInterval = 0.65f;
    [SerializeField] private float frostDamageMultiplier = 0.28f;

    [Header("Frost Recall Settings")]
    [SerializeField] private float recallDamageMultiplier = 0.85f;
    [SerializeField] private float recallCheckRadius = 1.2f;

    private bool isStuck;
    private float frostTimer;
    private float stuckTimer;
    private GameObject activeBlizzardVfx;
    private readonly HashSet<IDamageable> enemiesHitOnReturn = new HashSet<IDamageable>();

    public override void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        base.SetupSword(swordManager, direction);
        isStuck = false;
        stuckTimer = 0f;
        frostTimer = frostDamageInterval;
        enemiesHitOnReturn.Clear();
    }

    protected override void Update()
    {
        if (shouldComeback)
        {
            HandleRecallMovementAndDamage();
            return;
        }

        if (!isStuck)
        {
            transform.right = rb.linearVelocity;
            HandleDistanceCheck();
        }
        else
        {
            HandleBlizzardVortex();
        }
    }

    private void HandleDistanceCheck()
    {
        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) > maxAllowedDistance)
            GetSwordBackToPlayer();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (isStuck || shouldComeback)
            return;

        StopSword(collision);
        isStuck = true;
        stuckTimer = 0f;

        TriggerInitialImpact();
        SpawnBlizzardVfx();
    }

    private void TriggerInitialImpact()
    {
        // Nổ Lần 1: Khi kiếm va chạm cắm vào đất/mục tiêu
        if (iceBlastVfxPrefab != null)
        {
            Instantiate(iceBlastVfxPrefab, transform.position, Quaternion.identity);
        }

        if (playerStats == null)
            return;

        AttackData attackData = playerStats.GetAttackData(damageScaleData);
        float baseDamage = playerStats.GetBaseDamage();
        float physDmg = baseDamage * 0.25f * (damageScaleData != null ? damageScaleData.phyiscal : 1f);
        float iceDmg = Mathf.Max(baseDamage * 1.1f * (damageScaleData != null ? damageScaleData.elemental : 1f), 8f);

        foreach (var target in GetEnemiesAround(transform, 2.0f))
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(physDmg, iceDmg, ElementType.Ice, transform);
            }

            Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();
            if (statusHandler != null && attackData != null)
            {
                statusHandler.ApplyStatusEffect(ElementType.Ice, attackData.effectData);
            }
        }
    }

    private void SpawnBlizzardVfx()
    {
        if (blizzardVfxPrefab != null && activeBlizzardVfx == null)
        {
            activeBlizzardVfx = Instantiate(blizzardVfxPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    private void HandleBlizzardVortex()
    {
        stuckTimer += Time.deltaTime;
        if (stuckTimer >= maxStuckDuration)
        {
            GetSwordBackToPlayer();
            return;
        }

        frostTimer -= Time.deltaTime;
        if (frostTimer <= 0f)
        {
            frostTimer = frostDamageInterval;
            ApplyBlizzardTick();
        }
    }

    private void ApplyBlizzardTick()
    {
        if (playerStats == null)
            return;

        Collider2D[] targets = GetEnemiesAround(transform, vortexRadius);
        if (targets == null || targets.Length == 0)
            return;

        AttackData attackData = playerStats.GetAttackData(damageScaleData);
        float baseDamage = playerStats.GetBaseDamage();
        float iceDmg = Mathf.Max(baseDamage * frostDamageMultiplier * (damageScaleData != null ? damageScaleData.elemental : 1f), 3f);

        // Trong thời gian kiếm cắm: Quái KHÔNG bị stun, chỉ bị hiệu ứng Băng làm chậm (Chill) và nhận sát thương DoT
        foreach (var col in targets)
        {
            if (col == null) continue;

            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(0, iceDmg, ElementType.Ice, transform);

                Entity_StatusHandler statusHandler = col.GetComponent<Entity_StatusHandler>();
                if (statusHandler != null && attackData != null)
                {
                    statusHandler.ApplyStatusEffect(ElementType.Ice, attackData.effectData);
                }
            }
        }
    }

    private void HandleRecallMovementAndDamage()
    {
        if (activeBlizzardVfx != null)
        {
            Destroy(activeBlizzardVfx);
            activeBlizzardVfx = null;
        }

        if (transform.parent != null)
            transform.parent = null;

        if (playerTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, comebackSpeed * Time.deltaTime);

        Vector2 toPlayer = (playerTransform.position - transform.position).normalized;
        if (toPlayer.sqrMagnitude > 0.001f)
            transform.right = toPlayer;

        DamageEnemiesOnReturnPath();

        if (distance < 0.6f)
            Destroy(gameObject);
    }

    private void DamageEnemiesOnReturnPath()
    {
        if (playerStats == null)
            return;

        Collider2D[] inPath = GetEnemiesAround(transform, recallCheckRadius);
        if (inPath == null)
            return;

        AttackData attackData = playerStats.GetAttackData(damageScaleData);
        float baseDamage = playerStats.GetBaseDamage();
        float returnDmg = Mathf.Max(baseDamage * recallDamageMultiplier * (damageScaleData != null ? damageScaleData.elemental : 1f), 6f);

        foreach (var col in inPath)
        {
            if (col == null) continue;

            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null && !enemiesHitOnReturn.Contains(damageable))
            {
                enemiesHitOnReturn.Add(damageable);

                damageable.TakeDamage(baseDamage * 0.15f, returnDmg, ElementType.Ice, transform);

                Entity_StatusHandler statusHandler = col.GetComponent<Entity_StatusHandler>();
                if (statusHandler != null && attackData != null)
                    statusHandler.ApplyStatusEffect(ElementType.Ice, attackData.effectData);

                Entity enemyEntity = col.GetComponent<Entity>();
                if (enemyEntity != null)
                {
                    enemyEntity.ReciveKnockback(new Vector2(Mathf.Sign(enemyEntity.transform.position.x - transform.position.x) * 3f, 2f), 0.15f);
                }
            }
        }
    }

    public override void GetSwordBackToPlayer()
    {
        // Nổ Lần 2: Khi rút kiếm về (kiếm bật tung ra khỏi đất nổ băng)
        if (isStuck && iceBlastVfxPrefab != null)
        {
            Instantiate(iceBlastVfxPrefab, transform.position, Quaternion.identity);
        }

        base.GetSwordBackToPlayer();
        isStuck = false;
        if (rb != null)
            rb.simulated = false;
        if (transform.parent != null)
            transform.parent = null;

        if (activeBlizzardVfx != null)
        {
            Destroy(activeBlizzardVfx);
            activeBlizzardVfx = null;
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, vortexRadius);
    }
}
